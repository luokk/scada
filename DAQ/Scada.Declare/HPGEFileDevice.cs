using Scada.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Scada.Declare
{
    public class HPGEFileDevice : Device
    {
        private bool isVirtual = false;

        private bool isOpen = false;

        private DeviceEntry entry = null;

        private Timer timer = null;

        private string strFileMonitoringPath;

        private string strFileCopy2Path;

        private string strSidPath;

        private string SID;

        private string strActionInterval;

        private string insertIntoCommand;

        public HPGEFileDevice(DeviceEntry entry)
        {
            this.entry = entry;
            this.Initialize(entry);
        }

        ~HPGEFileDevice()
        {
        }

        // Initialize the device
        private void Initialize(DeviceEntry entry)
        {
            this.Name = entry[DeviceEntry.Name].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
            this.Version = entry[DeviceEntry.Version].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();

            this.strFileMonitoringPath = (StringValue)entry["FileMonitoringPath"];
            this.strFileCopy2Path = (StringValue)entry["FileCopy2Path"];
            this.strSidPath = (StringValue)entry["SIDPath"];
            this.strActionInterval = (StringValue)entry[DeviceEntry.ActionInterval];

            string tableName = (StringValue)entry[DeviceEntry.TableName];
            string tableFields = (StringValue)entry[DeviceEntry.TableFields];
            this.InitializeHPGeTable(tableName, tableFields, out this.insertIntoCommand);
        }

        private void InitializeHPGeTable(string tableName, string tableFields, out string insertIntoCommand)
        {
            string[] fields = tableFields.Split(',');
            string atList = string.Empty;
            for (int i = 0; i < fields.Length; ++i)
            {
                string at = string.Format("@{0}, ", i + 1);
                atList += at;
            }
            atList = atList.TrimEnd(',', ' ');
            string cmd = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
            insertIntoCommand = cmd;
        }

        public bool IsVirtual
        {
            get { return this.isVirtual; }
        }

        private bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        /// <summary>
        /// 
        /// Ignore the address parameter
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool Connect(string address)
        {
            bool connected = true;

            this.timer = new Timer(new TimerCallback(TimerCallback), null, 1000, int.Parse(strActionInterval) * 1000);
            this.isOpen = true;

            return connected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        private void TimerCallback(object o)
        {
            if (!File.Exists(strSidPath))
            {
                RecordManager.DoSystemEventRecord(this, "no SID exsiting!", RecordType.Error);
                return;
            }
            
            this.SID = File.ReadAllText(strSidPath);
            if (this.SID == "")
            {
                RecordManager.DoSystemEventRecord(this, "no date in SID file!", RecordType.Error);
                return;
            }

            string DES = strFileCopy2Path + "\\" + this.SID + "\\";
            if (Directory.Exists(DES) == false)
            {
                Directory.CreateDirectory(DES);
            }

            string originPath = strFileMonitoringPath + "\\";
            if (Directory.Exists(originPath) == false)
            {
                Directory.CreateDirectory(originPath);
            }

            foreach (string vFile in Directory.GetFiles(originPath))
            {
                string filename = Path.GetFileName(vFile);
                filename = filename.ToLower();
                if (File.GetLastWriteTime(vFile) >= DateTime.Now.AddSeconds(-150000)) // 如果文件是在150秒内修改
                {
                    if (filename.Contains("qaspectra.spe"))
                    {
                        string NewSpecName = "qaspectra" + DateTime.Now.ToString("_yyyy_MM_ddTHH_mm_ss") + ".spe";
                        File.Move(vFile, DES + "!" + NewSpecName);

                        this.Record(NewSpecName);
                    }
                    else if (filename.Contains("qareport.rpt"))
                    {
                        string NewReportName = "qareport" + DateTime.Now.ToString("_yyyy_MM_ddTHH_mm_ss") + ".spe";
                        File.Move(vFile, DES + "!" + NewReportName);

                        this.Record(NewReportName);
                    }
                    else if (filename.Contains("samplespectra2-"))
                    {
                        int index = filename.LastIndexOf(".");
                        string newfilenameA = filename.Substring(0, index) + DateTime.Now.ToString("_yyyy_MM_ddTHH_mm_ss") + ".spe";
                        File.Move(vFile, DES + "!" + newfilenameA);

                        this.Record(newfilenameA);
                    }
                    else if (filename.Contains("samplespectra24.spe"))
                    {
                        string newfilenameB = "samplespectra24" + DateTime.Now.ToString("_yyyy_MM_ddTHH_mm_ss") + ".spe";
                        File.Move(vFile, DES + "!" + newfilenameB);

                        this.Record(newfilenameB);
                    }
                    else if (filename.Contains("samplereport24.rpt"))
                    {
                        string newfilenameC = "samplereport24" + DateTime.Now.ToString("_yyyy_MM_ddTHH_mm_ss") + ".rpt";
                        try
                        {
                            File.Move(vFile, DES + "!" + newfilenameC);
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(10000);
                            File.Move(vFile, DES + "!" + newfilenameC);
                        }

                        this.Record(newfilenameC);
                    }
                }
            }
        }

        public override void Start(string address)
        {
            this.Connect(address);
        }

        public override void Stop()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
            isOpen = false;
        }

        public override void Send(byte[] action, DateTime time)
        {
        }

        private void Record(string str)
        {
            DateTime time = DateTime.Now;

            object[] data = new object[]{ time, this.SID, str};

            DeviceData dd = new DeviceData(this, data);
            dd.InsertIntoCommand = this.insertIntoCommand;

            this.SynchronizationContext.Post(this.DataReceived, dd);
        }

        public override bool OnReceiveData(byte[] line)
        {
            return false;
        }
    }
}

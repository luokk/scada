using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Declare;
using System.Timers;
using Scada.Config;
using System.Runtime.InteropServices;
using OPC.Data;
using System.Windows.Forms;
using OPC.Data.Interface;
using System.IO;

namespace Scada.Device.Siemens
{
    public class OpcDevice : Scada.Declare.Device
    {
        public class HandleCode
        {
            public HandleCode(int handle, string code)
            {
                this.Handle = handle;
                this.Code = code;
            }

            public int Handle { get; set; }
            public string Code { get; set; }
        }

        protected string deviceKey;

        private OpcServer server;

        private OpcGroup group;

        private bool connected = false;

        private bool start = false;

        private System.Windows.Forms.Timer timer = null;

        private string ipAddr;

        private OPCItemResult[] results;

        private DateTime lastRecordTime;

        private string Flow
        {
            get;
            set;
        }

        private string Hours
        {
            get;
            set;
        }

        private const string TSAP = ":1001:1001,";

        private string tsap = TSAP;

        private string serverId;

        private string insertSQL;

        private DateTime beginTime;

        private DateTime endTime;

        private DateTime latestTime;

        private string tableName;

        public OpcDevice(DeviceEntry entry)
        {
            this.Initialize(entry);
        }

        private void Initialize(DeviceEntry entry)
        {
            this.Name = entry[DeviceEntry.Name].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
            this.Version = entry[DeviceEntry.Version].ToString();

            this.ipAddr = entry["IPADDR"].ToString();

            this.Flow = entry["Flow"].ToString();
            this.Hours = entry["Hours"].ToString();

            this.tableName = (StringValue)entry[DeviceEntry.TableName];
            if (!string.IsNullOrEmpty(tableName))
            {
                string tableFields = (StringValue)entry[DeviceEntry.TableFields];
                this.insertSQL = this.MakeInsertSQL(this.tableName, tableFields);
            }
            
            IValue v = entry["TSAP"];
            if (v != null)
            {
                this.tsap = v.ToString();
            }

        }

        private void PutDeviceFile(bool running)
        {
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (!Directory.Exists(statusPath))
            {
                Directory.CreateDirectory(statusPath);
            }

            string relFileName = string.Format("status\\@{0}-running", this.deviceKey);
            string fileName = ConfigPath.GetConfigFilePath(relFileName);

            if (running)
            {
                try
                {
                    using (File.Create(fileName))
                    {
                    }
                }
                catch (Exception)
                { }
            }
            else
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception)
                {

                }
            }
        }

        public override void Start(string address)
        {
            this.server = new OpcServer();
            this.serverId = address;
        }

        public override void Stop()
        {
            this.server = null;
        }

        public override void Send(byte[] action, DateTime time)
        {
            string cmd = Encoding.UTF8.GetString(action);
            RecordManager.DoSystemEventRecord(this, string.Format("CMD={0}", cmd), RecordType.Event, true);

            if (cmd.IndexOf("connect") >= 0)
            {   
                this.Connect();
            }
            else if (cmd.IndexOf("disconnect") >= 0)
            {
                this.Disconnect();
            }
            else if (cmd.IndexOf("start") >= 0)
            {
                int b = cmd.IndexOf("Sid=");
                if (b > 0)
                {
                    int length = cmd.IndexOf(";", b) - b - 4;
                    if (length > 0)
                    {
                        this.Sid = cmd.Substring(b + 4, length);
                    }
                    else
                    {
                        DateTime n = DateTime.Now;
                        this.Sid = string.Format("SID-{0}", n.ToString("yyyyMMdd-HHmmss"));
                    }
                }
                else
                {
                    DateTime n = DateTime.Now;
                    this.Sid = string.Format("SID-{0}", n.ToString("yyyyMMdd-HHmmss"));
                }
                RecordManager.DoSystemEventRecord(this, string.Format("Start SID={0}", this.Sid), RecordType.Event, true);

                this.Start();
            }
            else if (cmd.IndexOf("stop") >= 0)
            {
                this.StopDevice();
            }
            else if (cmd == "reset")
            {
                this.Reset();
            }
            
        }

        private void Connect()
        {
            try
            {
                this.server.Connect(this.serverId);

                RecordManager.DoSystemEventRecord(this, "Connected to CPU", RecordType.Event, true);

                this.group = this.server.AddGroup("Group1", true, 2000);

                this.OnConnect();

                this.Write(new HandleCode(1, this.Flow), new HandleCode(2, this.Hours));
                this.connected = true;

                this.timer = new System.Windows.Forms.Timer();
                this.timer.Interval = 3000;
                this.timer.Tick += this.OnDataTimer;
                this.timer.Start();
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Connect:{0}", e.Message), RecordType.Error);
            }
        }


        private void Start()
        {
            if (this.connected)
            {
                this.beginTime = default(DateTime); // HERE, re-init
                this.Write(new HandleCode(13, "2"));
                this.start = true;
                this.PutDeviceFile(true);
                RecordManager.DoSystemEventRecord(this, string.Format("Start SID={0}", this.Sid), RecordType.Event, true);
            }
        }

        private bool stopping = false;

        private void StopDevice()
        {
            if (this.connected)
            {
                this.PutDeviceFile(false);
                // this.MarkEndTime(); HERE 应该在真正停止后在update db
                this.Write(new HandleCode(13, "4"));
                this.stopping = true;
                RecordManager.DoSystemEventRecord(this, string.Format("Stopping SID={0}", this.Sid), RecordType.Event, true);
            }
        }

        private void Reset()
        {
            if (this.connected)
            {
                this.Write(new HandleCode(13, "1"));
                RecordManager.DoSystemEventRecord(this, string.Format("Reset"), RecordType.Event, true);
            }
        }


        /// <summary>
        /// Functional write data on channel
        /// </summary>
        /// <param name="hc"></param>
        private void Write(params HandleCode[] hc)
        {
            int[] h = hc.Select((i) => i.Handle).ToArray();
            string[] c = hc.Select((i) => i.Code).ToArray();
            try
            {
                int[] errors;
                if (this.group.SyncWrite(h, c, out errors))
                {
                    RecordManager.DoSystemEventRecord(this, "Write Succeeded", RecordType.Event, true);
                }
                else
                {
                    RecordManager.DoSystemEventRecord(this, string.Format("Write Failed: {0} = {1}", string.Join(", ", h), string.Join(", ", c)), RecordType.Error, true);
                }
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Write Exception: {0} = {1}, {2}", string.Join(", ", h), string.Join(", ", c), e.Message), RecordType.Error, true);
            }
        }

        private void OnDataTimer(object sender, EventArgs e)
        {
            int[] handles = this.results.Select((r) => r.HandleServer).ToArray();
            OPCItemState[] states;
            if (this.group.SyncRead(OPCDATASOURCE.OPC_DS_DEVICE, handles, out states))
            {
                if (this.start)
                {
                    // Start
                    DateTime time;
                    if (this.OnRightTime(out time))
                    {
                        if (time == this.lastRecordTime)
                        {
                            return;
                        }

                        this.lastRecordTime = time;

                        if (this.beginTime == default(DateTime))
                        {
                            this.beginTime = time;
                        }

                        object[] values = states.Select((s) => s.DataValue).ToArray();
                        string valueLine = string.Join(", ", values);
                        RecordManager.DoSystemEventRecord(this, valueLine, RecordType.Origin, true);

                        this.latestTime = time;
                        string status = values[6].ToString();
                        RecordManager.DoSystemEventRecord(this, string.Format("STATUS:{0}", status), RecordType.Event, true);
                        if (this.stopping && status == "0")
                        {
                            this.endTime = time;
                            this.stopping = false;
                            this.start = false;
                            RecordManager.DoSystemEventRecord(this, string.Format("Stopped SID={0}", this.Sid), RecordType.Event, true);
                            this.PutDeviceFile(false);
                        }
                        byte statusb = (status == "1") ? (byte)1 : (byte)0;
                        object[] data = new object[] { time, this.Sid, this.beginTime, this.endTime, values[3], values[4], values[5], statusb, 0, 0, 0 };
                        DeviceData deviceData = new DeviceData(this, data);
                        deviceData.InsertIntoCommand = this.insertSQL;
                        RecordManager.DoDataRecord(deviceData);

                        // HERE
                        if (this.start)
                        {
                            this.MarkEndTime(this.endTime);
                        }
                    }
                }
                else
                {
                    // Not start
                    object[] values = states.Select((s) => s.DataValue).ToArray();
                    string status = values[6].ToString();
                    if (status == "0")
                    {
                        this.start = false;
                    }
                    else if (status == "1")
                    {
                        this.start = true;
                    }
                }
            }
            else
            {
                RecordManager.DoSystemEventRecord(this, "Read Faild", RecordType.Event, true);
            }
        }

        private bool OnRightTime(out DateTime rightTime)
        {
            return Scada.Declare.Device.At30Sec(DateTime.Now, out rightTime);
        }

        private void Disconnect()
        {
            try
            {
                this.connected = false;
                this.server.Disconnect();
                RecordManager.DoSystemEventRecord(this, string.Format("Disconnect"), RecordType.Event, true);

                this.timer.Stop();
                this.timer = null;
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Disconnect:{0}", e.Message), RecordType.Error);
            }
        }

        private void MarkEndTime(DateTime endTime)
        {
            DeviceData deviceData = new DeviceData(this, new object[]{});
            deviceData.InsertIntoCommand = string.Format("update {0} set EndTime='{1}' where time='{2}'", this.tableName, endTime, endTime);
            RecordManager.DoDataRecord(deviceData);
        }

        public override bool OnReceiveData(byte[] line)
        {
            return true;
        }

        protected void OnConnect()
        {
            List<OPCItemDef> items = new List<OPCItemDef>();
            string[] codes =
            {
                "VW134,WORD",
                "VD114,REAL",
                "VW159,WORD",
                "VD34,REAL",
                "VD54,REAL",
                "VD74,REAL",
                "VB485,BYTE",
                "VW313,WORD",
                "VW890,WORD",
                "A0.1,BOOL",
                "A0.2,BOOL",
                "A1.0,BOOL",
                "VB490,BYTE",
                "E0.3,BOOL",
                "AEW4,WORD",
                "AEW6,WORD",
                "A0.6,BOOL"
            };
            int i = 0;
            foreach (var code in codes)
            {
                i++;
                OPCItemDef item = new OPCItemDef(this.Code(code), true, i, VarEnum.VT_EMPTY);
                items.Add(item);
            }

            if (this.group.AddItems(items.ToArray(), out this.results))
            {
                RecordManager.DoSystemEventRecord(this, string.Format("AddItems OK"), RecordType.Event, true);
                int[] data = this.results.Select((r) => r.HandleServer).ToArray();

                RecordManager.DoSystemEventRecord(this, string.Join(", ", data), RecordType.Event, true);

            }
            else
            {
                RecordManager.DoSystemEventRecord(this, string.Format("AddItems Failed"), RecordType.Error, true);

            }
        }

        public override bool Running
        {
            get
            {
                return this.start;
            }
            set 
            {
                this.start = value;
            }
        }

        private string MakeInsertSQL(string tableName, string tableFields)
        {
            string[] fields = tableFields.Split(',');
            string atList = string.Empty;
            for (int i = 0; i < fields.Length; ++i)
            {
                string at = string.Format("@{0}, ", i + 1);
                atList += at;
            }
            atList = atList.TrimEnd(',', ' ');
            string sql = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
            return sql;
        }

        protected string Code(string code)
        {
            return string.Format("2:{0}{1}{2}", this.ipAddr, this.tsap, code);
        }

        public object Sid { get; set; }
    }


    public class MDSDevice : OpcDevice
    {
        public MDSDevice(DeviceEntry entry)
            :base(entry)
        {
            this.deviceKey = "scada.mds";
        }

    }

    public class AISDevice : OpcDevice
    {
        public AISDevice(DeviceEntry entry)
            : base(entry)
        {
            this.deviceKey = "scada.ais";
        }

    }

}

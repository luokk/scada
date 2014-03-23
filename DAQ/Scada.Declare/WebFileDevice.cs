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
	public class WebFileDevice : Device
	{
		private bool isVirtual = false;

		private bool isOpen = false;

		private DeviceEntry entry = null;

		private Timer timer = null;

		private string addr = "127.0.0.1";

        private string insertIntoCommand;

        private string insertIntoCommand2;

		private string deviceSn = string.Empty;

		private int minuteAdjust = 0;

        private const int TimeZone = 8;

        // private int index = 0;

		public WebFileDevice(DeviceEntry entry)
		{
            this.entry = entry;
			this.Initialize(entry);
		}

        ~WebFileDevice()
        {
        }

		// Initialize the device
		private void Initialize(DeviceEntry entry)
		{
			this.Name = entry[DeviceEntry.Name].ToString();
			this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
			this.Version = entry[DeviceEntry.Version].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();

			string deviceSn = entry[DeviceEntry.DeviceSn].ToString();
			if (string.IsNullOrEmpty(deviceSn))
			{
				throw new Exception("Config Error: No Device SN");
			}
			this.deviceSn = deviceSn.Trim();

			this.addr = (StringValue)entry[DeviceEntry.IPAddress];

			this.minuteAdjust = (StringValue)entry["MinuteAdjust"];


            string tableName = (StringValue)entry[DeviceEntry.TableName];
            string tableFields = (StringValue)entry[DeviceEntry.TableFields];
            this.InitializeNaITable(tableName, tableFields, out this.insertIntoCommand);

            string tableName2 = (StringValue)entry["TableName2"];
            string tableFields2 = (StringValue)entry["TableFields2"];
            this.InitializeNaITable(tableName2, tableFields2, out this.insertIntoCommand2);


            // Virtual On
            string isVirtual = (StringValue)entry[DeviceEntry.Virtual];
            if (isVirtual != null && isVirtual.ToLower() == "true")
            {
                this.isVirtual = true;
            }
		}

        private void InitializeNaITable(string tableName, string tableFields, out string insertIntoCommand)
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

			this.timer = new Timer(new TimerCallback(TimerCallback), null, 1000, 1000 * 30);
            this.isOpen = true;
			
			return connected;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
		private void TimerCallback(object o)
		{
            // The temp file name is fixed.
            DateTime now = DateTime.Now;
            string tempFile = this.DeviceConfigPath + "\\temp_download_file.xml";
            string filePath = string.Empty;
            if (this.IsVirtual)
            {
                filePath = this.DeviceConfigPath + "/sara0231_2012-09-01T03_50_00Z-5min.n42";

                File.Copy(filePath, tempFile, true);
            }
            else
            {
                // Start download ...
                string fileName1 = GetFileNameOnDevice(now);
                string fileName2 = GetFileName(now);
                string datePath = LogPath.GetDeviceLogFilePath("scada.naidevice", now);
                this.DoFolderPolicy(datePath);

                filePath = datePath + "\\" + fileName2;
				if (File.Exists(filePath))
                {
                    return;
                }

                // Download the file.
                string address = this.addr + fileName1;
                
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.Credentials = new NetworkCredential("root", "root");
                        // DO NOT USE the FOLLOWING LINE
                        // client.UseDefaultCredentials = true;
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        } 
                        client.DownloadFile(address, tempFile);
                    }
                    catch (Exception e)
                    {
                        RecordManager.DoSystemEventRecord(this, string.Format("{0} Try to download {1}: Failed.", now, address));
                        RecordManager.DoSystemEventRecord(this, e.Message);
                    }
                }
            }
			Thread.Sleep(1000);
            if (File.Exists(tempFile))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(tempFile);
                    
                    // XmlElement root = doc.DocumentElement;
                    var nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("a", "http://physics.nist.gov/Divisions/Div846/Gp4/ANSIN4242/2005/ANSIN4242");
                    nsmgr.AddNamespace("s", "http://www.technidata.com/ENVINET/SARA");
                    nsmgr.AddNamespace("e", "http://www.technidata.com/ENVINET");

                    NuclideDataSet set = this.ParseData(doc, nsmgr);
                    this.Record(set, now);

                    File.Move(tempFile, filePath);
                }
                catch (IOException e)
                {
                    RecordManager.DoSystemEventRecord(this, e.Message);
                }
                catch (Exception e)
                {
                    RecordManager.DoSystemEventRecord(this, e.Message);
                }
                finally
                {
                    // TODO: Gzip the file, put it into the Date-folder, then delete this xml file.
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

        private void Record(NuclideDataSet set, DateTime now)
        {
            DateTime time = DateTime.Parse(set.EndTime);
            double minutesAdjust = this.minuteAdjust / 5 * 5 - 5;
            time = time.AddMinutes(-minutesAdjust);

            var dd = this.ParseNaI(set, time);
            this.SynchronizationContext.Post(this.DataReceived, dd);
            foreach (var nd in set.sets)
            {
                if (nd.Indication == "100")
                {
                    var dd2 = this.ParseNuclideData(nd, time);
                    this.SynchronizationContext.Post(this.DataReceived, dd2);
                }
            }
        }

        private DeviceData ParseNaI(NuclideDataSet s, DateTime time)
        {
            object[] data = new object[]{ time,
                time.AddMinutes(-5) , time, s.Coefficients, 
                s.ChannelData, s.DoseRate, s.Temperature, s.HighVoltage, 
                s.CalibrationNuclideFound, 
                s.ReferencePeakEnergyFromPosition
            };
            DeviceData dd = new DeviceData(this, data);
            dd.InsertIntoCommand = this.insertIntoCommand;
            return dd;
        }

        private DeviceData ParseNuclideData(NuclideData nd, DateTime time)
        {
            object[] data = new object[]{
                time, nd.Name, nd.Activity, nd.Indication, nd.DoseRate, nd.Channel, nd.Energy
            };
            DeviceData dd = new DeviceData(this, data);
            dd.InsertIntoCommand = this.insertIntoCommand2;
            return dd;
        }

        /// <summary>
        /// index = 0, min = 0
        /// index = 1, min = 5
        /// ...
        /// index = 11, min = 55
        /// Returns this hour the Nth file by index.
        /// sara0240_2013-01-19T06_05_00Z-5min.n42
        /// </summary>
        /// <param name="min"></param>
        /// <returns></returns>
        private string GetFileNameOnDevice(DateTime now)
        {
            string fileName;
            DateTime t = now;
            t = t.AddHours(-TimeZone).AddMinutes(this.minuteAdjust);
            fileName = string.Format("{0}_{1}-{2:D2}-{3:D2}T{4:D2}_{5:D2}_00Z-5min.n42",
				this.deviceSn, t.Year, t.Month, t.Day, t.Hour, t.Minute / 5 * 5);
            return fileName;
        }

        private string GetFileName(DateTime now)
        {
            string fileName;
            DateTime t = now;
            fileName = string.Format("{0}_{1}-{2:D2}-{3:D2}T{4:D2}_{5:D2}_00-5min.n42",
                this.deviceSn, t.Year, t.Month, t.Day, t.Hour, t.Minute / 5 * 5);
            return fileName;
        }


        private string GetDatePath(DateTime date)
        {
            return string.Format("{0}-{1:D2}", date.Year, date.Month);
        }

		// TODO: CalibrationNuclideFound and ReferencePeakEnergyFromPosition
        private NuclideDataSet ParseData(XmlDocument doc, XmlNamespaceManager nsmgr)
        {
            // 
            string st = doc.Value("//a:Spectrum/a:StartTime", nsmgr);
            // Basicly, we use the EndTime.
            string et = doc.Value("//s:EndTime", nsmgr);

            string co = doc.Value("//a:Coefficients", nsmgr);

            string cd = doc.Value("//a:ChannelData", nsmgr);

            string dr = doc.Value("//a:DoseRate", nsmgr);
            if (dr != null && dr.Length > 0)
            {
                double v;
                if (double.TryParse(dr, out v))
                {
                    dr = (v * 1000).ToString();
                }
                else
                {
                    dr = "0.0 (ERR)";
                }
            }
            string tp = doc.Value("//s:Temperature", nsmgr);
            string hv = doc.Value("//s:HighVoltage", nsmgr);

            // 参考核素状态
            string ns = doc.Value("//s:CalibrationNuclideFound", nsmgr);
            // 参考核素能量
            string ne = doc.Value("//s:ReferencePeakEnergyFromPosition", nsmgr);

            NuclideDataSet set = new NuclideDataSet();
            set.StartTime = st;
            set.EndTime = et;
            set.Coefficients = co;
            set.ChannelData = cd;
            set.DoseRate = dr;
            set.Temperature = tp;
            set.HighVoltage = hv;
            set.CalibrationNuclideFound = (bool)(ns != "false");
            set.ReferencePeakEnergyFromPosition = ne;

            XmlNodeList list = doc.SelectNodes("//a:Nuclide", nsmgr);

            foreach (XmlNode n in list)
            {
                string nn = n.Value("a:NuclideName", nsmgr);

                string ni = n.Value("a:NuclideIDConfidenceIndication", nsmgr);

                string na = n.Value("a:NuclideActivity", nsmgr);

                string nd = n.Value("s:DoseRate", nsmgr);
                if (nd != null && nd.Length > 0)
                {
                    double v;
                    if (double.TryParse(nd, out v))
                    {
                        nd = (v * 1000).ToString();
                    }
                    else
                    {
                        nd = "0.0 (ERR)";
                    }
                }


                string ch = string.Empty;
                string en = string.Empty;

                var peak = n.SelectSingleNode("s:Peak", nsmgr);
                if (peak != null)
                {
                    foreach (XmlAttribute attr in peak.Attributes)
                    {
                        string attrName = attr.Name.ToLower();
                        if (attrName == "channel")
                        {
                            ch = attr.Value;
                        }
                        else if (attrName == "energy")
                        {
                            en = attr.Value;
                        }
                    }
                }

                NuclideData data = new NuclideData() { 
                    Name = nn, Activity = na, Indication = ni, DoseRate = nd,
                    Channel = ch, Energy = en
                };
                set.AddNuclideData(data);
            }

            return set;
        }

        private void DoFolderPolicy(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            DateTime threeMonthAgo = DateTime.Now.AddMonths(-3);
            string threeMonthAgoPath = this.DeviceConfigPath + "\\" + this.GetDatePath(threeMonthAgo);

            if (Directory.Exists(threeMonthAgoPath))
            {
                Directory.Delete(threeMonthAgoPath, true);
            }
        }

	}
}

using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Scada.Data.Client.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    public class Settings : ISettings
    {
        /// <summary>
        /// 
        /// </summary>
        /// !
        /// 
        public const string DeviceKey_Hpic = "scada.hpic";

        public const string DeviceKey_Weather = "scada.weather";

        public const string DeviceKey_MDS = "scada.mds";

        public const string DeviceKey_AIS = "scada.ais";

        public const string DeviceKey_Shelter = "scada.shelter";

        public const string DeviceKey_Dwd = "scada.dwd";

        public const string DeviceKey_NaI = "scada.naidevice";


        public string[] DeviceKeys = {
                                DeviceKey_Hpic, 
                                DeviceKey_Weather, 
                                DeviceKey_MDS, 
                                DeviceKey_AIS, 
                                DeviceKey_Shelter,
                                DeviceKey_Dwd,  
                                DeviceKey_NaI
                                     };

        public static Settings Instance = new Settings();

        /// <summary>
        /// 
        /// </summary>
        public class DataCenter
        {
            public string Ip { get; set; }
            public string WirelessIp { get; set; }

            public int Port { get; set; }
            public int WirelessPort { get; set; }


            public bool CountryCenter { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DeviceCode
        {
            public string Code
            {
                get;
                set;
            }

            public string Field
            {
                get;
                set;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class Device
        {
            public string TableName
            {
                get;
                set;
            }

            private List<DeviceCode> codes = new List<DeviceCode>();


            public void AddCode(string code, string field)
            {
                this.codes.Add(new DeviceCode() { Code = code, Field = field });
            }


            public string Key { get; set; }

            public string EquipNumber { get; set; }

            internal List<DeviceCode> GetCodes()
            {
                return this.codes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private XmlDocument doc = new XmlDocument();

        private List<DataCenter> dataCenters = new List<DataCenter>();

        private List<Device> devices = new List<Device>();

        public Settings()
        {
            // "Agent.Settings"
            string settingFileName = ConfigPath.GetConfigFilePath("agent.settings"); 
            if (File.Exists(settingFileName))
            {
                doc.Load(settingFileName);
            }

            // Code for quick test.
            // AddNewIpAddress("", "", "", "", true);

            // Data Center
            var datacenters = doc.SelectNodes("//datacenter");
            foreach (XmlNode dcn in datacenters)
            {
                DataCenter dc = new DataCenter();
                
                dc.Ip = this.GetAttribute(dcn, "ip");
                dc.Port = int.Parse(this.GetAttribute(dcn, "port", "0"));
                dc.WirelessIp = this.GetAttribute(dcn, "wirelessip");
                dc.WirelessPort = int.Parse(this.GetAttribute(dcn, "wirelessport", "0"));
                dc.CountryCenter = this.GetAttribute(dcn, "type", "1") == "2"; 
                dataCenters.Add(dc);
            }

            // Site
            var siteNode = doc.SelectNodes("//site")[0];
            this.SysName = this.GetAttribute(siteNode, "sysname");
            this.SysSt = this.GetAttribute(siteNode, "sysst");
            this.Mn = this.GetAttribute(siteNode, "mn");
            this.Sno = this.GetAttribute(siteNode, "sno");

            this.LoadPassword();
            
            // Devices
            var devices = doc.SelectNodes("//devices/device");
            foreach (XmlNode deviceNode in devices)
            {
                Device device = this.ParseDeviceNode(deviceNode);
                this.devices.Add(device);
                var codes = deviceNode.SelectNodes("code");
                foreach (XmlNode codeNode in codes)
                {
                    string code = codeNode.InnerText;
                    XmlNode fieldNode = codeNode.Attributes.GetNamedItem("field");
                    if (fieldNode != null)
                    {
                        device.AddCode(code, fieldNode.Value);
                    }
                }
            }


            // Load NaI device config.
            // TODO:

            // this.NaIFilePath = string.Format("{0}\\..\\devices\\Scada.NaIDevice\\0.9", Application.ExecutablePath);

            

            const string NaIDeviceKey = "scada.naidevice";
            DeviceEntry entry = LoadFromConfig(NaIDeviceKey, ConfigPath.GetDeviceConfigFilePath(NaIDeviceKey, "0.9"));
            if (entry != null)
            {
                this.NaIDeviceSn = (StringValue)entry["DeviceSn"];
                this.MinuteAdjust = (StringValue)entry["MinuteAdjust"];
            }
        }

        private Device ParseDeviceNode(XmlNode deviceNode)
        {
            var tableNameNode = deviceNode.Attributes.GetNamedItem("table");
            string tableName = string.Empty;
            if (tableNameNode != null)
            {
                tableName = tableNameNode.Value;
            }

            var idNode = deviceNode.Attributes.GetNamedItem("id");
            string deviceKey = string.Empty;
            if (idNode != null)
            {
                deviceKey = idNode.Value;
            }

            var equipNode = deviceNode.Attributes.GetNamedItem("eno");
            string equipNumber = string.Empty;
            if (equipNode != null)
            {
                equipNumber = equipNode.Value;
            }

            Device device = new Device();
            device.TableName = tableName;
            device.Key = deviceKey;
            device.EquipNumber = equipNumber;
            return device;
        }

        public List<DataCenter> DataCenters
        {
            get
            {
                return this.dataCenters;
            }
        }

        internal string GetTableName(string deviceKey)
        {
            Device device = devices.Find((d) => { return deviceKey.Equals(d.Key, StringComparison.OrdinalIgnoreCase)  ; });
            if (device != null)
            {
                return device.TableName;
            }
            return string.Empty;
        }

        internal string GetEquipNumber(string deviceKey)
        {
            Device device = devices.Find((d) => { return deviceKey.Equals(d.Key, StringComparison.OrdinalIgnoreCase); });
            if (device != null)
            {
                return device.EquipNumber;
            }
            return string.Empty;
        }

        internal List<DeviceCode> GetCodes(string deviceKey)
        {
            Device device = devices.Find((d) => { return deviceKey.Equals(d.Key, StringComparison.OrdinalIgnoreCase); });
            if (device != null)
            {
                return device.GetCodes();
            }
            return new List<DeviceCode>();
        }

        private string GetAttribute(XmlNode node, string attr)
        {
            var xmlAttr = node.Attributes.GetNamedItem(attr);
            return xmlAttr.Value; 
        }

        private string GetAttribute(XmlNode node, string attr, string defaultValue = "")
        {
            var xmlAttr = node.Attributes.GetNamedItem(attr);
            if (xmlAttr != null)
            {
                if (!string.IsNullOrEmpty(xmlAttr.Value))
                {
                    return xmlAttr.Value;
                }
            }
            return defaultValue;
        }

        private string password = string.Empty;

        public string Password
        {
            get
            {
                if (this.password == string.Empty)
                {
                    this.LoadPassword();
                }
                return password;
            }

            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    this.UpdatePassword(value);
                }
            }

        }

        private string GetPasswordFile()
        {
            return ConfigPath.GetConfigFilePath("password");
        }

        private void LoadPassword()
        {
            using (StreamReader sr = new StreamReader(GetPasswordFile()))
            {
                this.password = sr.ReadLine();
            }
        }

        private void UpdatePassword(string password)
        {
            File.Delete(GetPasswordFile());
            using (StreamWriter sw = new StreamWriter(GetPasswordFile()))
            {
                sw.WriteLine(password);
            }
        }

        public string SysName
        {
            get;
            private set;
        }

        public string SysSt
        {
            get;
            private set;
        }

        public string Mn
        {
            get;
            set;
        }

        public string Sno
        {
            get;
            private set;
        }


        public DateTime CurrentTime
        {
            set { }
            get
            {
                return DateTime.Now;
            }
        }

        internal string GetDeviceKeyByEno(string eno)
        {
            Device device = devices.Find((d) => { return eno.Equals(d.EquipNumber, StringComparison.OrdinalIgnoreCase); });
            if (device != null)
            {
                return device.Key;
            }
            return string.Empty;
        }

        public static DeviceEntry LoadFromConfig(string deviceName, string configFile)
        {
            if (!File.Exists(configFile))
                return null;

            using (ScadaReader sr = new ScadaReader(configFile))
            {
                SectionType secType = SectionType.None;
                string line = null;
                string key = null;
                IValue value = null;
                ReadLineResult result = sr.ReadLine(out secType, out line, out key, out value);
                // Dictionary<string, string> config = new Dictionary<string, string>();
                DeviceEntry entry = new DeviceEntry();
                while (result == ReadLineResult.OK)
                {
                    result = sr.ReadLine(out secType, out line, out key, out value);

                    if (secType == SectionType.KeyWithStringValue)
                    {
                        entry[key] = value;
                    }
                }
                DirectoryInfo di = Directory.GetParent(configFile);
                string devicePath = di.FullName;
                // Path
                entry[DeviceEntry.Path] = new StringValue(devicePath);
                entry[DeviceEntry.Identity] = new StringValue(deviceName);

                // Virtual 
                if (File.Exists(devicePath + "\\virtual-device"))
                {
                    entry[DeviceEntry.Virtual] = new StringValue("true");
                }
                return entry;
            }
        }

        public string NaIDeviceSn { get; set; }

        public int MinuteAdjust { get; set; }

        public string NaIFilePath { get; set; }

        // to test;
        internal void AddNewIpAddress(string wireIp, string wirePort, string wirelessIp, string wirelessPort, bool country)
        {
            string settingFileName = ConfigPath.GetConfigFilePath("agent.settings");
            // string settingFileName = string.Format("{0}\\..\\{1}", Application.ExecutablePath, "agent.settings");
            if (File.Exists(settingFileName))
            {
                doc.Load(settingFileName);
            }

            // Data Center
            var dsNotes = doc.SelectNodes("//datacenter");
            var ds = dsNotes[0].ParentNode;

            XmlNode newDataCenterNode = doc.CreateElement("datacenter");
            ds.AppendChild(newDataCenterNode);
            this.AddAttribute(doc, newDataCenterNode, "ip", wireIp);
            this.AddAttribute(doc, newDataCenterNode, "port", wirePort);
            this.AddAttribute(doc, newDataCenterNode, "wirelessip", wirelessIp);
            this.AddAttribute(doc, newDataCenterNode, "wirelessport", wirelessPort);
            this.AddAttribute(doc, newDataCenterNode, "type", country ? "2" : "1");
            doc.Save(settingFileName);
        }

        private void AddAttribute(XmlDocument doc, XmlNode node, string key, string value)
        {
            XmlAttribute attrGender = doc.CreateAttribute(key);
            attrGender.Value = value;
            node.Attributes.Append(attrGender);
        }

        internal void SetThreshold(string polId, string th1, string th2)
        {
            string settingFileName = ConfigPath.GetConfigFilePath("agent.settings");
            if (File.Exists(settingFileName))
            {
                doc.Load(settingFileName);
            }

            // Data Center
            var polNotes = doc.SelectNodes("//polId");
            XmlNode thePolIdNode = null;
            foreach (XmlNode polIdNode in polNotes)
            {
                var nameAttr = polIdNode.Attributes.GetNamedItem("name");
                if (nameAttr != null)
                {
                    string name = nameAttr.Value;
                    if (name == polId)
                    {
                        thePolIdNode = polIdNode;

                        XmlAttribute v1Attr = (XmlAttribute)thePolIdNode.Attributes.GetNamedItem("v1");
                        v1Attr.Value = th1;
                        XmlAttribute v2Attr = (XmlAttribute)thePolIdNode.Attributes.GetNamedItem("v2");
                        v2Attr.Value = th2;
                        doc.Save(settingFileName);
                        return;
                    }
                }
            }

            if (thePolIdNode == null)
            {
                var thresholdNode = polNotes[0].ParentNode;

                thePolIdNode  = doc.CreateElement("polId");
                thresholdNode.AppendChild(thePolIdNode);

                this.AddAttribute(doc, thePolIdNode, "name", polId);
                this.AddAttribute(doc, thePolIdNode, "v1", th1);
                this.AddAttribute(doc, thePolIdNode, "v2", th2);
                doc.Save(settingFileName);
            }  
        }

        internal bool GetThreshold(string polId, out string th1, out string th2)
        {
            th1 = "";
            th2 = "";
            string settingFileName = ConfigPath.GetConfigFilePath("agent.settings");
            if (File.Exists(settingFileName))
            {
                doc.Load(settingFileName);
            }

            // Data Center
            var polNotes = doc.SelectNodes("//polId");
            foreach (XmlNode polIdNode in polNotes)
            {
                var nameAttr = polIdNode.Attributes.GetNamedItem("name");
                if (nameAttr != null)
                {
                    string name = nameAttr.Value;
                    if (name == polId)
                    {
                        th1 = polIdNode.Attributes.GetNamedItem("v1").Value;
                        th2 = polIdNode.Attributes.GetNamedItem("v2").Value;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

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

namespace Scada.MainVision
{
    /// <summary>
    /// 
    /// </summary>
    public class Settings
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
        private XmlDocument doc = new XmlDocument();

        public Settings()
        {
            // "Agent.Settings"
            string settingFileName = ConfigPath.GetConfigFilePath("mainvs.settings");
            if (!File.Exists(settingFileName))
            {
                return;
            }

            doc.Load(settingFileName);

            var application = doc.SelectNodes("//application");
            XmlNode appNode = application[0];

            this.ApplicationName = this.GetAttribute(appNode, "title", "MainVision");
            this.StationName = this.GetAttribute(appNode, "station", "????自动站");
            this.Status = this.GetAttribute(appNode, "status", "试运行");
            this.StationId = this.GetAttribute(appNode, "stationId", "00000000001");
        }

        private string GetAttribute(XmlNode node, string attr, string defaultValue = "")
        {
            try
            {
                var xmlAttr = node.Attributes.GetNamedItem(attr);
                return xmlAttr.Value;
            }
            catch (Exception e)
            {
                return defaultValue;
            }
        }

        public string ApplicationName
        {
            get;
            set;
        }
        public string StationName
        {
            get;
            set;
        }


        public string Status { get; set; }
        public string StationId { get; set; }
        
    }
}

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

        public bool IsCAS { get; set; }

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

            this.IsCAS = false;
            string cfg2FileName = ConfigPath.GetConfigFilePath("dsm2.cfg");
            if (File.Exists(cfg2FileName))
            {
                this.IsCAS = true;
            }

            doc.Load(settingFileName);

            var application = doc.SelectNodes("//application");
            XmlNode appNode = application[0];

            this.ApplicationName = this.GetAttribute(appNode, "title", "MainVision");
            this.StationName = this.GetAttribute(appNode, "station", "????自动站");
            this.Status = this.GetAttribute(appNode, "status", "试运行");
            this.StationId = this.GetAttribute(appNode, "stationId", "00000000001");
            this.Pos = this.GetAttribute(appNode, "pos", "未知GPS信息");
            this.UserComp = this.GetAttribute(appNode, "usercomp", "未知用户单位");
            this.AdminComp = this.GetAttribute(appNode, "admincomp", "未知运营单位");
            this.BuildComp = this.GetAttribute(appNode, "buildcomp", "未知承建单位");
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

        public string Pos
        {
            get;
            set;
        }




        public string Status { get; set; }

        public string StationId { get; set; }

        public string UserComp { get; set; }

        public string AdminComp { get; set; }

        public string BuildComp { get; set; }
    }
}

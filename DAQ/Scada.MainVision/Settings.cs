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

        public const string DeviceKey_HvSampler = "scada.hvsampler";

        public const string DeviceKey_ISampler = "scada.isampler";

        public const string DeviceKey_Shelter = "scada.shelter";

        public const string DeviceKey_Dwd = "scada.dwd";

        public const string DeviceKey_NaI = "scada.naidevice";


        public string[] DeviceKeys = {
                                DeviceKey_Hpic, 
                                DeviceKey_Weather, 
                                DeviceKey_HvSampler, 
                                DeviceKey_ISampler, 
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

            this.ApplicationName = appNode.Attributes.GetNamedItem("title").Value;
            this.StationName = appNode.Attributes.GetNamedItem("station").Value;

           
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

    }
}

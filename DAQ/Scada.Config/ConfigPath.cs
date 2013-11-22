using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scada.Config
{
    public class ConfigPath
    {
        private static string CurrentConfigPath
        {
            get;
            set;
        }

        public static string Current()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string path = Path.GetDirectoryName(location);
            string configPath = Path.Combine(path, "config");
            if (Directory.Exists(configPath))
            {
                return Path.GetFullPath(configPath);
            }
            else
            {
                configPath = Path.Combine(path, "..\\config");
                
                if (Directory.Exists(configPath))
                {
                    return Path.GetFullPath(configPath);
                }
            }
            return string.Empty;
        }


        public static string GetConfigFilePath(string relativePath)
        {
            if (string.IsNullOrEmpty(CurrentConfigPath))
            {
                CurrentConfigPath = ConfigPath.Current();
            }

            string configFilePath = Path.Combine(CurrentConfigPath, relativePath);
            return configFilePath;
        }

        public static string GetDeviceConfigFilePath(string deviceKey, string version)
        {
            string relateDeviceConfigFilePath = string.Format("devices\\{0}\\{1}\\device.cfg", deviceKey, version);
            return GetConfigFilePath(relateDeviceConfigFilePath);
        }
    }
}

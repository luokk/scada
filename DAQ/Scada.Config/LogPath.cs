using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scada.Config
{
    public class LogPath
    {
        private static string LogPathBase;

        static LogPath()
        {
            LogPath.Initialize();
        }

        public static string GetExeFilePath(string fileName)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string path = Path.GetDirectoryName(location);
            return Path.Combine(path, fileName);
        }

        // Maybe NO use
        public static string GetDeviceLogFilePath(string deviceName)
        {
            return Path.Combine(LogPathBase, deviceName);
        }

        public static string GetDeviceLogFilePath(string deviceName, DateTime now)
        {
            string deviceLogPath = Path.Combine(LogPathBase, deviceName);
            string monthPath = string.Format("{0}-{1:D2}", now.Year, now.Month);
            return Path.Combine(deviceLogPath, monthPath);
        }

        private static void Initialize()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string path = Path.GetDirectoryName(location);
            LogPathBase = Path.Combine(path, "logs");
        }

    }
}

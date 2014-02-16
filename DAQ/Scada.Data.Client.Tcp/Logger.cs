using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Data.Client.Tcp
{
    class Logger
    {
        private StreamWriter writer;

        private int counter = 0;

        public Logger(string fileName)
        {
            this.writer = new StreamWriter(fileName);
        }

        public void Log(string msg)
        {
            string content = string.Format("{0}: {1}", DateTime.Now, msg);
            this.writer.WriteLine(content);
            if (this.counter % 5 == 0)
            {
                this.writer.Flush();
            }
            this.counter += 1;
        }

        public void Close()
        {
            try
            {
                this.writer.Flush();
                this.writer.Close();
            }
            catch (Exception)
            { }
        }
    }

    class Log
    {
        public static Dictionary<string, Logger> dict = new Dictionary<string, Logger>();

        private static string GetLogFileName(DateTime date)
        {
            string logFileName = string.Format("{0}-{1:D2}-{2:D2}.t.log", date.Year, date.Month, date.Day);
            return logFileName;
        }

        private static string GetLogFileName(string deviceKey, DateTime date)
        {
            string logPath = Program.GetLogPath(deviceKey);
            string logFileName = GetLogFileName(date);
            string logFilePath = string.Format("{0}\\{1}", logPath, logFileName);
            return logFilePath;
        }

        public static Logger GetLogFile(string deviceKey)
        {
            string logFilePath = GetLogFileName(deviceKey, DateTime.Now);

            string key = logFilePath.ToLower();
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                // Clear yesterday's log.
                DateTime yd = DateTime.Now.AddDays(-1);
                CloseLastLogFile(deviceKey, yd);

                Logger logger = new Logger(logFilePath);
                dict.Add(key, logger);
                return logger;
            }
        }

        private static void CloseLastLogFile(string deviceKey, DateTime time)
        {
            string logFilePath = GetLogFileName(deviceKey, time);

            string key = logFilePath.ToLower();
            if (dict.ContainsKey(key))
            {
                Logger logger = dict[key];
                logger.Close();

                dict.Remove(key);
            }
        }
    }
}

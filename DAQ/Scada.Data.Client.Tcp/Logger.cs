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
            this.writer.Flush();
            this.writer.Close();
        }
    }

    class Log
    {
        public static Dictionary<string, Logger> dict = new Dictionary<string, Logger>();

        public static Logger GetLogFile(string deviceKey)
        {
            string logPath = Program.GetLogPath(deviceKey);
            DateTime t = DateTime.Now;
            string logFileName = string.Format("{0}-{1:D2}-{2:D2}.t.log", t.Year, t.Month, t.Day);
            string logFilePath = string.Format("{0}\\{1}", logPath, logFileName);


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
            string logPath = Program.GetLogPath(deviceKey);
            DateTime t = time;
            string logFileName = string.Format("{0}-{1:D2}-{2:D2}", t.Year, t.Month, t.Day);
            string logFilePath = string.Format("{0}\\{1}", logPath, logFileName);

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Scada.Config
{
    public class LoggerClient
    {
        private Uri loggerApi = new Uri("http://127.0.0.1:6060/");

        public LoggerClient()
        {
        }

        private byte[] BuildMessage(string fileName, string content)
        {
            string line = string.Format("[{0}]:{1}", fileName, content);
            byte[] bytes = Encoding.ASCII.GetBytes(line);
            return bytes;
        }

        public void Send(string fileName, string content)
        {
            using (var client = new WebClient())
            {
                // client.UploadDataCompleted += UploadDataCompleted;
                client.UploadDataAsync(loggerApi, "POST", BuildMessage(fileName, content), client);
            }
        }

        public static bool Contains(string ff)
        {
            return fileNameSets.Contains(string.Format("@{0}", ff));
        }

        public static HashSet<string> fileNameSets = null;

        public static void Initialize()
        {
            WatchLoggerFlagFiles();
        }

        private static void WatchLoggerFlagFiles()
        {
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (!Directory.Exists(statusPath))
            {
                Directory.CreateDirectory(statusPath);
            }

            string[] filePaths = Directory.GetFiles(statusPath, @"@*");
            var fileNames = filePaths.Select((string path) =>
            {
                return Path.GetFileName(path);
            });
            fileNameSets = new HashSet<string>(fileNames);

            FileSystemWatcher fsw = new FileSystemWatcher(statusPath, "@*");
            fsw.Created += FileChanged;
            fsw.Deleted += FileChanged;
            fsw.EnableRaisingEvents = true;
        }

        static void FileChanged(object sender, FileSystemEventArgs e)
        {
            string ff = e.Name.ToLower();
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                fileNameSets.Add(ff);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                fileNameSets.Remove(ff);
            }
        }
    }
}

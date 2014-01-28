using Scada.Common;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Scada.Declare
{
    public enum RecordType
    {
        Data,
        Origin,
        Event,
        Notice,
        Error,
    }

	public static class RecordManager
	{
		private static MySQLRecord mysql = null;

		private static AnalysisRecord analysis = null;

        private static LoggerClient logger = null;

		private static int flushCtrlCount = 0;

		/// <summary>
		/// StreamHolder presents a Daily stream for log.
		/// </summary>
        public struct StreamHolder
        {
            private FileStream fileStream;

            private string filePath;

            public FileStream FileStream
            {
                get { return this.fileStream; }
                set { this.fileStream = value; }
            }

            public string FilePath
            {
                get { return this.filePath; }
                set { this.filePath = value; }
            }
            
        }

        private static Dictionary<string, StreamHolder> streams = new Dictionary<string, StreamHolder>();

		public static void Initialize()
		{
			RecordManager.mysql = new MySQLRecord();

			RecordManager.analysis = new AnalysisRecord();

            LoggerClient.Initialize();
            RecordManager.logger = new LoggerClient();
			// RecordManager.frameworkRecord = new FileRecord("");
		}

        public static void DoSystemEventRecord(Device device, string systemEvent, RecordType recordType = RecordType.Event)
		{
			RecordManager.WriteDataToLog(device, systemEvent, recordType);
		}

		public static void DoDataRecord(DeviceData deviceData)
		{
			// TODO: Record it in the files.
            if (deviceData.OriginData != null && deviceData.OriginData.Length > 0)
            {
                string originLine = deviceData.OriginData;
                RecordManager.WriteDataToLog(deviceData.Device, originLine.Trim(), RecordType.Origin);
            }
			string line = RecordManager.PackDeviceData(deviceData);
			RecordManager.WriteDataToLog(deviceData.Device, line, RecordType.Data);

            // Record into MySQL:)
			if (!RecordManager.mysql.DoRecord(deviceData))
			{
				// TODO: Do log this failure.
			}
		}

		private static string PackDeviceData(DeviceData deviceData)
        {
            if (deviceData.Data == null)
            {
                return "<DeviceData::Data Is Null>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (object o in deviceData.Data)
            {
                if (o != null)
                {
                    sb.Append(o.ToString()).Append(" ");
                }
            }
            return sb.ToString();
        }

		private static void WriteDataToLog(Device device, string content, RecordType recordType)
		{
            // To Log File
			DateTime now = DateTime.Now;
			FileStream stream = RecordManager.GetLogFileStream(device, now);
			string time = string.Format("[{0:HH:mm:ss}] ", now);
			StringBuilder sb = new StringBuilder(time);
            sb.Append(string.Format(" <{0}> ", recordType.ToString()));
			sb.Append(content);
			sb.Append("\r\n");
			string line = sb.ToString();

			byte[] bytes = Encoding.ASCII.GetBytes(line);
			stream.Write(bytes, 0, bytes.Length);

			// Flush Control.
			if (flushCtrlCount % 10 == 0)
			{
				stream.Flush();
			}
			flushCtrlCount = (flushCtrlCount + 1) % 5;

            // To Log Console
            if (ExistLoggerConsoleProc())
            {
                string deviceKey = device.Id.ToLower();
                if (LoggerClient.Contains(deviceKey))
                {
                    logger.Send(deviceKey, line);
                }
            }
		}

        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Optimze: Only has logger.server process, the module would try HTTP connection.
        /// Detect process every 15 sec, not every time, 
        /// </summary>
        private static long lastDetectTimeTicks = 0;

        private static bool lastDetectResult = false;

        public static void ResetDetectTime()
        {
            lastDetectTimeTicks = 0;
        }

        private static bool ExistLoggerConsoleProc()
        {
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - lastDetectTimeTicks > (15 * 10000000))
            {
                lastDetectTimeTicks = nowTicks;
                Process[] ps = Process.GetProcessesByName(@"Scada.Logger.Server");
                lastDetectResult = (ps != null && ps.Length > 0);
                return lastDetectResult;
            }
            return lastDetectResult;
        }
        //////////////////////////////////////////////////////////////////////////////

        private static FileStream GetLogFileStream(Device device, DateTime now)
        {
            string path = GetDeviceLogPath(device, now);
            string deviceName = device.Name;
            if (streams.ContainsKey(deviceName))
            {
                StreamHolder holder = streams[deviceName];
                if (holder.FilePath.ToLower() == path.ToLower())
                {
                    return holder.FileStream;
                }
                else
                {
                    FileStream fileStream = holder.FileStream;
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                    streams.Remove(deviceName);
                }
            }

            StreamHolder newHolder = new StreamHolder() { FilePath = path };
            if (!File.Exists(path))
            {
                // TODO: TO TEST!
                // string logPath = GetDeviceLogPath(device, DateTime.Now);
                FileStream fs = File.Create(path);
                newHolder.FileStream = fs;
                streams[deviceName] = newHolder;
                return fs;
            }
            else
            {
                FileStream fs = File.Open(path, FileMode.Append);
                newHolder.FileStream = fs;
                streams[deviceName] = newHolder;
                return fs;
            }
        }

        private static Dictionary<string, bool> existPaths = new Dictionary<string, bool>();

        private static string GetDeviceLogPath(Device device, DateTime now)
        {
            string deviceLogPath = LogPath.GetDeviceLogFilePath(device.Id, now);
            if (!existPaths.ContainsKey(deviceLogPath.ToLower()))
            {
                if (!Directory.Exists(deviceLogPath))
                {
                    Directory.CreateDirectory(deviceLogPath);
                }
                existPaths.Add(deviceLogPath.ToLower(), true);
            }
            string fileName = string.Format("{0}-{1}-{2}.daq.log", now.Year, now.Month, now.Day);
            
            string path = string.Format("{0}\\{1}", deviceLogPath, fileName);
            return path;
        }

    }
}

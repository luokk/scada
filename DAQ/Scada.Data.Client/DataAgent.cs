using Newtonsoft.Json.Linq;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Scada.Data.Client
{
    /// <summary>
    /// 
    /// </summary>
    public enum NotifyEvents
    {
        DebugMessage,
        EventMessage,
        UploadFileOK,
        UploadFileFailed,
        SendDataOK,
        SendDataFailed,
        BadCommand,
        HistoryData
    }


    public delegate void OnNotifyEvent(DataAgent agent, NotifyEvents notifyEvent, Notify p);

    /// <summary>
    /// 
    /// </summary>
    public class DataAgent
    {
        private const string Post = @"POST";

        private const int Timeout = 5000;

        public event OnNotifyEvent NotifyEvent;
        
        internal bool SendDataStarted
        {
            get;
            set;
        }

        public DataAgent(Settings.DataCenter2 dataCenter)
        {
            this.DataCenter = dataCenter;
        }

        public Settings.DataCenter2 DataCenter
        {
            get;
            set;
        }

        /// <summary>
        /// Upload Data Entry
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="time"></param>
        internal void SendDataPacket(Packet packet, DateTime time)
        {
            this.Send(this.DataCenter.GetUrl("data/commit"), packet, time);
        }

        /// <summary>
        /// Commands
        /// </summary>
        internal void FetchCommands()
        {
            Uri uri = new Uri(this.DataCenter.GetUrl("command/query/" + Settings.Instance.Station));
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                    {
                        if (e.Error == null)
                        {
                            this.NotifyEvent(this, NotifyEvents.EventMessage, new Notify() { Message = e.Result });
                            this.ParseCommand(e.Result);
                        }
                        else
                        {
                            this.NotifyEvent(this, NotifyEvents.EventMessage, new Notify() { Message = e.Error.Message });
                        }

                    };
                wc.DownloadStringAsync(uri);
            }
            catch (Exception)
            {
            }
        }

        private void ParseCommand(string cmd)
        {
            try
            {
                JObject json = JObject.Parse(cmd);
                JToken res = json["results"];
                JToken command = res["command"];

                
                JToken type = command["type"];
                if (type == null)
                    return;

                if (type.Value<string>() == "history")
                {
                    string device = command["device"].Value<string>();
                    JToken times =  command["times"];
                    string timesStr = null;
                    if (times != null)
                    {
                        timesStr = times.Value<string>();
                    }

                    string start = command["start"].Value<string>();
                    string end = command["end"].Value<string>();
                    this.HandleHistoryData(device, start, end, timesStr);
                    
                }

            }
            catch (Exception e)
            {
                this.NotifyEvent(this, NotifyEvents.BadCommand, new Notify() { Message = e.Message });
            }
        }

        private void HandleHistoryData(string device, string start, string end, string times)
        {
            Notify n = new Notify();
            n.SetValue("device", device);
            n.SetValue("start", start);
            n.SetValue("end", end);
            n.SetValue("times", times);
            this.NotifyEvent(this, NotifyEvents.HistoryData, n);
        }

        /// <summary>
        /// Send Dispatcher
        /// </summary>
        /// <param name="p"></param>
        internal void SendPacket(Packet p)
        {
            if (!p.IsFilePacket)
            {
                this.SendDataPacket(p, default(DateTime));
            }
            else
            {
                this.SendFilePacket(p);
            }
        }

        /// <summary>
        /// Upload Data Implements
        /// </summary>
        /// <param name="api"></param>
        /// <param name="packet"></param>
        /// <param name="time"></param>
        private void Send(string api, Packet packet, DateTime time)
        {
            try
            {
                Uri uri = new Uri(api);
                byte[] data = Encoding.ASCII.GetBytes(packet.ToString());
                using (WebClient wc = new WebClient())
                {
                    wc.UploadDataCompleted += (object sender, UploadDataCompletedEventArgs e) =>
                    {
                        if (e.Error != null)
                        {
                            this.NotifyEvent(this, NotifyEvents.SendDataOK, new Notify() { DeviceKey = packet.DeviceKey, Message = e.Error.Message });
                            this.HandleWebException(e.Error);
                            return;
                        }

                        Packet p = (Packet)e.UserState;
                        if (p != null)
                        {
                            string result = Encoding.ASCII.GetString(e.Result);
                            result = result.Trim();
                            if (!string.IsNullOrEmpty(result))
                            {
                                this.NotifyEvent(this, NotifyEvents.SendDataOK, new Notify() { DeviceKey = packet.DeviceKey, Message = result });
                            }
                        }
                    };

                    wc.UploadDataAsync(uri, "POST", data, packet);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Upload File
        /// </summary>
        /// <param name="packet"></param>
        internal void SendFilePacket(Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Path) || !File.Exists(packet.Path))
            {
                Notify msg = new Notify();
                msg.Message = "No File Found";
                this.NotifyEvent(this, NotifyEvents.EventMessage, msg);
                return;
            }

            string uploadUrl = string.Empty;
            if (packet.FileType.Equals("labr", StringComparison.OrdinalIgnoreCase))
            {
                string path = Path.GetDirectoryName(packet.Path);
                var folder1 = Path.GetFileName(Path.GetDirectoryName(path));
                var folder2 = Path.GetFileName(path);
                uploadUrl = this.GetUploadApi(packet.FileType, folder1, folder2);
            }
            else if (packet.FileType.Equals("hpge", StringComparison.OrdinalIgnoreCase))
            {
                var folder = DataSource.GetCurrentSid();
                var param = this.GetHpGeParams(packet.Path);    // "2014-07-04 00:00:00,2014-07-04 00:00:00,2014-07-04 00:00:00,PRT";
                param = param.Replace('/', '-');
                uploadUrl = this.GetUploadApi(packet.FileType, folder, param);
            }

            Uri uri = new Uri(this.DataCenter.GetUrl(uploadUrl));
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadFileCompleted += (object sender, UploadFileCompletedEventArgs e) =>
                        {

                            Packet p = (Packet)e.UserState;
                            if (p != null)
                            {
                                if (e.Error == null)
                                {
                                    string result = Encoding.UTF8.GetString(e.Result);
                                    this.RemovePrefix(p.Path);
                                    // LogPath.GetDeviceLogFilePath("");
                                    string msg = string.Format("成功上传 {0}", this.GetRelFilePath(packet));
                                    this.NotifyEvent(this, NotifyEvents.UploadFileOK, new Notify() { Message = msg });
                                }
                                else
                                {
                                    this.NotifyEvent(this, NotifyEvents.UploadFileFailed, new Notify() { Message = e.Error.Message });
                                }
                            }
                        };
                    wc.UploadFileAsync(uri, Post, packet.Path, packet);
                }
            }
            catch (WebException)
            {
             
            }
        }

        private string GetRelFilePath(Packet packet)
        {
            if (packet.FileType.Equals("labr", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(packet.Path);

                return string.Format("{0}", fileName);
            }
            else if (packet.FileType.Equals("hpge", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(packet.Path);
                string path = Path.GetDirectoryName(packet.Path);
                var sidFolder = Path.GetFileName(path);
                if (fileName.StartsWith("!"))
                {
                    return string.Format("{0}\\{1}", sidFolder, fileName.Remove(0, 1));
                }
                return string.Format("{0}\\{1}", sidFolder, fileName);
            }
            return string.Empty;
        }

        private string GetHpGeParams(string fileName)
        {
            string filename = Path.GetFileName(fileName).ToLower();
            string mode = string.Empty;
            if (filename.IndexOf("qaspectra") >= 0)
            {
                mode = "QA.SPE";
            }
            else if (filename.EndsWith(".rpt"))
            {
                mode = "RPT";
            }
            else if (filename.IndexOf("spectra24") >= 0)
            {
                mode = "Sample24.SPE";
            }
            else
            {
                mode = "Sample2.SPE";
            }

            int p = filename.IndexOf("_");
            int e = filename.IndexOf(".");
            string time = filename.Substring(p + 1, e - p - 1);
            time = time.Replace('t', ' ');
            time = time.Replace('_', ':');

            DateTime dt = DateTime.ParseExact(time, "yyyy-MM-dd hh:mm:ss", null);

            string[] lines = File.ReadAllLines(fileName);

            bool b1 = false;
            bool b2 = false;
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            foreach (var line in lines)
            {
                if (b1)
                {
                    startTime = DateTime.ParseExact(line, "MM/dd/yyyy hh:mm:ss", null);
                    
                    b1 = false;
                    continue;
                }
                if (b2)
                {
                    string[] parts = line.Split(' ');
                    int max = Math.Max(int.Parse(parts[0]), int.Parse(parts[1]));
                    endTime = startTime.AddSeconds(max);
                    break;
                }
                if (line.IndexOf("$DATE_MEA:") >= 0)
                {
                    b1 = true;
                    continue;
                }

                if (line.IndexOf("$MEAS_TIM:") >= 0)
                {
                    b2 = true;
                }
            }

            return string.Format("{0},{1},{2},{3}", dt, startTime, endTime, mode);
        }

        private void RemovePrefix(string p)
        {
            if (File.Exists(p))
            {
                string fileName = Path.GetFileName(p);
                if (fileName.StartsWith("!"))
                {
                    string dirName = Path.GetDirectoryName(p);
                    File.Move(p, Path.Combine(dirName, fileName.Substring(1)));
                }
            }
        }

        private string GetUploadApi(string fileType, string folder)
        {
            string stationId = Settings.Instance.Station;

            return string.Format("data/upload/{0}/{1}/{2}", stationId, fileType, folder);
        }

        private string GetUploadApi(string fileType, string folder1, string folder2)
        {
            string stationId = Settings.Instance.Station;

            return string.Format("data/upload/{0}/{1}/{2}/{3}", stationId, fileType, folder1, folder2);
        }

        private void HandleWebException(Exception e)
        {
            WebException we = e as WebException;

            if (we == null)
            {
                return;
            }

            HttpWebResponse hwr = we.Response as HttpWebResponse;
            if (hwr != null)
            {
                switch (hwr.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // TODO: No response!
            }
        }
      
        // Connect means first HTTP packet to the data Center.
        internal void DoAuth()
        {
            

        }

    }
}

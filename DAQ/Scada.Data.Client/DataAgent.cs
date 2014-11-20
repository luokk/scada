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
using System.Threading;

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

        private WebClient commandClient;
        
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
        internal bool SendDataPacket(Packet packet, DateTime time)
        {
            return this.Send(this.DataCenter.GetUrl("data/commit"), packet, time);
        }

        /// <summary>
        /// Commands
        /// </summary>
        internal void FetchCommands()
        {
            // TODO: Maybe need a Lock?
            Uri uri = new Uri(this.DataCenter.GetUrl("command/query/" + Settings.Instance.Station));
            try
            {
                if (this.commandClient == null)
                {
                    this.commandClient = new WebClient();

                    this.commandClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
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
                }

                this.commandClient.DownloadStringAsync(uri);
            }
            catch (Exception)
            {
                this.commandClient.Dispose();
                this.commandClient = null;
            }
        }

        private void ParseCommand(string cmd)
        {
            try
            {
                JObject json = JObject.Parse(cmd);

                JToken command = json["results"];

                JToken type = command["type"];
                if (type == null)
                    return;

                if (type.Value<string>() == "history")
                {
                    string device = command["device"].Value<string>();

                    JToken content = command["content"];
                    if (content != null)
                    {
                        JToken times = content["times"];
                        string timesStr = null;
                        if (times != null)
                        {
                            timesStr = times.Value<string>();
                        }

                        JToken sid = content["sid"];
                        string sidStr = null;
                        if (sid != null)
                        {
                            sidStr = sid.Value<string>();
                        }

                        if (device == "hpge")
                        {
                            this.HandleHistoryData(device, sidStr);
                        }
                        else
                        {
                            string start = content["start"].Value<string>();
                            string end = content["end"].Value<string>();
                            this.HandleHistoryData(device, start, end, timesStr);
                        }
                    }

                    
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

        private void HandleHistoryData(string device, string sid)
        {
            Notify n = new Notify();
            n.SetValue("device", device);
            n.SetValue("sid", sid);
            this.NotifyEvent(this, NotifyEvents.HistoryData, n);
        }

        /// <summary>
        /// Send Dispatcher
        /// </summary>
        /// <param name="p"></param>
        internal bool SendPacket(Packet p)
        {
            if (!p.IsFilePacket)
            {
                return this.SendDataPacket(p, default(DateTime));
            }
            else
            {
                return this.SendFilePacket(p);
            }
        }

        /// <summary>
        /// Upload Data Implements
        /// </summary>
        /// <param name="api"></param>
        /// <param name="packet"></param>
        /// <param name="time"></param>
        private bool Send(string api, Packet packet, DateTime time)
        {
            try
            {
                Uri uri = new Uri(api);
                byte[] data = Encoding.ASCII.GetBytes(packet.ToString());
                using (WebClient wc = new WebClient())
                {
                    Byte[] result = wc.UploadData(uri, "POST", data);
                    string strResult = Encoding.ASCII.GetString(result);
                    this.NotifyEvent(this, NotifyEvents.SendDataOK, new Notify() { DeviceKey = packet.DeviceKey, Message = strResult });

                    return true;
                }
            }
            catch (Exception e)
            {
                this.NotifyEvent(this, NotifyEvents.SendDataFailed, new Notify() { DeviceKey = packet.DeviceKey, Message = e.Message });
                this.HandleWebException(e);
                return false;
            }
        }

        string GetPacketSID(Packet p)
        {
            string tmp = p.Path;
            int endIndex = tmp.LastIndexOf("\\");
            tmp = tmp.Substring(0, endIndex);
            int startIndex = tmp.LastIndexOf("\\");
            return tmp.Substring(startIndex + 1, tmp.Length - startIndex - 1);
        }

        void RemoveOccupiedToken(Packet p)
        {
            string fileNameWithToken = p.Path;

            if (File.Exists(fileNameWithToken))
            {
                string fileName = Path.GetFileName(fileNameWithToken);
                string path = Path.GetDirectoryName(fileNameWithToken);
                fileName = fileName.Substring(1);
                string newFileName = Path.Combine(path, fileName);
                File.Move(fileNameWithToken, newFileName);
            }
        }

        /// <summary>
        /// Upload File
        /// </summary>
        /// <param name="packet"></param>
        internal bool SendFilePacket(Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Path) || !File.Exists(packet.Path))
            {
                Notify msg = new Notify();
                msg.Message = "No File Found";
                this.NotifyEvent(this, NotifyEvents.EventMessage, msg);

                // 上传失败，移除占用标志
                RemoveOccupiedToken(packet);

                return false;
            }

            string uploadUrl = string.Empty;

            // 判断是哪种设备的文件上传
            if (packet.FileType.Equals("labr", StringComparison.OrdinalIgnoreCase))
            {
                string path = Path.GetDirectoryName(packet.Path);
                var folder1 = Path.GetFileName(Path.GetDirectoryName(path));
                var folder2 = Path.GetFileName(path);
                uploadUrl = this.GetUploadApi(packet.FileType, folder1, folder2);
            }
            else if (packet.FileType.Equals("hpge", StringComparison.OrdinalIgnoreCase))
            {
                //var folder = DataSource.GetCurrentSid();
                var folder = GetPacketSID(packet);

                var param = "";
                try 
                {  
                    param = this.GetHpGeParams(packet.Path); 
                }
                catch (Exception)
                {
                    RemoveOccupiedToken(packet);
                    return false;
                }
                
                param = param.Replace('/', '-');
                uploadUrl = this.GetUploadApi(packet.FileType, folder, param);
            }

            Uri uri = new Uri(this.DataCenter.GetUrl(uploadUrl));
            try
            {
                using (WebClient wc = new WebClient())
                {
                    // 同步上传
                    Byte[] result = wc.UploadFile(uri, Post, packet.Path);
                    string strResult = Encoding.UTF8.GetString(result);
                    string msg = string.Format("成功上传 {0}，信息 {1}", this.GetRelFilePath(packet), strResult);
                    this.NotifyEvent(this, NotifyEvents.UploadFileOK, new Notify() { Message = msg });

                    return true;
                }
            }
            catch (WebException e)
            {
                // 上传失败，移除占用标志
                RemoveOccupiedToken(packet);

                string msg = string.Format("错误上传 {0}，错误信息 {1}", this.GetRelFilePath(packet), e.Message);
                this.NotifyEvent(this, NotifyEvents.UploadFileFailed, new Notify() { Message = msg });

                return false;
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

        // 处理HPGe文件参数
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
            
            //time = time.Replace('_', ':');

            DateTime dt = DateTime.ParseExact(time, "yyyy_MM_dd HH_mm_ss", null);

            // 计算开始、结束时间
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            string[] lines = File.ReadAllLines(fileName);
            if (mode.Contains("SPE"))
            {
                bool b1 = false;
                bool b2 = false;

                foreach (var line in lines)
                {
                    if (b1)
                    {
                        startTime = DateTime.ParseExact(line, "MM/dd/yyyy HH:mm:ss", null);

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
            }
            else if (mode.Contains("RPT"))
            {
                foreach (var line in lines)
                {
                    int index1 = line.IndexOf("Start time:");
                    int index2 = line.IndexOf("Real time:");
                    if (index1 >= 0)
                    {
                        string tmpDate = line.Substring(index1 + 12);
                        tmpDate = tmpDate.Replace('\0', ' ').Trim();

                        startTime = DateTime.Parse(tmpDate);
                        continue;
                    }

                    if (index2 >= 0)
                    {
                        string tmpSecond = line.Substring(index2 + 11);
                        tmpSecond = tmpSecond.Replace('\0', ' ').Trim();
                        int second = int.Parse(tmpSecond);
                        endTime = startTime.AddSeconds(second);
                        break;
                    }
                }
            }
            else { }

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
                    int t = 0;
                    while (t < 5)
                    {
                        try
                        {
                            File.Move(p, Path.Combine(dirName, fileName.Substring(1)));
                            break;
                        }
                        catch (IOException)
                        {
                            t++;
                            Thread.Sleep(1000);
                        }
                    }
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

using Newtonsoft.Json.Linq;
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
        LocalMessage
    }

    public delegate void OnNotifyEvent(DataAgent agent, NotifyEvents notifyEvent, PacketBase p);

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
            Uri uri = new Uri(this.DataCenter.GetUrl("command/query"));
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                    {
                        if (e.Error == null)
                        {
                            this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = e.Result });
                            this.ParseCommand(e.Result);
                        }
                        else
                        {
                            this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = e.Error.Message });
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

            }
            catch (Exception e)
            {
                this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = e.Message });
            }
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
                                this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = result });
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
                PacketBase msg = new PacketBase();
                msg.Message = "No File Found";
                this.NotifyEvent(this, NotifyEvents.LocalMessage, msg);
                return;
            }

            string folder = string.Empty;
            if (packet.FileType.Equals("labr", StringComparison.OrdinalIgnoreCase))
            {
                folder = Path.GetFileName(Path.GetDirectoryName(packet.Path));
            }
            else if (packet.FileType.Equals("hpge", StringComparison.OrdinalIgnoreCase))
            {
                folder = DataSource.GetCurrentSid();
            }

            Uri uri = new Uri(this.DataCenter.GetUrl(this.GetUploadApi(packet.FileType, folder)));
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
                                    this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = result });
                                }
                                else
                                {
                                    this.NotifyEvent(this, NotifyEvents.LocalMessage, new PacketBase() { Message = e.Error.Message });
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

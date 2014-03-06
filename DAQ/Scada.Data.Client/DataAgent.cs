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
    }

    public delegate void OnNotifyEvent(DataAgent agent, NotifyEvents ne, string msg);

    /// <summary>
    /// 
    /// </summary>
    public class DataAgent
    {
        private const string Post = @"POST";

        private const int Timeout = 5000;
        
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

        public int ServerPort
        {
            set;
            get;
        }

        public override string ToString()
        {
            return "";
           
        }


        internal void SendDataPacket(Packet packet, DateTime time)
        {
            Packet.Send(this.DataCenter.GetUrl("data/commit"), packet, time);
        }

        internal void FetchCommands()
        {
            Uri uri = new Uri(this.DataCenter.GetUrl("cmd/query"));
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                    {
                        if (e.Error == null)
                        {
                            this.ParseCommand(e.Result);
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
            catch (Exception)
            {
            }
        }

        internal void SendPacket(Packet p)
        {
            this.SendDataPacket(p, default(DateTime));
        }

        internal void SendFilePacket(Packet packet)
        {
            Uri uri = new Uri(this.DataCenter.GetUrl("data/upload"));
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.UploadFileCompleted += (object sender, UploadFileCompletedEventArgs e) =>
                        {
                            if (e.Error != null)
                            {
                                return;
                            }
                            Packet p = (Packet)e.UserState;
                            if (p != null)
                            {
                                // TODO: with p.Path
                            }
                        };
                    wc.UploadFileAsync(uri, Post, packet.Path, packet);
                }
            }
            catch (WebException)
            {
             
            }
        }
      
        internal void SendReplyPacket(Packet p, DateTime time)
        {
            string s = p.ToString();
            // this.Send(Encoding.ASCII.GetBytes(s));
        }

        // Connect means first HTTP packet to the data Center.
        internal void DoAuth()
        {
            // TODO: Send Packet of init.

        }

    }
}

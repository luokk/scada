using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Scada.Data.Client
{
    public class PacketHandler
    {
        private WebClient wc;

        public PacketHandler()
        {
            this.wc = new WebClient();
            this.wc.UploadDataCompleted += this.OnUploadDataCompleted;
        }

        public string Host
        {
            get;
            set;
        }

        public string GetApi(string apiPath)
        {
            return string.Format("{0}/{1}", this.Host, apiPath);
        }

        private void Send(string apiPath, byte[] bytes)
        {
            try
            {
                const string Method = "POST";
                Uri uri = new Uri(this.GetApi(apiPath));
                wc.UploadDataAsync(uri, Method, bytes);
            }
            catch (WebException we)
            {
                // TODO:

            }
        }

        public void Commit(Packet packet)
        {
            byte[] data = ASCIIEncoding.UTF8.GetBytes(packet.ToString());
            this.Send("data/commit", data);
        }

        private void OnUploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                byte[] result = e.Result;
                if (result.Length > 0)
                {
                    string response = ASCIIEncoding.UTF8.GetString(result);

                    JObject jo = JObject.Parse(response);

                }
            }
        }
    }
}

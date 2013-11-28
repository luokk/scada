using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Scada.Config
{
    public class LoggerClient
    {
        private WebClient wc = new WebClient();

        public LoggerClient()
        {
        }

        public void Send(string content)
        {
            this.wc.UploadData("http://127.0.0.1:6060/", Encoding.ASCII.GetBytes(content));
        }
    }
}

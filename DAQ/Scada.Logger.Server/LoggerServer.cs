using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Logger.Server
{
    class LoggerServer
    {
        private Thread thread;

        // private TcpListener server;
        private HttpListener server;

        private Action<string> action;

        public LoggerServer()
        {

        }

        public void Start(Action<string> action)
        {
            this.action = action;
            this.thread = new Thread(new ParameterizedThreadStart(StartServerThread));
            this.thread.Start();
        }

        private void StartServerThread(object obj)
        {
            try
            {
                // Debug.Assert(false);
                this.server = new HttpListener();
                
                this.server.Prefixes.Add("http://+:6060/");
                this.server.Start();

                this.ResumeListening();
            }
            catch (Exception e)
            {
                MessageBox.Show("If Run in Win7+, Please Run Cmd=netsh http add urlacl url=http://+:6060/ user=<username> as Administrator.");
            }
        }

        private void ResumeListening()
        {
            this.server.BeginGetContext(new AsyncCallback(this.DoRequestCallback), this.server);
        }

        private void DoRequestCallback(IAsyncResult asyncRequestResult)
        {
            if (asyncRequestResult.IsCompleted)
            {
                HttpListener session = (HttpListener)asyncRequestResult.AsyncState;
                HttpListenerContext context = session.EndGetContext(asyncRequestResult);
                this.ResumeListening();

                Stream stream = context.Request.InputStream;

                byte[] bytes = new byte[1024];
                stream.BeginRead(bytes, 0, 1024, new AsyncCallback((IAsyncResult asyncReadResult) => 
                {
                    Stream stream2 = (Stream)asyncReadResult.AsyncState;
                    int r = stream2.EndRead(asyncReadResult);
                    
                    string content = Encoding.ASCII.GetString(bytes, 0, r);
                    if (!string.IsNullOrEmpty(content))
                    {
                        this.action(content);
                    }
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }), stream);
            }

        }

    }
}

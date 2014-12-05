using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Scada.Common
{
    public class CommandReceiver
    {
        private Socket WinSocket = null;

        private Action<string> callbackAction;

        private Thread commandThread;

        private int port;

        private const string Quit = "<!--QUIT-->";

        public CommandReceiver(int port)
        {
            this.port = port;
            IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.WinSocket.Bind(ServerEndPoint);
        }

        public void Close()
        {
            Command.Send(this.port, "<QUIT>");
            this.WinSocket.Close();
            this.commandThread.Abort();
        }

        public void Start(Action<string> callbackAction)
        {
            this.commandThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (true)
                    {
                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        EndPoint Remote = (EndPoint)(sender);

                        byte[] buffer = new byte[1024];
                        
                        int size = this.WinSocket.ReceiveFrom(buffer, ref Remote);
                        if (size > 0)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, size);
                            if (msg == Quit)
                            {
                                break;
                            }
                            callbackAction(msg);
                        }
                        
                    }
                }
                catch (Exception)
                {
                }
            }));
            this.commandThread.Start();
        }

    }

    /// <summary>
    /// Command Sender
    /// </summary>
    public class Command
    {
        public string Source { get; set; }

        public string Dest { get; set; }

        public string Type { get; set; }

        public string Content { get; set; }

        public Command(string source, string dest, string type, string content)
        {
            this.Source = source;
            this.Dest = dest;
            this.Type = type;
            this.Content = content;
        }

        public override string ToString()
        {
            JObject jobject = new JObject();
            jobject["src"] = this.Source;       
            jobject["dest"] = this.Dest;
            jobject["type"] = this.Type;
            jobject["content"] = this.Content;
            return jobject.ToString();
        }

        public static Command Parse(string cmd)
        {
            JObject j = JObject.Parse(cmd);
            string source = j["src"].ToString();
            string dest = j["dest"].ToString();
            string type = j["type"].ToString();
            string content = j["content"].ToString();
            return new Command(source, dest, type, content);
        }

        public static int Send(int port, string msg)
        {
            IPEndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            UdpClient client = new UdpClient();

            var data = Encoding.UTF8.GetBytes(msg);
            int r = client.Send(data, data.Length, RemoteEndPoint);
            client.Close();
            return r;
        }

        public static int Send(int port, Command cmd)
        {
            return Send(port, cmd.ToString());
        }
    }

    public class Ports
    {
        public const int Main = 3100;

        public const int DataClient = 3101;

        public const int DataClientV2 = 3102;

        public const int MainVision = 3103;
    }
}

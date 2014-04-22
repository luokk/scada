using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Scada.Common
{
    public class StateObject
    {
        public Socket Socket
        {
            get;
            set;
        }

        public byte[] bytes
        {
            get;
            set;
        }
    }

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
            this.commandThread.Abort();
        }

        public void Start(Action<string> callbackAction)
        {
            this.commandThread = new Thread(new ThreadStart(() =>
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
            }));
            this.commandThread.Start();
        }

    }

    /// <summary>
    /// Command Sender
    /// </summary>
    public class Command
    {
        public static int Send(int port, string msg)
        {
            IPEndPoint RemoteEndPoint = new IPEndPoint( IPAddress.Parse("127.0.0.1"), port );
            UdpClient client = new UdpClient();

            var data = Encoding.UTF8.GetBytes(msg);
            int r = client.Send(data, data.Length, RemoteEndPoint);
            client.Close();
            return r;
        }
    }
}

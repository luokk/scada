using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Scada.Common
{
    public class CommandReceiver
    {
        private UdpClient receiver = null;

        private Action<string> callbackAction;

        public CommandReceiver(int port)
        {
            IPAddress localIp = IPAddress.Parse("127.0.0.1");
            IPEndPoint localIpEndPoint = new IPEndPoint(localIp, port);
            this.receiver = new UdpClient(localIpEndPoint);
        }

        public void Start(Action<string> callbackAction)
        {
            this.callbackAction = callbackAction;
            this.receiver.BeginReceive(new AsyncCallback(OnReceiveCommand), this.receiver);
        }

        public void OnReceiveCommand(IAsyncResult r)
        {
            if (r.AsyncState != null)
            {
                UdpClient c = (UdpClient)r.AsyncState;
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = c.EndReceive(r, ref remoteIpEndPoint);
                string line = Encoding.ASCII.GetString(bytes);
                this.callbackAction(line);
            }
            this.receiver.BeginReceive(new AsyncCallback(OnReceiveCommand), this.receiver);
        }

    }
}

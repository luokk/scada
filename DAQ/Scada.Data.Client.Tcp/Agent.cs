using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Scada.Data.Client.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    public class StateObject
    {
        public const int BufferSize = 1024;

        public TcpClient client = null;

        public int totalBytesRead = 0;

        // 
        public string readType = null;

        public byte[] buffer = new byte[BufferSize];


        public StringBuilder messageBuffer = new StringBuilder();
    }

    /// <summary>
    /// 
    /// </summary>
    public enum NotifyEvent
    {
        Connected,
        ConnectError,
        ConnectToCountryCenter,
        DisconnectToCountryCenter,
    }

    public enum Type
    {
        Province = 1,
        Country = 2,
    }

    public delegate void OnReceiveMessage(Agent agent, string msg);

    public delegate void OnNotifyEvent(Agent agent, NotifyEvent ne, string msg);

    /// <summary>
    /// 
    /// </summary>
    public class Agent
    {
        // Wired connection Tcp client
        private TcpClient client = null;

        // Wireless connection Tcp client
        private TcpClient wirelessClient = null;

        // Maybe it uses wired connection, or wireless.
        private NetworkStream stream;

        // the current data handler.
        private MessageDataHandler handler;

        private const int Timeout = 5000;
        
        internal bool SendDataStarted
        {
            get;
            set;
        }

        internal bool SendDataDirectlyStarted
        {
            get;
            set;
        }

        internal bool OnHistoryData
        {
            get;
            set;
        }

        public Agent(string serverAddress, int serverPort)
        {
            this.ServerAddress = serverAddress;
            this.ServerPort = serverPort;
        }

        internal void AddWirelessInfo(string wirelessServerAddress, int wirelessServerPort)
        {
            this.WirelessServerAddress = wirelessServerAddress;
            this.WirelessServerPort = wirelessServerPort;
        }

        public string ServerAddress
        {
            get;
            set;
        }

        public int ServerPort
        {
            set;
            get;
        }

        public string WirelessServerAddress
        {
            get;
            set;
        }

        public int WirelessServerPort
        {
            set;
            get;
        }

        // No use.
        public Type Type
        {
            get;
            set;
        }

        public bool Wireless
        {
            get;
            set;
        }

        public OnReceiveMessage OnReceiveMessage
        {
            get;
            set;
        }

        public OnNotifyEvent OnNotifyEvent
        {
            get;
            set;
        }

        public override string ToString()
        {
            return (this.client != null) 
                ? string.Format("{0}:{1}", this.ServerAddress, this.ServerPort) 
                : string.Format("{0}:{1}", this.WirelessServerAddress, this.WirelessServerPort);
        }

        public string ToString(bool hasPortInfo)
        {
            if (hasPortInfo)
            {
                return this.ToString();
            }
            return (this.client != null)
                ? string.Format("{0}", this.ServerAddress)
                : string.Format("{0}", this.WirelessServerAddress);
        }

        private void OnConnectionException(Exception e)
        {
            this.ConnectToWireless();
        }

        public void Connect()
        {
            if ((this.client == null) || (!this.client.Connected))
            {
                try
                {
                    this.client = new TcpClient();
                    this.client.ReceiveTimeout = Timeout;

                    this.client.BeginConnect(
                        this.ServerAddress, this.ServerPort, 
                        new AsyncCallback(ConnectCallback), 
                        this.client);
                }
                catch (Exception e)
                {
                    this.ScreenLogAppend("Connect(): " + e.Message);
                    this.OnNotifyEvent(this, NotifyEvent.ConnectError, "有线连接失败: " + e.Message);
                    this.OnConnectionException(e);
                }
            }
        }

        private void ConnectToWireless()
        {
            if ((this.wirelessClient == null) || (!this.wirelessClient.Connected))
            {
                try
                {
                    this.wirelessClient = new TcpClient();
                    this.wirelessClient.ReceiveTimeout = Timeout;

                    this.wirelessClient.BeginConnect(
                        this.WirelessServerAddress, this.WirelessServerPort, 
                        new AsyncCallback(ConnectToWirelessCallback), 
                        this.wirelessClient);
                    this.ScreenLogAppend("using wireless connection");
                }
                catch (Exception e)
                {
                    this.ScreenLogAppend("ConnectToWireless(): " + e.Message);
                    this.OnNotifyEvent(this, NotifyEvent.ConnectError, "无线连接失败: " + e.Message);
                }
            }
        }

        internal void Disconnect()
        {
            try
            {
                if (this.client != null)
                {
                    this.client.Close();
                }

                if (this.wirelessClient != null)
                {
                    this.wirelessClient.Close();
                }
            }
            catch (Exception e)
            {
                this.OnNotifyEvent(this, NotifyEvent.ConnectError, "断开连接时发生错误:" + e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                try
                {
                    TcpClient client = (TcpClient)result.AsyncState;
                    
                    client.EndConnect(result);
                    //client.
                    if (client.Connected)
                    {
                        this.stream = this.client.GetStream();
                        this.BeginRead(this.client);

                        this.handler = new MessageDataHandler(this);
                        // [Auth]
                        this.handler.SendAuthPacket();

                        this.OnNotifyEvent(this, NotifyEvent.Connected, "已连接");
                    }
                }
                catch (SocketException e)
                {
                    this.ScreenLogAppend(e.Message);
                    this.OnNotifyEvent(this, NotifyEvent.ConnectError, e.Message);
                    this.client = null;
                    this.OnConnectionException(e);
                }

            }
        }

        // Callback for Wireless
        private void ConnectToWirelessCallback(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                try
                {
                    TcpClient client = (TcpClient)result.AsyncState;
                    client.EndConnect(result);
                    //client.
                    if (client.Connected)
                    {
                        this.stream = this.client.GetStream();
                        this.BeginRead(this.client);

                        this.handler = new MessageDataHandler(this);
                        // [Auth]
                        this.handler.SendAuthPacket();

                        this.OnNotifyEvent(this, NotifyEvent.Connected, "已连接");
                    }
                }
                catch (SocketException e)
                {
                    this.wirelessClient = null;
                    var s = e.Message;
                }

            }
        }

        // BeginRead~ <client>
        private void BeginRead(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            if (stream.CanRead)
            {
                try
                {
                    StateObject so = new StateObject() { client = client };
                    IAsyncResult ar = stream.BeginRead(so.buffer, 0, StateObject.BufferSize, new AsyncCallback(OnReadCallback), so);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private void OnReadCallback(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                StateObject so = (StateObject)result.AsyncState;
                try
                {
                    NetworkStream stream = client.GetStream();
                    int c = stream.EndRead(result);

                    if (c > 0)
                    {
                        string msg = Encoding.ASCII.GetString(so.buffer, 0, c);
                        this.DoReceivedMessages(msg);
                        this.BeginRead(so.client);
                    }
                }
                catch (Exception e)
                {
                    string readErrorMessage = e.Message;
                }
            }
        }

        private void DoReceivedMessages(string messages)
        {
            string[] msgs = messages.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string msg in msgs)
            {
                if (msg.Trim() != string.Empty)
                {
                    if (this.handler != null)
                    {
                        this.OnReceiveMessage(this, msg);
                        this.handler.OnMessageDispatcher(msg);
                    }
                }
            }
        }

        private void ScreenLogAppend(string msg)
        {
            this.OnReceiveMessage(this, msg);
        }

        /// <summary>
        /// Final entry of send bytes.
        /// </summary>
        /// <param name="message"></param>
        private void Send(byte[] message)
        {
            try
            {
                if (this.stream != null)
                {
                    this.stream.Write(message, 0, message.Length);
                }
            }
            catch(IOException e)
            {
                this.stream = null;
                if (this.client != null)
                {
                    this.client = null;

                    this.RetryConnection();
                    this.ConnectToWireless();
                }
            }
        }

        private void RetryConnection()
        {
            Timer timer = new Timer();
            timer.Interval = 30 * 1000;
            timer.Tick += this.RetryConnectionTimerTick;
            
            timer.Start();
        }

        void RetryConnectionTimerTick(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            if (timer != null)
            {

                timer.Stop();

                this.Connect();
            }
            else
            {
                MessageBox.Show("Timer CastException");
            }
        }

        internal void SendPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            this.Send(Encoding.ASCII.GetBytes(s));
        }

        internal void SendPacket(DataPacket p)
        {
            this.SendPacket(p, default(DateTime));
        }

        internal void SendDataPacket(DataPacket p, DateTime time)
        {
            if (p == null)
                return;
            // Only start or history.
            if (this.SendDataStarted || this.OnHistoryData)
            {
                string s = p.ToString();
                this.Send(Encoding.ASCII.GetBytes(s));
            }
        }


        internal void SendReplyPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            this.Send(Encoding.ASCII.GetBytes(s));
        }

        internal void StartConnectCountryCenter()
        {
            string msg = string.Format("启动到国家数据中心的连接!");
            this.OnNotifyEvent(this, NotifyEvent.ConnectToCountryCenter, msg);
        }

        internal void StopConnectCountryCenter()
        {
            string msg = string.Format("国家数据中心连接已断开");
            this.OnNotifyEvent(this, NotifyEvent.DisconnectToCountryCenter, msg);
        }

    }
}

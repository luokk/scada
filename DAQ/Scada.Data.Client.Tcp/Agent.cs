using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Timers;

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
    public enum NotifyEvents
    {
        Messages,
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

    // public delegate void OnReceiveMessage(Agent agent, string msg);

    public delegate void NotifyEventHandler(Agent agent, NotifyEvents ne, string msg);

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


        private int retryCount = 0;
        
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

        public NotifyEventHandler NotifyEvent
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
            this.RetryConnection();
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
                    this.NotifyEvent(this, NotifyEvents.ConnectError, "Connect(): " + e.Message);
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
                    this.ShowNetMessage("using wireless connection");
                }
                catch (Exception e)
                {
                    this.NotifyEvent(this, NotifyEvents.ConnectError, "ConnectToWireless(): " + e.Message);
                    this.OnConnectionException(e);
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
                this.NotifyEvent(this, NotifyEvents.ConnectError, "Disconnect(): " + e.Message);
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

                        this.NotifyEvent(this, NotifyEvents.Connected, "已连接");
                    }
                }
                catch (Exception se)
                {
                    this.wirelessClient = null;
                    this.client = null;

                    this.NotifyEvent(this, NotifyEvents.ConnectError, "ConnectCallback(): " + se.Message);
                    this.OnConnectionException(se);
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

                        this.NotifyEvent(this, NotifyEvents.Connected, "已连接");
                    }
                }
                catch (Exception se)
                {
                    this.wirelessClient = null;
                    this.client = null;

                    this.NotifyEvent(this, NotifyEvents.ConnectError, "ConnectToWirelessCallback(): " + se.Message);
                    this.OnConnectionException(se);
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
                        this.ShowNetMessage(msg);
                        this.handler.OnMessageDispatcher(msg);
                    }
                }
            }
        }

        private void ShowNetMessage(string msg)
        {
            if ("6031" == Value.Parse(msg, "CN"))
            {
                return;
            }
            this.NotifyEvent(this, NotifyEvents.Messages, msg);
        }

        /// <summary>
        /// Final entry of send bytes.
        /// </summary>
        /// <param name="message"></param>
        private bool Send(byte[] message)
        {
            try
            {
                if (this.stream != null)
                {
                    this.stream.Write(message, 0, message.Length);
                    return true;
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
            return false;
        }

        // A. 每30秒试图重连一次
        // B. 6次连接失败, 则选择无线方式
        // C. 无线连接也失败, 则重新测试连接无线4次
        // D. 无线也连接不上, 则回到A步骤
        private void RetryConnection()
        {
            Timer timer = new Timer();
            timer.Interval = 30 * 1000;
            timer.Elapsed += (s, e) => 
            {
                if (timer != null)
                {
                    // Timer once;
                    timer.Stop();
                    timer.Dispose();
                    // Connect to wireline Network.
                    if (this.retryCount < 6)
                    {
                        this.retryCount++;
                        this.Connect();
                    }
                    else if (this.retryCount < 10)
                    {
                        this.retryCount++;
                        this.ConnectToWireless();
                    }
                    else
                    {
                        this.retryCount = 0;
                    }
                }
            };
            
            timer.Start();
        }

        internal bool SendPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            return this.Send(Encoding.ASCII.GetBytes(s));
        }

        internal bool SendPacket(DataPacket p)
        {
            return this.SendPacket(p, default(DateTime));
        }

        internal bool SendDataPacket(DataPacket p, DateTime time)
        {
            if (p == null)
                return false;
            // Only start or history.
            if (this.SendDataStarted || this.OnHistoryData)
            {
                string s = p.ToString();
                return this.Send(Encoding.ASCII.GetBytes(s));
            }
            return false;
        }


        internal void SendReplyPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            this.Send(Encoding.ASCII.GetBytes(s));
        }

        internal void StartConnectCountryCenter()
        {
            string msg = string.Format("启动到国家数据中心的连接!");
            this.NotifyEvent(this, NotifyEvents.ConnectToCountryCenter, msg);
        }

        internal void StopConnectCountryCenter()
        {
            string msg = string.Format("国家数据中心连接已断开");
            this.NotifyEvent(this, NotifyEvents.DisconnectToCountryCenter, msg);
        }

    }
}

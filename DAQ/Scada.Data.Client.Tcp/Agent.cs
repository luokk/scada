using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Scada.Data.Client.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    public class SessionState
    {
        public const int BufferSize = 1024;

        public SessionState(TcpClient client, NetworkStream stream)
        {
            this.Client = client;
            this.Stream = stream;
        }

        public string GetReceivedMessage(int size)
        {
            if (size > 0)
            {
                return Encoding.ASCII.GetString(buffer, 0, size);
            }
            return string.Empty;
        }

        public TcpClient Client { get; set; }

        public NetworkStream Stream { get; set; }

        public int totalBytesRead = 0;

        public string readType = null;

        public byte[] buffer = new byte[BufferSize];

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

        public const string ScadaDataClient = "scada.data.client";

        // Wired connection Tcp client
        private TcpClient client = null;

        // Wireless connection Tcp client
        private TcpClient wirelessClient = null;

        // Maybe it uses wired connection, or wireless.
        private NetworkStream stream;

        // the current data handler.
        private MessageDataHandler handler;

        private const int Timeout = 5000;

        private LoggerClient logger = new LoggerClient();

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
            this.handler = new MessageDataHandler(this);
            LoggerClient.Initialize();
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

        private void Log(string fileName, string msg)
        {
            if (LoggerClient.Contains(fileName))
            {
                this.logger.Send(fileName, string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), msg));
            }
        }

        public override string ToString()
        {
            return (this.client != null) 
                ? string.Format("{0}:{1}", this.ServerAddress, this.ServerPort) 
                : string.Format("{0}:{1}", this.WirelessServerAddress, this.WirelessServerPort);
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

                    this.Log(ScadaDataClient, string.Format("Connecting to {0} retry times = {1}.", this.ToString(), this.retryCount));
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

                    this.Log(ScadaDataClient, string.Format("Connecting to {0} <wireless> retry times = {1}.", this.ToString(), this.retryCount));                    
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
                this.client.Close();
            }
            catch (Exception e)
            {
                this.NotifyEvent(this, NotifyEvents.ConnectError, "Disconnect(): " + e.Message);
            }

            try
            {
                this.wirelessClient.Close();
            }
            catch (Exception e)
            {
                this.NotifyEvent(this, NotifyEvents.ConnectError, "Disconnect(): <wireless> " + e.Message);
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

                    if (client.Connected)
                    {
                        this.BeginRead(client);

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
                        string connectedMessage = string.Format("Connected to {0}", this.ToString());
                        this.Log(ScadaDataClient, connectedMessage);

                        this.BeginRead(this.client);

                        
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
                    SessionState session = new SessionState(client, stream);
                    IAsyncResult ar = stream.BeginRead(session.buffer, 0, SessionState.BufferSize,
                        new AsyncCallback(OnReadCallback), session);
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
                SessionState session = (SessionState)result.AsyncState;
                try
                {
                    int c = session.Stream.EndRead(result);
                    this.DoReceivedMessages(session.GetReceivedMessage(c));
                    this.BeginRead(session.Client);
                }
                catch (Exception e)
                {
                    this.Log(ScadaDataClient, e.ToString());
                    this.CloseClient();
                }
            }
        }

        private void DoReceivedMessages(string messages)
        {
            if (this.handler == null || string.IsNullOrEmpty(messages))
            {
                return;
            }
            string[] msgs = messages.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string msg in msgs)
            {
                if (msg.Trim() != string.Empty)
                {
                    this.ShowReceivedMessage(msg);
                    this.handler.OnMessageDispatcher(msg);
                }
            }
        }

        private void ShowReceivedMessage(string msg)
        {
            if ("6031" == Value.Parse(msg, "CN"))
            {
                return;
            }
            this.Log(ScadaDataClient, msg);
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
            catch(Exception e)
            {
                this.CloseClient();
                return false;
            }
            return false;
        }

        private void CloseClient()
        {
            try
            {
                if (this.stream != null)
                {
                    this.stream.Close();
                    this.stream = null;
                }
            }
            catch (Exception) { }

            try
            {
                if (this.client != null)
                {
                    this.client.Close();
                    this.client = null;
                }
            }
            catch (Exception) { }

            try
            {
                if (this.wirelessClient != null)
                {
                    this.wirelessClient.Close();
                    this.wirelessClient = null;
                }
            }
            catch (Exception) { }

            this.stream = null;
            this.client = null;
            this.wirelessClient = null;
            this.RetryConnection();
        }

        // A. 每30秒试图重连一次
        // B. 6次连接失败, 则选择无线方式
        // C. 无线连接也失败, 则重新测试连接无线4次
        // D. 无线也连接不上, 则回到A步骤
        private void RetryConnection()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
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

        internal bool SendDataPacket(DataPacket p)
        {
            if (p == null)
                return false;

            if (this.SendDataStarted)
            {
                string s = p.ToString();
                return this.Send(Encoding.ASCII.GetBytes(s));
            }
            return false;
        }

        internal bool SendHistoryDataPacket(DataPacket p)
        {
            if (p == null)
                return false;

            if (this.OnHistoryData)
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

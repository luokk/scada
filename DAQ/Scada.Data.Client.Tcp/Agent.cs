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
        Connecting,
        Connected,
        Disconnect,
        Received,
        Sent,
        SentHistoryData,
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
        private bool? IsWired
        {
            get;
            set;
        }

        private bool isConnectingWired = false;

        public bool IsRetryConnection
        {
            get;
            set;
        }

        // Maybe it uses wired connection, or wireless.
        public NetworkStream Stream
        {
            get;
            set;
        }

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

            this.IsRetryConnection = true;
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

        private void DoLog(string fileName, string msg)
        {
            if (this.MainThreadSyncContext == null)
                return;
            this.MainThreadSyncContext.Post(new SendOrPostCallback((o) => 
            {
                string line = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), msg);
                if (LoggerClient.Contains(fileName))
                {
                    this.logger.Send(fileName, line);
                }
                Logger logger = Log.GetLogFile(fileName);
                if (logger != null)
                {
                    logger.Log(line);
                }
            }), null);
        }

        public override string ToString()
        {
            if (this.IsWired.HasValue)
            {
                return this.IsWired.Value ? string.Format("{0}:{1}", this.ServerAddress, this.ServerPort) : string.Format("{0}:{1}", this.WirelessServerAddress, this.WirelessServerPort);
            }
            return "<No-connection>";
        }

        private void OnConnectionException(Exception e)
        {
            this.Disconnect();
            if (this.IsRetryConnection)
            {
                this.RetryConnection();
            }
        }


        public void Connect()
        {
            this.isConnectingWired = true;
            this.Connect(this.ServerAddress, this.ServerPort);
        }

        public void ConnectToWireless()
        {
            this.isConnectingWired = false;
            this.Connect(this.WirelessServerAddress, this.WirelessServerPort);
        }

        private void Connect(string serverIpAddress, int serverPort)
        {
            try
            {
                if (this.client == null)
                {
                    this.client = new TcpClient();
                    this.client.ReceiveTimeout = Timeout;

                    this.client.BeginConnect(serverIpAddress, serverPort,
                        new AsyncCallback(ConnectCallback),
                        this.client);

                    string msg = string.Format("Connecting to {0}:{1} retry times = {2}.", serverIpAddress, serverPort, this.retryCount);
                    this.DoLog(ScadaDataClient, msg);
                    this.NotifyEvent(this, NotifyEvents.Connecting, msg);
                }
                else
                {
                    // TODO: Multi-thread;
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Connecting to {0}:{1} failed => {2}", serverIpAddress, serverPort, e.Message);
                this.DoLog(ScadaDataClient, msg);
                this.NotifyEvent(this, NotifyEvents.Connecting, msg);
                this.OnConnectionException(e);
            }
        }

        internal void Disconnect()
        {
            this.IsWired = null;
            try
            {
                this.Stream = null;
                if (this.client != null)
                {
                    this.client.Close();
                }
                string msg = string.Format("Disconnect from {0}", this.ToString());
                this.DoLog(ScadaDataClient, msg);
                this.NotifyEvent(this, NotifyEvents.Disconnect, msg);
            }
            catch (Exception e)
            {
                string msg = string.Format("Disconnect from {0} Failed => {1}", this.ToString(), e.Message);
                this.DoLog(ScadaDataClient, msg);
                this.NotifyEvent(this, NotifyEvents.Disconnect, msg);
            }

            this.client = null;
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (!result.IsCompleted || this.MainThreadSyncContext == null)
            {
                return;
            }
            this.MainThreadSyncContext.Post(new SendOrPostCallback((o) => 
            { 
                try
                {
                    TcpClient client = (TcpClient)result.AsyncState;
                    if (client != null)
                    {
                        client.EndConnect(result);

                        if (client.Connected)
                        {
                            // Send need this.stream
                            this.Stream = client.GetStream();

                            this.IsWired = this.isConnectingWired;

                            this.BeginRead(client, this.Stream);

                            // [Auth]
                            this.handler.SendAuthPacket();

                            string msg = string.Format("Connected to {0}", this.ToString());
                            this.DoLog(ScadaDataClient, msg);
                            this.NotifyEvent(this, NotifyEvents.Connected, msg);
                        }
                    }
                }
                catch (Exception e)
                {
                    string address = this.isConnectingWired ? string.Format("{0}:{1}", this.ServerAddress, this.ServerPort) : string.Format("{0}:{1}", this.WirelessServerAddress, this.WirelessServerPort);            
                    string msg = string.Format("Connected to {0} Failed => {1}", address, e.Message);
                    this.DoLog(ScadaDataClient, msg);
                    this.NotifyEvent(this, NotifyEvents.Connected, msg); 
                    
                    this.OnConnectionException(e);
                }
            }), null);
        }

        // BeginRead~ <client>
        private void BeginRead(TcpClient client, NetworkStream stream)
        {
            this.MainThreadSyncContext.Post(new SendOrPostCallback((o) =>
            {
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
                        this.OnConnectionException(e);
                    }
                }
            }), null);
        }

        private void OnReadCallback(IAsyncResult result)
        {
            this.MainThreadSyncContext.Post(new SendOrPostCallback((o) =>
            {
                if (result.IsCompleted)
                {
                    SessionState session = (SessionState)result.AsyncState;
                    try
                    {
                        int c = session.Stream.EndRead(result);
                        // Log handled in this function
                        this.DoReceivedMessages(session.GetReceivedMessage(c));
                        this.BeginRead(session.Client, session.Stream);
                    }
                    catch (Exception e)
                    {
                        this.OnConnectionException(e);
                    }
                }
            }), null);
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
                // Not not record KeepAlive.
                return;
            }
            this.DoLog(ScadaDataClient, msg);
            this.NotifyEvent(this, NotifyEvents.Received, msg);
        }

        // Send final implements
        private bool Send(byte[] message)
        {
            try
            {
                this.Stream.Write(message, 0, message.Length);
                return true;
            }
            catch(Exception e)
            {
                this.OnConnectionException(e);
                return false;
            }
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
                this.NotifyEvent(this, NotifyEvents.SentHistoryData, p.DeviceKey);
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


        internal void SetSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            this.MainThreadSyncContext = synchronizationContext;  
        }

        public SynchronizationContext MainThreadSyncContext { get; set; }
    }
}

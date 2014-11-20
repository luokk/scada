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
        public const int BufferSize = 2048;

        public SessionState(TcpClient client, NetworkStream stream)
        {
            this.Client = client;
            this.Stream = stream;
        }

        public string GetReceivedMessage(int size)
        {
            if (size > 0 && size <= BufferSize)
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
        ConnectedCountry,
        Disconnect,
        DisconnectCountry,
        Disconnect2,
        BeginRead,
        EndRead,
        Received,
        HistoryDataSent,
        HandleEvent,
        ConnectToCountryCenter,
        DisconnectToCountryCenter,
    }

    public enum Type
    {
        Province = 1,
        Country = 2,
    }

    public class ThreadMashaller
    {
        public ThreadMashaller(SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
            this.threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void Mashall(Action<object> action)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.threadId)
            {
                action(null);
            }
            else
            {
                this.synchronizationContext.Post(new SendOrPostCallback(action), null);
            }
        }

        private int threadId;

        private SynchronizationContext synchronizationContext;
    }

    // public delegate void OnReceiveMessage(Agent agent, string msg);

    public delegate void NotifyEventHandler(Agent agent, NotifyEvents ne, string msg1, string msg2);

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

        internal bool SendDataDirectlyStartedByError
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
            this.ConnectRetryRoutine();
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

        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Optimze: Only has logger.server process, the module would try HTTP connection.
        /// Detect process every 15 sec, not every time, 
        /// </summary>
        private static long lastDetectTimeTicks = 0;

        private static bool lastDetectResult = false;

        public static void ResetDetectTime()
        {
            lastDetectTimeTicks = 0;
        }

        private static bool ExistLoggerConsoleProc()
        {
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - lastDetectTimeTicks > (15 * 10000000))
            {
                lastDetectTimeTicks = nowTicks;
                Process[] ps = Process.GetProcessesByName(@"Scada.Logger.Server");
                lastDetectResult = (ps != null && ps.Length > 0);
                return lastDetectResult;
            }
            return lastDetectResult;
        }

        internal void DoLog(string deviceKey, string msg)
        {
            if (this.UIThreadMashaller == null)
                return;

            this.UIThreadMashaller.Mashall((_n) => 
            {
                if (ExistLoggerConsoleProc() && LoggerClient.Contains(deviceKey))
                {
                    this.logger.Send(deviceKey, msg);
                }
                Logger logger = Log.GetLogFile(deviceKey);
                if (logger != null)
                {
                    logger.Log(msg);
                }
            });
        }

        public void OnTimeChanged(DateTime timeToSet)
        {
            string s = string.Format("SET TIME = {0}", timeToSet);
            this.NotifyEvent(this, NotifyEvents.HandleEvent, s, null);
        }

        public void OnHandleHistoryData(string deviceKey, string msg, bool notify)
        {
            deviceKey = string.IsNullOrEmpty(deviceKey) ? ScadaDataClient : deviceKey;
            this.DoLog(deviceKey, msg);
            if (notify)
            {
                this.NotifyEvent(this, NotifyEvents.HandleEvent, deviceKey, msg);
            }
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
            // this.TryToPing();
        }

        private IPStatus GetPingResult(string serverIpAddress)
        {
            return IPStatus.Success;
            /* 虎林站问题: ping不通, 但是可以连接.
            Ping ping = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "Ping...";
            byte[] buffer = Encoding.ASCII.GetBytes(data);  
            PingReply reply = ping.Send(serverIpAddress, 200, buffer, options);
            return reply.Status;
             * */
        }

        private void TryToPing()
        {
            new Thread(new ParameterizedThreadStart((o) => 
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;

                p.Start();

                string cmd = string.Format("ping -n 1 {0}", this.ServerAddress);
                p.StandardInput.WriteLine(cmd);

                p.StandardInput.WriteLine("exit");
                string content = p.StandardOutput.ReadToEnd();
                // TODO: lines break
                string[] lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var l = line.Trim();
                    if (l.StartsWith("Reply") || l.StartsWith("Ping request"))
                    {
                        DoLog(ScadaDataClient, l);
                    }
                }

                p.WaitForExit();
                int n = p.ExitCode;  // n 为进程执行返回值  
                p.Close();
            })).Start(null);

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
                    if (this.Type == Type.Province)
                    {
                        this.NotifyEvent(this, NotifyEvents.Connecting, msg, null);
                    }
                }
                else
                {
                    string msg = string.Format("Connecting to {0}:{1} Already!!", serverIpAddress, serverPort);
                    this.DoLog(ScadaDataClient, msg);
                    if (this.Type == Type.Province)
                    {
                        this.NotifyEvent(this, NotifyEvents.Connecting, msg, null);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Connecting to {0}:{1} failed => {2}", serverIpAddress, serverPort, e.Message);
                this.DoLog(ScadaDataClient, msg);
                this.NotifyEvent(this, NotifyEvents.Connecting, msg, null);
                this.OnConnectionException(e);
            }
        }

        internal void Disconnect()
        {
            this.IsWired = null;
            try
            {
                if (this.Stream != null)
                {
                    this.Stream.Close();
                }

                if (this.client != null)
                {
                    this.client.Close();
                }
                string msg = string.Format("Disconnect from {0}", this.ToString());
                this.DoLog(ScadaDataClient, msg);
                if (this.Type == Type.Country)
                {
                    this.NotifyEvent(this, NotifyEvents.Disconnect, msg, null);
                }
                else
                {
                    this.NotifyEvent(this, NotifyEvents.DisconnectCountry, msg, null);
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Disconnect from {0} Failed => {1}", this.ToString(), e.Message);
                this.DoLog(ScadaDataClient, msg);
                this.NotifyEvent(this, NotifyEvents.Disconnect2, msg, null);
            }
            this.Stream = null;
            this.client = null;
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
                        // Send need this.stream
                        this.Stream = client.GetStream();

                        this.IsWired = this.isConnectingWired;

                        this.BeginRead(client, this.Stream);

                        // [Auth]
                        this.handler.SendAuthPacket();

                        string msg = string.Format("Connected to {0}", this.ToString());
                        this.DoLog(ScadaDataClient, msg);
                        if (this.Type == Type.Country)
                        {
                            this.NotifyEvent(this, NotifyEvents.ConnectedCountry, msg, null);
                        }
                        else
                        {
                            this.NotifyEvent(this, NotifyEvents.Connected, msg, null);
                        }

                        this.NotifyEvent(this, NotifyEvents.HandleEvent, msg, null);
                    }   
                }
                catch (Exception e)
                {
                    string address = this.isConnectingWired ? string.Format("{0}:{1}", this.ServerAddress, this.ServerPort) : string.Format("{0}:{1}", this.WirelessServerAddress, this.WirelessServerPort);
                    string msg = string.Format("Connected to {0} Failed => {1}", address, e.Message);
                    this.DoLog(ScadaDataClient, msg);

                    if (this.Type == Type.Country)
                    {
                        this.NotifyEvent(this, NotifyEvents.DisconnectCountry, msg, null);
                    }
                    else
                    {
                        this.NotifyEvent(this, NotifyEvents.Disconnect, msg, null);
                    }

                    this.OnConnectionException(e);
                }

            }
        }

        // BeginRead~ <client>
        private void BeginRead(TcpClient client, NetworkStream stream)
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
                    string msg = string.Format("BeginRead from {0} Failed => {1}", this.ToString(), e.Message);
                    this.DoLog(ScadaDataClient, msg);
                    // this.NotifyEvent(this, NotifyEvents.BeginRead, msg); 

                    this.OnConnectionException(e);
                }
            }
        }

        private void OnReadCallback(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                int size = 0;
                try
                {
                    SessionState session = (SessionState)result.AsyncState;

                    Thread.Sleep(100);
                    size = session.Stream.EndRead(result);
                    if (size > 0)
                    {
                        string content = session.GetReceivedMessage(size);

                        this.DoReceivedMessages(content);
                    }
                    // Log handled in this function
                    
                    this.BeginRead(session.Client, session.Stream);
                }
                catch (Exception e)
                {
                    string msg = string.Format("EndRead from {0} Failed => {1} Size={2}", this.ToString(), e.Message, size);
                    this.DoLog(ScadaDataClient, msg);
                    // this.NotifyEvent(this, NotifyEvents.EndRead, msg); 

                    this.OnConnectionException(e);
                }
            }
        }

        internal void DoReceivedMessages(string messages)
        {
            if (this.handler == null || string.IsNullOrEmpty(messages))
            {
                return;
            }

            string[] msgs = messages.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string msg in msgs)
            {
                this.ShowReceivedMessage(msg.Trim());
                if (msg.EndsWith("\r") || msg.EndsWith("\n"))
                {
                    this.handler.OnMessageDispatcher(msg);
                }
                else
                {
                    // 不完整的包!
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
            this.DoLog(ScadaDataClient, string.Format("<--: {0}", msg));
            this.NotifyEvent(this, NotifyEvents.Received, msg, null);
        }

        // Send final implements
        private bool Send(byte[] message)
        {
            try
            {
                if (this.Stream != null)
                {
                    this.Stream.Write(message, 0, message.Length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                string msg = string.Format("Sent to {0} Failed => {1}", this.ToString(), e.Message);
                this.DoLog(ScadaDataClient, msg);

                this.OnConnectionException(e);
            }
            return false;
        }

        // A. 每30秒试图重连一次
        // B. 6次连接失败, 则选择无线方式
        // C. 无线连接也失败, 则重新测试连接无线4次
        // D. 无线也连接不上, 则回到A步骤
        private void ConnectRetryRoutine()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 6000; // 6 seconds
            timer.Elapsed += (s, e) => 
            {
                if (this.client != null)
                {
                    Console.WriteLine("ConnectRetryRoutine client != null");
                    return;
                }

                if (this.UIThreadMashaller != null)
                {
                    this.retryCount++;
                    if (this.GetPingResult(this.ServerAddress) == IPStatus.Success)
                    {
                        this.UIThreadMashaller.Mashall((o) =>
                        {
                            this.DoLog(ScadaDataClient, "ConnectRetryRoutine");
                            Console.WriteLine("ConnectRetryRoutine");
                            this.Connect();
                        });   
                    }
                    else if (this.GetPingResult(this.WirelessServerAddress) == IPStatus.Success)
                    {
                        this.UIThreadMashaller.Mashall((o) =>
                        {
                            this.ConnectToWireless();
                        });
                    }
                }

            };

            timer.Start();
        }

        internal bool SendPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            // TODO:!
            this.DoLog(Agent.ScadaDataClient, string.Format("-->: {0}", s));
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
            bool result = false;

            string s = p.ToString();
            result = this.Send(Encoding.ASCII.GetBytes(s));
            if (result)
            {
                if (this.NotifyEvent != null && !string.IsNullOrEmpty(p.DeviceKey))
                {
                    this.NotifyEvent(this, NotifyEvents.HistoryDataSent, p.DeviceKey, s);
                }
            }
            return result;
        }

        internal void SendExceptionNotify(DataPacket packet)
        {
            this.SendPacket(packet);
        }

        internal bool SendReplyPacket(DataPacket p, DateTime time)
        {
            string s = p.ToString();
            return this.Send(Encoding.ASCII.GetBytes(s));
        }

        internal void StartConnectCountryCenter(bool sendDataDirectlyStartedByError = false)
        {
            this.SendDataDirectlyStarted = true;
            this.SendDataDirectlyStartedByError = sendDataDirectlyStartedByError;
            string msg = string.Format("启动到国家数据中心的连接!");
            this.NotifyEvent(this, NotifyEvents.ConnectToCountryCenter, msg, null);
        }

        internal void StopConnectCountryCenter(bool sendDataDirectlyStartedByError = false)
        {
            if (this.SendDataDirectlyStartedByError == sendDataDirectlyStartedByError)
            {
                this.SendDataDirectlyStarted = false;
                string msg = string.Format("国家数据中心连接已断开");
                this.NotifyEvent(this, NotifyEvents.DisconnectToCountryCenter, msg, null);
            }
        }

        public ThreadMashaller UIThreadMashaller
        {
            get;
            set;
        }

        internal void Quit()
        {
            this.handler.Quit();
            this.Disconnect();
            this.DoLog(ScadaDataClient, "<Quit>");
        }

        public bool CanHandleSetTime 
        {
            get
            {
                return this.handler.CanHandleSetTime;
            }
            set
            {
                this.handler.CanHandleSetTime = value;
            }
        }
    }
}

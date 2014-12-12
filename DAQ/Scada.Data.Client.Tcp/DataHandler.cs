using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Scada.Data.Client.Tcp
{
    // Command for received.
    public enum ReceivedCommand
    {
        Unknown = -1,
        None = 0,
        SetPassword = 1072,
        GetTime = 1011,
        SetTime = 1012,
        GetAlertThreshold = 1021,
        SetAlertThreshold = 1022,
        GetFactor = 1028,
        StartSendData =  2011,
        StopSendData = 2012,

        GetFlowData = 2013,
        GetRunStatus = 2023,
        HistoryData = 2042,
        HistoryData2 = 2043,
        GetException = 2017,
        GetAlertHistory = 2071,
        StartSendDataDirectly = 3101,
        StopSendDataDirectly = 3102,
        Init = 6021,
        KeepAlive = 6031,
        StartDev = 3012,
        StopDev = 3015,

        SetNewIp = 3103,
        ApplyNewIp = 3104,
        SetFreq = 3105,
        
        Reply = 9012
    }

    public enum SentCommand
    {
        None = 0,
        GetTime = 1011,
        Data = 2011,
        FlowData = 2014,
        DoorState = 2015,   // TODO:

        RunStatus = 2023,
        HistoryData = 2042,
        WentException = 2073,
        AfterException = 2074,
        Auth = 6011,
        KeepAlive = 6031,
        Reply = 9011,
        Result = 9012,
        Notify = 9013,
        GetAlertThreshold = ReceivedCommand.GetAlertThreshold,

    }

    [StructLayout(LayoutKind.Sequential)]
    public class SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort whour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }

    class HistoryDataBundle
    {
        public string BeginTime
        {
            get;
            set;
        }

        public string EndTime
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public string QN
        {
            get;
            private set;
        }

        public string ENO
        {
            get;
            private set;
        }

        public string PolId
        {
            get;
            private set;
        }

        public string Sid
        {
            get;
            set;
        }


        public HistoryDataBundle(string qn, string eno, string polId)
        {
            this.Count = 0;
            this.QN = qn;
            this.ENO = eno;
            this.PolId = polId;
        }



    }

    /// <summary>
    /// DataHandler
    /// </summary>
    class MessageDataHandler
    {
        private DataPacketBuilder builder = new DataPacketBuilder();

        // Agent ref.
        private Agent agent;

        private SamplerController hvsc = new SamplerController("scada.mds");

        private SamplerController isc = new SamplerController("scada.ais");

        private Dictionary<string, HistoryDataBundle> taskDict = new Dictionary<string, HistoryDataBundle>();

        // Win32 API
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(SystemTime st);

        [DllImport("Kernel32.dll")]
        public static extern void SetLocalTime(SystemTime st);

        private SentCommand CurrentSentCommand
        {
            get;
            set;
        }

        public MessageDataHandler(Agent agent)
        {
            this.agent = agent;
        }

        public void SendAuthPacket()
        {
            // TODO:QN=20090516010101001;ST=38;CN=6011;PW=123456;
            // MN=0101A010000000;Flag=1;CP=&&&&
            var p = this.builder.GetAuthPacket();
            this.agent.SendPacket(p);
            this.CurrentSentCommand = SentCommand.Auth;
        }

        public void SendKeepAlivePacket()
        {
            // QN=20090516010101001;ST=38;CN=6031;PW=123456;
            // MN=0101A010000000;CP=&&&&
            var p = this.builder.GetKeepAlivePacket();
            this.agent.SendPacket(p);
        }

        public void SendReplyPacket(string qn)
        {
            var p = this.builder.GetReplyPacket(qn);
            
            this.agent.SendPacket(p, default(DateTime));
        }

        private void SendResultPacket(string qn, int result = 1)
        {
            var p = this.builder.GetResultPacket(qn, result);

            this.agent.SendPacket(p, default(DateTime));
        }

        private void SendNotifyPacket(string qn)
        {
            var p = this.builder.GetNotifyPacket(qn);
            this.agent.SendPacket(p, default(DateTime));
        }
        

        public void OnMessageDispatcher(string msg)
        {
            if (!(msg.EndsWith("\r") || msg.EndsWith("\n")))
            {
                // Imcompleted Packet
                return;
            }
            ReceivedCommand code = (ReceivedCommand)ParseCommandCode(msg);

            switch (code)
            {
                // 设置系统时间
                case ReceivedCommand.SetTime:
                    {
                        this.OnSetTime(msg);
                    }
                    break;
                // 获得系统时间
                case ReceivedCommand.GetTime:
                    {
                        this.OnGetTime(msg);
                    }
                    break;
                // 设置密码
                case ReceivedCommand.SetPassword:
                    {
                        this.HandleSetPassword(msg);
                    }
                    break;
                // 开始
                case ReceivedCommand.StartSendData:
                    {
                        this.OnStartSendData(msg);
                    }
                    break;
                // 结束
                case ReceivedCommand.StopSendData:
                    {
                        this.OnStopSendData(msg);
                    }
                    break;
                // 历史数据
                case ReceivedCommand.HistoryData:
                case ReceivedCommand.HistoryData2:
                    {
                        this.HandleHistoryData(msg);
                    }
                    break;
                // 大流量数据(历史 2013)
                case ReceivedCommand.GetFlowData:
                    {
                        this.HandleFlowData(msg);
                    }
                    break;
                // 运行状态
                case ReceivedCommand.GetRunStatus:
                    this.GetRunStatus(msg);
                    break;
                // 异常??
                case ReceivedCommand.GetException:
                    this.GetException(msg);
                    break;
                case ReceivedCommand.GetAlertHistory:
                    this.GetAlertHistory(msg);
                    break;
                case ReceivedCommand.GetAlertThreshold:
                    this.GetAlertThreshold(msg);
                    break;
                case ReceivedCommand.SetAlertThreshold:
                    this.SetAlertThreshold(msg);
                    break;
                // 直接数据
                case ReceivedCommand.StartSendDataDirectly:
                    {
                        // Debug.Assert(this.agent.Type != Type.Country);
                        // if (this.agent.Type != Type.Country)
                        {
                           // this.OnStartSendDataDirectly(msg);
                        }
                    }
                    break;
                // 停止直接数据
                case ReceivedCommand.StopSendDataDirectly:
                    {
                        Debug.Assert(this.agent.Type != Type.Country);
                        if (this.agent.Type != Type.Country)
                        {
                            this.OnStopSendDataDirectly(msg);
                        }
                    }
                    break;
                // 初始化
                case ReceivedCommand.Init:
                    {
                        this.OnInitializeRequest(msg);
                    }
                    break;
                // 心跳包
                case ReceivedCommand.KeepAlive:
                    {
                        this.OnKeepAlive(msg);
                    }
                    break;
                // 启动设备
                case ReceivedCommand.StartDev:
                    {
                        this.OnStartDevice(msg);
                        hvsc.Start();
                    }
                    break;
                // 停止设备
                case ReceivedCommand.StopDev:
                    {
                        this.OnStopDevice(msg);
                    }
                    break;
                // Server Reply
                case ReceivedCommand.Reply:
                    {
                        this.OnServerReply(msg);
                    }
                    break;
                case ReceivedCommand.SetNewIp:
                    {
                        this.AddNewIpAddress(msg);
                    }
                    break;
                case ReceivedCommand.ApplyNewIp:
                    {
                        this.AddNewIpAddress(msg, true);
                    }
                    break;
                case ReceivedCommand.SetFreq:
                    {
                        this.SetFrequence(msg);
                    }
                    break;
                case ReceivedCommand.GetFactor:
                    {
                        this.GetFactor(msg);
                    }
                    break;
                // Error!
                case ReceivedCommand.None:
                case ReceivedCommand.Unknown:
                default:
                    break;
            }
        }

        private void SetAlertThreshold(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            string polId = Value.Parse(msg, "PolId");

            string ut = Value.Parse(msg, string.Format("{0}-UseType", polId));
            string th1 = Value.Parse(msg, string.Format("{0}-LowValue", polId));
            string th2 = Value.Parse(msg, string.Format("{0}-UpValue", polId));
            try
            {
                Settings.Instance.SetThreshold(polId, th1, th2);
            }
            catch
            {
                this.agent.DoLog(Agent.ScadaDataClient, "Failed to [SetAlertThreshold]");
            }

            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void GetAlertThreshold(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            string eno = Value.Parse(msg, "ENO");
            string polId = Value.Parse(msg, "PolId");
            this.SendReplyPacket(qn);
            string v1, v2;
            if (Settings.Instance.GetThreshold(polId, out v1, out v2))
            {
                DataPacket p = this.builder.GetThresholdPacket(polId, eno, v1, v2);
                this.agent.SendPacket(p);
                this.SendResultPacket(qn);
            }
            else
            {
                this.SendResultPacket(qn, 0);
            }
        }

        // TODO: 采样周期(无实现)
        private void SetFrequence(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void GetFactor(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void AddNewIpAddress(string msg, bool apply = false)
        {
            string centerType = Value.Parse(msg, "CenterType");
            string wirelessIp = Value.Parse(msg, "WirelessIp");
            string wirelessPort = Value.Parse(msg, "WirelessPort");
            string wireIp = Value.Parse(msg, "WireIp");
            string wirePort = Value.Parse(msg, "WirePort");

            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);

            Settings.Instance.AddNewIpAddress(wireIp, wirePort, wirelessIp, wirelessPort, centerType == "2");
            if (apply)
            {
                this.agent.DoLog(Agent.ScadaDataClient, "Password changed!");
            }

            this.SendResultPacket(qn);
        }

        private void GetRunStatus(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            
            // 获得运行状态包
            var packet = this.builder.GetRunStatusPacket(qn,
                Settings.Instance.DeviceKeys,
                new string[] { "1", "1", "1", "1", "1", "1", "1" });
            this.agent.SendPacket(packet);

            string runStatus = string.Format("RUNNING-STATUS: {0}", packet.ToString());
            this.agent.DoLog(Agent.ScadaDataClient, runStatus);
            // 
            this.SendResultPacket(qn);
        }

        private void GetException(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void GetAlertHistory(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void OnGetTime(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);

            var p = this.builder.GetTimePacket(qn);
            this.agent.SendPacket(p, default(DateTime));
            this.SendResultPacket(qn);
        }

        private void OnSetTime(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);

            string time = Value.Parse(msg, "SystemTime");
            DateTime dt = DeviceTime.Parse(time);

            this.agent.OnTimeChanged(dt);

            if (this.CanHandleSetTime)
            {
                SystemTime st = new SystemTime();

                st.wYear = (ushort)dt.Year;
                st.wMonth = (ushort)dt.Month;
                st.wDay = (ushort)dt.Day;
                st.whour = (ushort)dt.Hour;
                st.wMinute = (ushort)dt.Minute;
                st.wSecond = (ushort)dt.Second;
                st.wMilliseconds = 0;

                SetLocalTime(st);
            }
            this.SendResultPacket(qn);
        }

        private void OnServerReply(string msg)
        {
            // TODO:
            string ret = Value.Parse(msg, "ExeRtn");
            if (this.CurrentSentCommand == SentCommand.Auth)
            {

            }
            else if (this.CurrentSentCommand == SentCommand.Data)
            {

            }
            else if (this.CurrentSentCommand == SentCommand.HistoryData)
            {

            }

            // After reply, reset the Current Sent Command
            this.CurrentSentCommand = SentCommand.None;
        }

        private Queue<string> historyDataBundleQueue = new Queue<string>();

        private Thread uploadHistoryDataThread;

        private bool fQuit = false;

        private void ActiveUploadHistoryDataThread(string taskId)
        {
            this.historyDataBundleQueue.Enqueue(taskId);
            if (this.uploadHistoryDataThread == null)
            {
                this.uploadHistoryDataThread = new Thread(new ThreadStart(this.PrepareUploadHistoryData));
                this.uploadHistoryDataThread.Start();
            }
        }

        private void PrepareUploadHistoryData()
        {
            while (true)
            {
                if (this.fQuit)
                    break;
                if (this.historyDataBundleQueue.Count > 0)
                {
                    string taskId = this.historyDataBundleQueue.Dequeue();
                    if (!this.taskDict.ContainsKey(taskId))
                    {
                        continue;
                    }
                    HistoryDataBundle hdb = this.taskDict[taskId];
                    
                    if (!string.IsNullOrEmpty(hdb.QN))
                    {
                        string msg = string.Format("Uploading history data [{0} - {1}]", DeviceTime.Parse(hdb.BeginTime), DeviceTime.Parse(hdb.EndTime));
                        this.agent.OnHandleHistoryData("", msg, true);
                        if (string.IsNullOrEmpty(hdb.ENO))
                        {
                            string[] enos = new string[] { "001001", "002000", "003001", "004000", "005000", "010002", "999000" };
                            foreach (string e in enos)
                            {
                                // for this Command, polId should be Null (means All polId);
                                this.UploadHistoryData(hdb.QN, e, hdb.BeginTime, hdb.EndTime, hdb.Sid, null);
                            }
                        }
                        else
                        {
                            this.UploadHistoryData(hdb.QN, hdb.ENO, hdb.BeginTime, hdb.EndTime, hdb.Sid, hdb.PolId);
                        }
                    }

                    this.SendResultPacket(hdb.QN);
                    // 
                    this.taskDict.Remove(taskId);
                }
                else
                {
                    Thread.Sleep(150);
                }
            }
        }

        private void HandleFlowData(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            string sno = Value.Parse(msg, "SNO");
            string eno = Value.Parse(msg, "ENO");

            string sid = Value.Parse(msg, "WorkID");

            string beginTime = Value.Parse(msg, "BeginTime");
            string endTime = Value.Parse(msg, "EndTime");
            string polId = Value.Parse(msg, "PolId");

            string taskId = string.Format("{0}-{1}@{2}:{3}", beginTime, endTime, polId, sid);

            if (this.taskDict.ContainsKey(taskId))
            {
                return;
            }

            this.SendReplyPacket(qn);

            HistoryDataBundle hdb = new HistoryDataBundle(qn, eno, polId);
            taskDict.Add(taskId, hdb);
            hdb.Sid = sid;
            hdb.BeginTime = beginTime;
            hdb.EndTime = endTime;

            this.ActiveUploadHistoryDataThread(taskId);
        }

        private void HandleHistoryData(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            string sno = Value.Parse(msg, "SNO");
            string eno = Value.Parse(msg, "ENO");

            string beginTime = Value.Parse(msg, "BeginTime");
            string endTime = Value.Parse(msg, "EndTime");

            string polId = Value.Parse(msg, "PolId");

            string taskId = string.Format("{0}-{1}@{2}", beginTime, endTime, polId);

            if (this.taskDict.ContainsKey(taskId))
            {
                return;
            }

            this.SendReplyPacket(qn);

            HistoryDataBundle hdb = new HistoryDataBundle(qn, eno, polId);
            taskDict.Add(taskId, hdb);
            hdb.BeginTime = beginTime;
            hdb.EndTime = endTime;

            this.ActiveUploadHistoryDataThread(taskId);
        }

        private void UploadHistoryData(string qn, string eno, string beginTime, string endTime, string sid, string polId)
        {
            DateTime f = DeviceTime.Parse(beginTime);
            DateTime t = DeviceTime.Parse(endTime);
            if (f >= t)
            {
                return;
            }
            string deviceKey = Settings.Instance.GetDeviceKeyByEno(eno);
            string deviceKeyLower = deviceKey.ToLower();
            
            if (deviceKey.Equals("Scada.NaIDevice", StringComparison.OrdinalIgnoreCase))
            {
                // NaIDevice ... Gose here.
                // 分包
                DateTime dt = f;
                while (dt <= t)
                {
                    string content = DBDataSource.Instance.GetNaIDeviceData(dt);
                    if (!string.IsNullOrEmpty(content))
                    {
                        List<DataPacket> pks = null;
                        try
                        {
                            pks = builder.GetDataPackets(deviceKey, dt, content, true);

                        }
                        catch (Exception e)
                        {
                            this.agent.OnHandleHistoryData(deviceKey, e.Message, true);
                        }

                        if (pks != null)
                        {
                            foreach (var p in pks)
                            {
                                this.agent.SendHistoryDataPacket(p);
                            }
                        }
                    }
                    
                    dt = dt.AddSeconds(60 * 5);
                }
            }
            else
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                ReadResult r = ReadResult.NoDataFound;
                string errorMessage = string.Empty;
                using (var conn = DBDataSource.Instance.CreateMySqlConnection())
                {
                    try
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        r = DBDataSource.GetData(cmd, deviceKey, f, t, sid, polId, data, out errorMessage);
                    }
                    catch (Exception e)
                    {
                        string line = string.Format("HD Error: {0} [{1}: {2} ~ {3}]", e.Message, deviceKey, f, t);
                        this.agent.OnHandleHistoryData(deviceKey, line, true);
                        return;
                    }
                }

                if (r == ReadResult.ReadOK)
                {
                    if (data.Count == 0)
                    {
                        return;
                    }
                    foreach (var item in data)
                    {
                        DataPacket p = null;
                        // By different device.

                        if (deviceKey.Equals("Scada.mds", StringComparison.OrdinalIgnoreCase) ||
                            deviceKey.Equals("Scada.ais", StringComparison.OrdinalIgnoreCase))
                        {
                            p = builder.GetFlowDataPacket(deviceKey, item, false);
                        }
                        else if (deviceKey.Equals("Scada.Shelter", StringComparison.OrdinalIgnoreCase))
                        {
                            p = builder.GetShelterPacket(deviceKey, item);
                        }
                        else
                        {
                            p = builder.GetDataPacket(deviceKey, item);
                        }

                        this.agent.SendHistoryDataPacket(p);
                    }
                }
                else
                {
                    string line = string.Format("HD Error: {0} - {1} [{2}: {3} ~ {4}]", r.ToString(), errorMessage, deviceKey, f, t);
                    this.agent.OnHandleHistoryData(deviceKey, line, true);
                }
            }
        }

        private void OnInitializeRequest(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
        }

        private void OnStartSendData(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.agent.SendDataStarted = true;
        }

        private void OnStopSendData(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendNotifyPacket(qn);

            this.agent.SendDataStarted = false;
        }

        private void OnStartDevice(string msg)
        {
            string eno = Value.Parse(msg, "ENO");
            string deviceKey = Settings.Instance.GetDeviceKeyByEno(eno);
            if (deviceKey.ToLower() == "Scada.mds".ToLower())
            {
                hvsc.Start();
            }
            else if (deviceKey.ToLower() == "Scada.ais".ToLower())
            {
                isc.Start();
            }
        }

        private void OnStopDevice(string msg)
        {
            string eno = Value.Parse(msg, "ENO");
            string deviceKey = Settings.Instance.GetDeviceKeyByEno(eno);
            if (deviceKey.ToLower() == Settings.DeviceKey_MDS.ToLower())
            {
                hvsc.Stop();
            }
            else if (deviceKey.ToLower() == Settings.DeviceKey_AIS.ToLower())
            {
                isc.Stop();
            }
        }

        private void OnStartSendDataDirectly(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
            Console.WriteLine("OnStartSendDataDirectly");
            // this.agent.StartConnectCountryCenter();
        }

        private void OnStopSendDataDirectly(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
            // this.agent.StopConnectCountryCenter();
        }

        private static int ParseCommandCode(string msg)
        {
            int code = 0;
            if (int.TryParse(Value.Parse(msg, "CN"), out code))
            {
                return code;
            }
            return 0;
        }

        private void HandleSetPassword(string msg)
        {
            string qn = Value.Parse(msg, "QN");
            this.SendReplyPacket(qn);
            this.SendResultPacket(qn);
            // Change password
            string newPasswd = Value.ParseInContent(msg, "PW");
            Settings.Instance.Password = newPasswd;
        }

        private void OnKeepAlive(string msg)
        {
            // TODO: Handle Timeout, but NO doc details talk about this.
        }

        internal void Quit()
        {
            this.fQuit = true;
        }

        public bool CanHandleSetTime { get; set; }
    }
}

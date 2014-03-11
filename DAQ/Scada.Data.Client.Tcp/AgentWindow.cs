using Microsoft.Win32;
using Scada.Config;
using Scada.DataCenterAgent.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Data.Client.Tcp
{
    public partial class AgentWindow : Form
    {
        public class DeviceDataDetails
        {

            public DeviceDataDetails()
            {
                this.SendDataCount = 0;
                this.LatestSendDataTime = default(DateTime);
                this.LatestSendHistoryDataTime = default(DateTime);
            }

            public string DeviceName
            {
                get;
                set;
            }

            public long SendDataCount
            {
                get;
                set;
            }

            public DateTime LatestSendDataTime
            {
                get;
                set;
            }


            public DateTime LatestSendHistoryDataTime
            {
                get;
                set;
            }
        }

        public class ConnetionRecord
        {
            public ConnetionRecord()
            {
                this.ConnectingTime = DateTime.Now;
                this.ConnectedTime = default(DateTime);
                this.DisconnectedTime = default(DateTime);
            }

            public override string ToString()
            {
                if (this.ConnectedTime != default(DateTime))
                {
                    // Connected
                    if (this.DisconnectedTime == default(DateTime))
                    {
                        return string.Format("<{0}> 连接", this.ConnectedTime);
                    }
                    else
                    {
                        return string.Format("<{0}> 连接; <{1}> 断开连接", this.ConnectedTime, this.DisconnectedTime);
                    }
                }
                else
                {
                    // Not connected
                    return string.Format("<{0}> 正在连接...", this.ConnectingTime);
                }
            }

            public DateTime ConnectingTime { get; set; }

            public DateTime ConnectedTime { get; set; }

            public DateTime DisconnectedTime { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private System.Windows.Forms.Timer timer;

        private System.Windows.Forms.Timer keepAliveTimer;

        private List<ConnetionRecord> connectionHistory = new List<ConnetionRecord>();

        private Agent agent = null;

        private Agent countryCenterAgent;

        private DataPacketBuilder builder = new DataPacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private LoggerClient logger = new LoggerClient();

        private Dictionary<string, object> data = new Dictionary<string, object>(20);

        private bool quitPressed = false;

        public bool StartState
        {
            get;
            set;
        }

        private bool started = false;

        public const string ScadaDataClient = "scada.data.client";

        public AgentWindow()
        {
            this.StartState = false;
            InitializeComponent();
        }

        private void AgentWindowLoad(object sender, EventArgs e)
        {
            LoggerClient.Initialize();
            this.logger.Send("ScadaDataClient", "Data (upload) Program starts at " + DateTime.Now);

            this.InitSysNotifyIcon();
            this.MakeWindowShownFront();
            this.ShowInTaskbar = false;
            this.SetTimeToolStripMenuItem.Checked = false;
            this.statusStrip.Items.Add(this.GetConnetionString());
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add("MS: " + Settings.Instance.Mn);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            // this.statusStrip.Items.Add("数据中心IP:");

            this.InitDetailsListView();
            if (this.StartState)
            {
                this.Start();
            }
        }

        private string GetConnetionString()
        {
            string address = "<NoConnection>";
            if (this.agent != null)
            {
                address = this.agent.ToString();
            }
            return string.Format("连接到{0}", address);
        }

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            sysNotifyIcon.Text = "数据上传";
            sysNotifyIcon.Icon = new Icon(Resources.AppIcon, new Size(16, 16));
            sysNotifyIcon.Visible = true;

            ContextMenu notifyContextMenu = new ContextMenu();
            MenuItem exitMenuItem = new MenuItem("退出");
            exitMenuItem.Click += (s, e) =>
            {
                this.PerformQuitByUser();
            };
            notifyContextMenu.MenuItems.Add(exitMenuItem);
            sysNotifyIcon.ContextMenu = notifyContextMenu;

            sysNotifyIcon.DoubleClick += new EventHandler(OnSysNotifyIconContextMenu);
        }

        private void OnSysNotifyIconContextMenu(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.MakeWindowShownFront();
            this.ShowAtTaskBar(true);
        }

        private void MakeWindowShownFront()
        {
            this.TopMost = true;
            this.Activate();
            this.TopMost = false;
        }

        private void Start()
        {
            if (this.started)
            {
                return;
            }
            DBDataSource.Instance.Initialize();
            this.InitializeAgents();
            this.InitializeTimer();
            this.started = true;
        }

        private void InitializeAgents()
        {
            Settings s = Settings.Instance;
            foreach (Settings.DataCenter dc in s.DataCenters)
            {
                if (dc.CountryCenter)
                {
                    // 国家中心
                    this.countryCenterAgent = CreateCountryCenterAgent(dc.Ip, dc.Port);
                    this.countryCenterAgent.AddWirelessInfo(dc.WirelessIp, dc.WirelessPort);
                    
                }
                else
                {
                    // 省中心
                    Agent agent = CreateAgent(dc.Ip, dc.Port, false);
                    agent.AddWirelessInfo(dc.WirelessIp, dc.WirelessPort);
                    SynchronizationContext synchronizationContext = SynchronizationContext.Current;
                    agent.UIThreadMashaller = new ThreadMashaller(synchronizationContext);
                    this.agent = agent;

                    this.agent.Connect();
                    this.agent.CanHandleSetTime = this.SetTimeToolStripMenuItem.Checked;
                }
            }
        }

        // 先连接有线的线路
        private Agent CreateAgent(string serverAddress, int serverPort, bool wireless)
        {
            Agent agent = new Agent(serverAddress, serverPort);
            agent.Type = Type.Province;
            agent.Wireless = wireless;
            agent.NotifyEvent += this.OnNotifyEvent;
            return agent;
        }

        // 国家中心 (Notice: 开始并不Connect, 区别于省)
        private Agent CreateCountryCenterAgent(string serverAddress, int serverPort)
        {
            Agent agent = new Agent(serverAddress, serverPort);
            agent.Type = Type.Country;
            agent.Wireless = false;
            agent.NotifyEvent += this.OnNotifyEvent;
            return agent;
        }

        private void InitializeTimer()
        {
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 4000;
            this.timer.Tick += this.SendDataTick;
            this.timer.Start();

            // KeepAlive timer per 30 sec
            this.keepAliveTimer = new System.Windows.Forms.Timer();
            this.keepAliveTimer.Interval = 1000 * 30;
            this.keepAliveTimer.Tick += this.KeepAliveTick;
            this.keepAliveTimer.Start();
        }


        private void DoLog(string fileName, string msg)
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
        }


        private void KeepAliveTick(object sender, EventArgs e)
        {
            DataPacket p = builder.GetKeepAlivePacket();
            if (agent.Stream != null)
            {
                agent.SendPacket(p);
            }
        }

        private void SendDataTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            foreach (var deviceKey in Settings.Instance.DeviceKeys)
            {
                if (IsDeviceSendTimeOK(now, deviceKey))
                {
                    DateTime sendTime = GetDeviceSendTime(now, deviceKey);

                    if (!this.lastDeviceSendData.ContainsKey(deviceKey))
                    {
                        this.lastDeviceSendData[deviceKey] = default(DateTime);
                    }

                    if (sendTime == this.lastDeviceSendData[deviceKey])
                    {
                        return;
                    }

                    this.lastDeviceSendData[deviceKey] = sendTime;

                    SendDataPackets(sendTime, deviceKey);
                }
            }
        }

        public void SendDataPackets(DateTime time, string deviceKey)
        {
            if (this.agent == null || !this.agent.SendDataStarted)
            {
                return;
            }

            if (deviceKey.Equals("Scada.NaIDevice", StringComparison.OrdinalIgnoreCase))
            {

                // NaI device packet;
                string content = DBDataSource.Instance.GetNaIDeviceData(time);
                if (!string.IsNullOrEmpty(content))
                {
                    List<DataPacket> pks = builder.GetDataPackets(deviceKey, time, content);
                    foreach (var p in pks)
                    {
                        agent.SendDataPacket(p);                        
                    }

                    if (pks.Count > 0)
                    {
                        Logger logger = Log.GetLogFile(deviceKey);
                        logger.Log("---- BEGIN ----");
                        foreach (var p in pks)
                        {
                            string msg = p.ToString();
                            logger.Log(msg);
                        }
                        logger.Log("---- END ---- ");

                        this.UpdateSendDataRecord(deviceKey, false);
                    }
                }
                else
                {
                    Log.GetLogFile(deviceKey).Log("RD Error: Empty NaI-file");
                }
            }
            else
            {
                this.data.Clear();
                var r = DBDataSource.Instance.GetData(deviceKey, time, null, this.data);
                if (r == ReadResult.ReadOK)
                {
                    DataPacket p = null;
                    // By different device.

                    if (deviceKey.Equals("Scada.HVSampler", StringComparison.OrdinalIgnoreCase) ||
                        deviceKey.Equals("Scada.ISampler", StringComparison.OrdinalIgnoreCase))
                    {
                        p = builder.GetFlowDataPacket(deviceKey, this.data, true);
                    }
                    else
                    {
                        p = builder.GetDataPacket(deviceKey, this.data, true);
                    }

                    if (agent.SendDataPacket(p))
                    {
                        string msg = string.Format("RD: {0}", p.ToString());
                        Log.GetLogFile(deviceKey).Log(msg);
                        this.UpdateSendDataRecord(deviceKey, false);
                    }
                }
                else
                {
                    string line = string.Format("RD Error: {0} [{1} <{2}>]", r.ToString(), deviceKey, time);
                    Log.GetLogFile(deviceKey).Log(line);

                }
            }
        }

        private static DateTime GetDeviceSendTime(DateTime dt, string deviceKey)
        {
            if (deviceKey.Equals("Scada.NaIDevice", StringComparison.OrdinalIgnoreCase))
            {
                int min = dt.Minute - 1;
                DateTime ret = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 0);
                return ret;
            }
            else
            {
                int second = dt.Second / 30 * 30;
                DateTime ret = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second);
                return ret;
            }
        }

        private static bool IsDeviceSendTimeOK(DateTime dt, string deviceKey)
        {
            if (deviceKey.Equals("Scada.NaIDevice", StringComparison.OrdinalIgnoreCase))
            {
                // 00, 05, 10, ...55,
                // Send data after 1 min.
                if ((dt.Minute - 1) % 5 == 0)
                {
                    return true;
                }
            }
            else
            {
                // 5 < current.second < 15 OR
                // 35 < current.second < 45
                int sec = dt.Second - 5;
                if ((sec >= 0 && sec <= 10) || ((sec >= 30) && sec <= 40))
                {
                    return true;
                }
            }
            return false;
        }

        // Main Thread Yet
        /*
        private void ShowAgentMessage(Agent agent, string msg)
        {
            string line = string.Format("{0}: {1}", agent.ToString(), msg);
            this.mainListBox.Items.Add(line);
        }
        */

        private void OnNotifyEvent(Agent agent, NotifyEvents notify, string msg1, string msg2)
        {
            if (this.quitPressed)
                return;
            this.SafeInvoke(() =>
            {
                this.statusStrip.Items[0].Text = this.GetConnetionString();

                if (NotifyEvents.Connecting == notify)
                {
                    ConnetionRecord cr = new ConnetionRecord();
                    this.connectionHistory.Add(cr);
                }
                else if (NotifyEvents.Connected == notify)
                {
                    int count = this.connectionHistory.Count;
                    ConnetionRecord cr = this.connectionHistory[count - 1];
                    cr.ConnectedTime = DateTime.Now;
                }
                else if (NotifyEvents.Disconnect == notify)
                {
                    int count = this.connectionHistory.Count;
                    ConnetionRecord cr = this.connectionHistory[count - 1];
                    cr.DisconnectedTime = DateTime.Now;
                }
                else if (NotifyEvents.HandleEvent == notify)
                {
                    string line = string.Format("{0}: {1} {2}", DateTime.Now, msg1, msg2);
                    this.mainListBox.Items.Add(line);
                }
                else if (NotifyEvents.HistoryDataSent == notify)
                {
                    string deviceKey = msg1.ToLower();
                    string line = string.Format("HD: {0}", msg2);
                    Log.GetLogFile(deviceKey).Log(line);
                    this.UpdateSendDataRecord(deviceKey, true);
                }
                /// 国家数据中心相关
                else if (NotifyEvents.ConnectToCountryCenter == notify)
                {
                    this.StartConnectCountryCenter();
                    this.mainListBox.Items.Add(msg1);
                }
                else if (NotifyEvents.DisconnectToCountryCenter == notify)
                {
                    this.StopConnectCountryCenter();
                    this.mainListBox.Items.Add(msg1);
                }
            });
        }


        private void StartConnectCountryCenter()
        {
            if (this.countryCenterAgent != null)
            {
                this.countryCenterAgent.Connect();
                //this.agents.Add(this.countryCenterAgent);
            }
            else
            {
                this.SafeInvoke(() =>
                {
                    string line = string.Format("请检查国家数据中心的配置");
                    this.mainListBox.Items.Add(line);

                    Log.GetLogFile(Program.DataClient).Log("Error: StartConnectCountryCenter(); Check the config.");
                });
            }
        }

        private void StopConnectCountryCenter()
        {
            if (this.countryCenterAgent != null)
            {
                this.countryCenterAgent.Disconnect();
                // this.agents.Remove(this.countryCenterAgent);
            }
        }

        private void ShowAtTaskBar(bool shown)
        {
            this.ShowInTaskbar = shown;
        }

        private void AgentWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.ShowAtTaskBar(false);
        }

        private int updateCounter = 0;

        // Realtime data sending...details
        private void UpdateSendDataRecord(string deviceKey, bool history)
        {
            var details = this.detailsDict[deviceKey];
            if (history)
            {
                details.LatestSendHistoryDataTime = DateTime.Now;
            }
            else
            {
                details.LatestSendDataTime = DateTime.Now;
            }
            details.SendDataCount += 1;

            // 主动更新数据表
            this.updateCounter++;
            if (this.mainTabCtrl.SelectedIndex == 2 && this.updateCounter > 7)
            {
                this.updateCounter = 0;
                this.UpdateDetailsListView();    
            }
        }

        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void startStripButton_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PerformQuitByUser();
        }

        private void PerformQuitByUser()
        {
            if (this.agent != null)
            {
                this.agent.Quit();
            }
            this.quitPressed = true;
            Application.Exit();
        }

        private void LoggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenProcessByName("Scada.Logger.Server.exe", true);
        }

        private void loggerStripButton1_Click(object sender, EventArgs e)
        {
            this.OpenProcessByName("Scada.Logger.Server.exe", true);
        }

        private void OpenProcessByName(string name, bool uac = false)
        {
            string fileName = LogPath.GetExeFilePath(name);
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                if (uac && Environment.OSVersion.Version.Major >= 6)
                {
                    processInfo.Verb = "runas";
                }
                processInfo.FileName = fileName;
                Process.Start(processInfo);
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("文件'{0}'不存在，或者需要管理员权限才能运行。", name));
            }
        }

        private void mainTabCtrl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.connHistoryList.Items.Clear();

            if (this.mainTabCtrl.SelectedIndex == 1)
            {
                foreach (var cr in this.connectionHistory)
                {
                    this.connHistoryList.Items.Insert(0, cr.ToString());
                }
            }
            else if (this.mainTabCtrl.SelectedIndex == 2)
            {
                this.UpdateDetailsListView();
            }
        }

        private Dictionary<string, DeviceDataDetails> detailsDict = new Dictionary<string, DeviceDataDetails>();

        private void InitDetailsListView()
        {
            this.InitDeviceColumn("scada.hpic", "高压电离室");
            this.InitDeviceColumn("scada.naidevice", "NaI谱仪");
            this.InitDeviceColumn("scada.weather", "气象站");
            this.InitDeviceColumn("scada.hvsampler", "超大流量气溶胶采样器");
            this.InitDeviceColumn("scada.isampler", "碘采样器");
            this.InitDeviceColumn("scada.shelter", "环境与安防监控");
            this.InitDeviceColumn("scada.dwd", "干湿沉降采样器");
        }

        private static string FormatTime(DateTime time)
        {
            if (time != default(DateTime))
                return time.ToString("yyyy-MM-dd HH:mm");
            else
                return "-";
        }

        private void InitDeviceColumn(string deviceKey, string deviceName)
        {
            ListViewItem lvi = new ListViewItem(deviceName);
            lvi.Tag = deviceKey;

            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "0"));
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "-"));
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "-"));

            this.detailsListView.Items.Add(lvi);
            this.detailsDict.Add(deviceKey, new DeviceDataDetails());
        }

        private void UpdateDetailsListView()
        {
            foreach (ListViewItem item in this.detailsListView.Items)
            {
                string deviceKey = (string)item.Tag;

                DeviceDataDetails details = this.detailsDict[deviceKey];

                item.SubItems[1].Text = details.SendDataCount.ToString();
                item.SubItems[2].Text = FormatTime(details.LatestSendDataTime);
                item.SubItems[3].Text = FormatTime(details.LatestSendHistoryDataTime);
            }
        }

        private void SetTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.agent.CanHandleSetTime = this.SetTimeToolStripMenuItem.Checked;
        }

    }
}

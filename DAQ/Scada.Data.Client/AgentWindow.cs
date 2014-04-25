using MySql.Data.MySqlClient;
using Scada.Data.Client;
using Scada.Data.Client.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Scada.Data.Client
{
    public partial class MainDataAgentWindow : Form
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


        private Timer sendDataTimer;

        private Timer recvDataTimer;

        private DataAgent agent;

        private PacketBuilder builder = new PacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private RealTimeForm detailForm;

        private const int TimerInterval = 4000;

        private ToolStripLabel statusLabel = new ToolStripLabel();

        private ToolStripLabel uploadLabel = new ToolStripLabel();

        private ToolStripLabel addressLabel = new ToolStripLabel();

        private ToolStripLabel counterLabel = new ToolStripLabel();

        private ToolStripLabel pingLabel = new ToolStripLabel();

        public bool CancelQuit { get; set; }

        public MySqlConnection MySqlConnection { get; set; }

        public MySqlCommand MySqlCmd { get; set; }

        /// <summary>
        /// Test code goes here
        /// </summary>
#if DEBUG
        private void TestSendPacket()
        {
            string deviceKey = "scada.shelter";
            DateTime sendTime = DateTime.Parse("2013-11-30 20:33:00");
            
            Packet packet = this.GetPacket(sendTime, deviceKey);
            if (packet != null)
            {
                string msg = packet.ToString();
                this.agent.SendPacket(packet);
            }
        }
        private void TestSendFilePacket()
        {
            // TODO: FIND A n42 file
            string deviceKey = "scada.naidevice";
            DateTime sendTime = DateTime.Parse("2012-09-01 11:50:00");
            Packet packet = this.GetPacket(sendTime, deviceKey);
            if (packet != null)
            {
                string msg = packet.ToString();
                this.agent.SendFilePacket(packet);
            }
        }
#endif

        public MainDataAgentWindow()
        {
            InitializeComponent();
        }

        private void AgentWindow_Load(object sender, EventArgs e)
        {
            this.CancelQuit = true;
            InitSysNotifyIcon();
            // StatusBar labels
            this.statusStrip.Items.Add(this.statusLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.addressLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.counterLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.uploadLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.pingLabel);

            this.InitDetailsListView();
            this.Start();
        }

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            sysNotifyIcon.Text = "数据上传 v2.0";
            sysNotifyIcon.Icon = new Icon(Resources.AppIcon, new Size(16, 16));
            sysNotifyIcon.Visible = true;
            sysNotifyIcon.DoubleClick += new EventHandler(OnSysNotifyIcon);

            ContextMenu notifyContextMenu = new ContextMenu();
            MenuItem exitMenuItem = new MenuItem("退出");
            exitMenuItem.Click += (s, e) =>
            {
                this.PerformQuitByUser();
            };

            MenuItem detailMenuItem = new MenuItem("详情");
            detailMenuItem.Click += (s, e) =>
            {
                this.ShowDetailsForm();
            };

            notifyContextMenu.MenuItems.Add(detailMenuItem);
            notifyContextMenu.MenuItems.Add(new MenuItem("-"));
            notifyContextMenu.MenuItems.Add(exitMenuItem);

            sysNotifyIcon.ContextMenu = notifyContextMenu;
        }

        private void PerformQuitByUser()
        {
            this.CancelQuit = false;
            // Close DB connection
            if (this.MySqlConnection != null)
            {
                try
                {
                    this.MySqlConnection.Close();
                }
                catch (Exception) { }
            }
            Application.Exit();
        }

        private void OnSysNotifyIcon(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void Start()
        {
            if (this.InitializeAgent())
            {
                this.InitializeTimer();

                string line = string.Format("{0} starts at {1}.", Program.DataClient, DateTime.Now);
                Log.GetLogFile(Program.DataClient).Log(line);
            }
            else
            {
                this.CancelQuit = false;
                Application.Exit();
            }
        }

        private bool InitializeAgent()
        {
            Settings.Instance.LoadSettings();
            Settings s = Settings.Instance;

            if (s.DataCenters.Count() == 0)
            {
                this.pingLabel.Text = "配置错误";
                return false;
            }

            Settings.DataCenter2 dc = s.DataCenters[0];
            this.agent = new DataAgent(dc);
            this.agent.DoAuth();

            this.statusLabel.Text = string.Format("开始:[{0}]", DateTime.Now);
            this.addressLabel.Text = string.Format("<{0}>", dc.BaseUrl);
            this.counterLabel.Text = string.Format("已发送: 0");
            this.uploadLabel.Text = string.Format("实时数据最后上传时间: {0}", FormatTime(DateTime.Now));
            return true;
        }

        private void ConnectToMySQL()
        {
            this.MySqlConnection = DataSource.Instance.GetDBConnection();
            this.MySqlCmd = this.MySqlConnection.CreateCommand();
        }

        private void InitializeTimer()
        {
            this.sendDataTimer = new Timer();
            this.sendDataTimer.Interval = TimerInterval;
            this.sendDataTimer.Tick += this.HttpSendDataTick;
            this.sendDataTimer.Start();

            this.recvDataTimer = new Timer();
            this.recvDataTimer.Interval = 20 * 1000;
            this.recvDataTimer.Tick += this.HttpRecvDataTick;
            this.recvDataTimer.Start();
        }

        private void HttpRecvDataTick(object sender, EventArgs e)
        {
            this.agent.FetchCommands();
        }

        // 当归一化时间到来时上传数据
        private void HttpSendDataTick(object sender, EventArgs e)
        {
            if (!this.canUploadData)
                return;
            DateTime now = DateTime.Now;
            // For Upload File Devices.
            if (!IsSendFileTimeOK(now))
            {
                SendDevicePackets(Settings.Instance.FileDeviceKeys, now);
            }

            // For Upload Data Devices.
            if (IsSendDataTimeOK(now))
            {
                SendDevicePackets(Settings.Instance.DataDeviceKeys, now);
            }
        }

        // 上传所有设备的(实时)数据
        private void SendDevicePackets(string[] deviceKeys, DateTime now, bool useDebugDataTime = false)
        {
            List<Packet> packets = new List<Packet>();
            foreach (var deviceKey in deviceKeys)
            {
                DateTime sendTime = GetDeviceSendTime(now, deviceKey);
#if DEBUG
                if (useDebugDataTime)
                {
                    if (Settings.Instance.DebugDataTime != default(DateTime))
                    {
                        sendTime = Settings.Instance.DebugDataTime;
                    }
                }
#endif

                if (!this.lastDeviceSendData.ContainsKey(deviceKey))
                {
                    this.lastDeviceSendData[deviceKey] = default(DateTime);
                }

                if (sendTime == this.lastDeviceSendData[deviceKey])
                {
                    continue;
                }

                this.lastDeviceSendData[deviceKey] = sendTime;

                Packet packet = this.GetPacket(sendTime, deviceKey);
                if (packet != null)
                {
                    packets.Add(packet);
                }
            }

            // Send ...
            packets = this.builder.CombinePackets(packets);
            this.SendPackets(packets);
        }

        private void SendPackets(List<Packet> packets)
        {
            foreach (var packet in packets)
            {
                this.agent.SendPacket(packet);
            }
        }

        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        private Packet GetPacket(DateTime time, string deviceKey)
        {
            if (this.MySqlCmd == null)
            {
                this.ConnectToMySQL();
            }

            string errorMsg;
            ReadResult d = DataSource.GetData(this.MySqlCmd, deviceKey, time, default(DateTime), this.data, out errorMsg);
            if (d == ReadResult.ReadDataOK)
            {
                if (this.data.Count > 0)
                {
                    Packet p = builder.GetPacket(deviceKey, this.data[0], true);
                    return p;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // TODO: errorMsg
                return null;
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

        private bool IsSendFileTimeOK(DateTime dt)
        {
            return (dt.Minute - 1) % 5 == 0;
        }

        private static bool IsSendDataTimeOK(DateTime dt)
        {
            // 5 < current.second < 15 OR
            // 35 < current.second < 45
            int sec = dt.Second - 5;
            if ((sec >= 0 && sec <= 10) || ((sec >= 30) && sec <= 40))
            {
                return true;
            }
            return false;
        }

        private void OnNotifyEvent(DataAgent agent, NotifyEvents ne, string msg)
        {
            this.SafeInvoke(() =>
            {
                /*
                if (NotifyEvents.Connected == ne)
                {
                    string logger = agent.ToString() + " 已连接";
                    this.statusStrip1.Items[1].Text = logger;
                    Log.GetLogFile(Program.DataClient).Log(logger);
                }
                else if (NotifyEvents.ConnectError == ne)
                {
                    this.statusStrip1.Items[1].Text = msg;
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                else if (NotifyEvents.ConnectToCountryCenter == ne)
                {
                    // this.StartConnectCountryCenter();
                    this.listBox1.Items.Add(msg);
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                else if (NotifyEvents.DisconnectToCountryCenter == ne)
                {
                    // this.StopConnectCountryCenter();
                    this.listBox1.Items.Add(msg);
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                 * */
            });
        }

        private void OnDetailsButtonClick(object sender, EventArgs e)
        {
            this.ShowDetailsForm();
        }

        private void ShowDetailsForm()
        {
            if (this.detailForm == null)
            {
                this.detailForm = new RealTimeForm(this.BeforeCloseDetailForm);
            }

            this.detailForm.Show();
        }

        private void BeforeCloseDetailForm()
        {
            this.detailForm = null;
        }

        private void AgentWindowClosingForm(object sender, FormClosingEventArgs e)
        {
            e.Cancel = this.CancelQuit;
            this.WindowState = FormWindowState.Minimized;
        }

        private void SendDetails(string deviceKey, string msg)
        {
            if (this.detailForm != null)
            {
                this.detailForm.OnSendDetails(deviceKey, msg);
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
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "0.0%"));
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "0"));
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

        private void testToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress localIp = IPAddress.Parse("127.0.0.1");
                IPEndPoint localIpEndPoint = new IPEndPoint(localIp, 3000);
                var receiveUpdClient = new UdpClient();
                receiveUpdClient.Connect(localIpEndPoint);
                int a = receiveUpdClient.Send(Encoding.ASCII.GetBytes("Hello"), 5);
            }
            catch (Exception ex)
            {

            }
        }

        private void SendDevicePacket(string deviceKey, DateTime now = default(DateTime))
        {
            foreach (var eachDeviceKey in Settings.Instance.DeviceKeys)
            {
                if (eachDeviceKey == deviceKey)
                {
                    bool useDebugDataTime = Settings.Instance.UseDebugDataTime;
                    this.SendDevicePackets(new string[] { deviceKey }, now, useDebugDataTime);
                }
            }
        }

        private void SendDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                string tag = (string)mi.Tag;
                SendDevicePacket(tag, DateTime.Now);
            }
        }

        private bool canUploadData = true;

        private void dataUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.canUploadData = !this.canUploadData;
        }

    }
}

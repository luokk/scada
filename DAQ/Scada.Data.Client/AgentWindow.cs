using MySql.Data.MySqlClient;
using Scada.Common;
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
using System.Threading;
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


        private System.Windows.Forms.Timer sendDataTimer;

        private System.Windows.Forms.Timer recvDataTimer;

        private DataAgent agent;

        private PacketBuilder builder = new PacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private RealTimeForm detailForm;

        private const int TimerInterval = 3500;

        private ToolStripLabel statusLabel = new ToolStripLabel();

        private ToolStripLabel uploadLabel = new ToolStripLabel();

        private ToolStripLabel addressLabel = new ToolStripLabel();

        private ToolStripLabel counterLabel = new ToolStripLabel();

        private ToolStripLabel pingLabel = new ToolStripLabel();

        private bool IsDBEnable = false;

        public bool CancelQuit { get; set; }

        public MySqlConnection MySqlConnection { get; set; }

        public MySqlCommand MySqlCmd { get; set; }

        private CommandReceiver cmdReceiver;

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

            this.CheckLastSendTime = true;

            if (!this.IsSecond)
            {
                this.cmdReceiver = new CommandReceiver(Ports.DataClient);
                cmdReceiver.Start(this.OnLocalCommand);
            }
            this.InitDetailsListView();
            this.Start();
        }

        private void OnLocalCommand(string msg)
        {
            this.SafeInvoke(() =>
            {
                if (msg.IndexOf("DOOR=") == 0)
                {
                    string state = msg.Substring(5);
                    // var packet = this.builder.GetDoorStatePacket(state);
                    // this.agent.SendPacket(packet);
                }
                else if (msg.IndexOf("ACTIVE=") == 0)
                {
                    string state = msg.Substring(6);
                    if (state == "1")
                    {
                        this.WindowState = FormWindowState.Normal;

                    }
                    else
                    {
                        this.WindowState = FormWindowState.Minimized;
                        // this.ShowAtTaskBar(false);
                    }
                }
            });
        }

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            sysNotifyIcon.Text = "数据上传（HTTP）";
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
            this.cmdReceiver.Close();
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
            if (!this.InitializeAgent())
            {
                this.CancelQuit = false;
                Application.Exit();
            }

            // 初始化时检查数据库是否启动
            if (this.ConnectToMySQL() != true)
            {
                this.IsDBEnable = false;
                this.statusLabel.Text = "数据库未启动";
                this.statusLabel.ForeColor = Color.Red;
                this.DBTestStripButton.Enabled = true;
                this.StartUpdateStripButton.Enabled = false;
                return;
            }
            else
            {
                this.statusLabel.Text = "正常";
                this.statusLabel.ForeColor = Color.Black;
                this.DBTestStripButton.Enabled = false;
                this.StartUpdateStripButton.Enabled = false;
            }

            // 开始执行
            this.InitializeTimer();

            string line = string.Format("{0} starts at {1}.", Program.DataClient, DateTime.Now);
            if (!this.IsSecond)
            {
                Log.GetLogFile(Program.DataClient).Log(line);
            }
        }

        private bool InitializeAgent()
        {
            Settings.AgentXml = this.IsSecond ? Settings.AgentXml + ".2" : Settings.AgentXml;
            Settings.Instance.LoadSettings();
            Settings s = Settings.Instance;

            if (s.DataCenters.Count() == 0)
            {
                MessageBox.Show("配置错误");
                this.pingLabel.Text = "配置错误";
                return false;
            }

            Settings.DataCenter2 dc = s.DataCenters[0];
            this.agent = new DataAgent(dc);
            this.agent.NotifyEvent += this.OnNotify;
            this.agent.DoAuth();

            this.IsDBEnable = true;
            this.statusLabel.Text = string.Format("开始:[{0}]", DateTime.Now);
            this.DBTestStripButton.Enabled = false;
            
            this.addressLabel.Text = string.Format("已连接{0}", dc.BaseUrl);
            this.counterLabel.Text = string.Format("已发送: 0");
            this.uploadLabel.Text = string.Format("实时数据最后上传时间: {0}", FormatTime(DateTime.Now));

            return true;
        }

        private bool ConnectToMySQL()
        {
            this.MySqlConnection = DataSource.Instance.GetDBConnection();

            if (this.MySqlConnection != null)
            {
                this.MySqlCmd = this.MySqlConnection.CreateCommand();
                if (this.MySqlCmd != null)
                {
                    return true;
                }
                else
                { return false; }
            }
            else
            { return false; }
        }

        private void InitializeTimer()
        {
            // 定期往数据中心发数据
            this.sendDataTimer = new System.Windows.Forms.Timer();
            this.sendDataTimer.Interval = TimerInterval;
            this.sendDataTimer.Tick += this.HttpSendDataTick;
            this.sendDataTimer.Start();

            // 每20s从数据中心取一次数据
            this.recvDataTimer = new System.Windows.Forms.Timer();
            this.recvDataTimer.Interval = 20 * 1000;
            this.recvDataTimer.Tick += this.HttpRecvDataTick;
            this.recvDataTimer.Start();
        }

        private void HttpRecvDataTick(object sender, EventArgs e)
        {
            if (this.isAutoData)
            {
                this.agent.FetchCommands();
            }
        }

        // 当归一化时间到来时上传数据
        private void HttpSendDataTick(object sender, EventArgs e)
        {
            if (!this.isAutoData)
                return;
            DateTime now = DateTime.Now;
            // For Upload File Devices.
            if (IsSendFileTimeOK(now))
            {
                Guid guid = Guid.NewGuid();
                SendDevicePackets(Settings.Instance.FileDeviceKeys, now, guid.ToString());
            }

            // For Upload Data Devices.
            if (IsSendDataTimeOK(now))
            {
                Guid guid = Guid.NewGuid();
                SendDevicePackets(Settings.Instance.DataDeviceKeys, now, guid.ToString());
            }
        }

        // 上传所有设备的(实时)数据
        private void SendDevicePackets(string[] deviceKeys, DateTime now, string packetId)
        {
            List<Packet> packets = new List<Packet>();
            foreach (var deviceKey in deviceKeys)
            {
                // 归一化时间
                DateTime sendTime = GetDeviceSendTime(now, deviceKey);

                if (this.CheckLastSendTime)
                {
                    if (!this.lastDeviceSendData.ContainsKey(deviceKey))
                    {
                        this.lastDeviceSendData[deviceKey] = default(DateTime);
                    }

                    if (sendTime == this.lastDeviceSendData[deviceKey])
                    {
                        continue;
                    }
                }

                Packet packet = this.GetPacket(sendTime, deviceKey, packetId);
                if (packet != null)
                {
                    if (this.agent.SendPacket(packet))
                    {
                        // 当发送成功后，才记录已发送的时间
                        this.lastDeviceSendData[deviceKey] = sendTime;
                    }
                }
            }
        }

        /* 未使用
        private void SendPackets(List<Packet> packets)
        {
            foreach (var packet in packets)
            {
                this.agent.SendPacket(packet);
            }
        }
         * */

        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        private Packet GetPacket(DateTime time, string deviceKey, string packetId)
        {
            if (Settings.Instance.FileDeviceKeys.Contains(deviceKey))
            {
                if (deviceKey.IndexOf("labr") >= 0)
                {
                    Packet p = builder.GetFilePacket(DataSource.Instance.GetLabrDeviceFile(time), "labr");
                    if (p != null)
                    {
                        p.DeviceKey = deviceKey;
                        p.Id = packetId;
                        return p;
                    }
                }
                else if (deviceKey.IndexOf("hpge") >= 0)
                {
                    string filePath = DataSource.Instance.GetNewHpGeFile();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        Packet p = builder.GetFilePacket(filePath, "hpge");
                        if (p != null)
                        {
                            p.DeviceKey = deviceKey;
                            p.Id = packetId;
                            return p;
                        }
                    }
                }
                return null;
            }
            else
            {
                if (this.MySqlCmd == null)
                {
                    this.ConnectToMySQL();
                }

                string errorMsg;

                DateTime from = time.AddSeconds(-30);
                ReadResult d = DataSource.GetData(this.MySqlCmd, deviceKey, from, time, RangeType.CloseOpen, this.data, out errorMsg);
                if (d == ReadResult.ReadDataOK)
                {
                    if (this.data.Count > 0)
                    {
                        Packet p = builder.GetPacket(deviceKey, this.data, true);
                        p.DeviceKey = deviceKey;
                        p.Id = packetId;
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
        }

        private static DateTime GetDeviceSendTime(DateTime dt, string deviceKey)
        {
            // Labr设备，每5分钟发送一次
            if (deviceKey.Equals(Devices.Labr, StringComparison.OrdinalIgnoreCase))
            {
                int min = dt.Minute / 5 * 5;
                DateTime ret = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 0);
                return ret;
            }

            /*
            // Cinderella.Status设备无固定发送频率，只要有数据立即发送
            else if (deviceKey.Equals("Scada.Cinderella.Status", StringComparison.OrdinalIgnoreCase))
            {
                return dt;
            }
             * */

            // 其余设备每30s发送一次
            else
            {
                int second = dt.Second / 30 * 30;
                DateTime ret = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second);
                return ret;
            }
        }

        private bool IsSendFileTimeOK(DateTime dt)
        {
            return true;
            //return (dt.Minute - 1) % 5 == 0;
        }

        private static bool IsSendDataTimeOK(DateTime dt)
        {
            // 5 < current.second < 30 OR
            // 35 < current.second < 60
            int sec = dt.Second - 5;
            if ((sec >= 0 && sec < 25) || ((sec >= 30) && sec < 55))
            {
                return true;
            }
            return false;
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

        public bool CheckLastSendTime
        {
            get;
            set;
        }

        private Dictionary<string, DeviceDataDetails> detailsDict = new Dictionary<string, DeviceDataDetails>();

        private void InitDetailsListView()
        {
            this.InitDeviceColumn("scada.hpic", "高压电离室");
            this.InitDeviceColumn("scada.cinderella.data", "Cinderella数据");
            this.InitDeviceColumn("scada.cinderella.status", "Cinderella状态");
            this.InitDeviceColumn("scada.weather", "气象站");
            this.InitDeviceColumn("scada.shelter", "环境与安防监控");
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

        private int updateCounter = 0;
        private long packetCount = 0;
        private void UpdateSendDataRecord(string deviceKey, bool history)
        {
            if (!this.detailsDict.ContainsKey(deviceKey))
                return;
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

            this.packetCount++;
            this.counterLabel.Text = string.Format("已发送: {0}", this.packetCount);

            // 主动更新数据表
            this.updateCounter++;
            if (this.updateCounter > 5)
            {
                this.updateCounter = 0;
                this.UpdateDetailsListView();
            }
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

        private void SendDevicePacket(string deviceKey, DateTime now, string packetId)
        {
            foreach (var eachDeviceKey in Settings.Instance.DeviceKeys)
            {
                if (eachDeviceKey == deviceKey)
                {
                    this.SendDevicePackets(new string[] { deviceKey }, now, packetId);
                }
            }
        }

        private void SendDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                // Starts DEBUG
                this.debugConsole.Text = "";
                string debugGuid = Guid.NewGuid().ToString();
                string deviceKey = (string)mi.Tag;
                DateTime sendTime = DateTime.Now;
                if (Settings.Instance.UseDebugDataTime)
                {
                    sendTime = Settings.Instance.GetDebugDataTime(deviceKey);
                }
                this.CheckLastSendTime = false;
                SendDevicePacket(deviceKey, sendTime, debugGuid);
                this.CheckLastSendTime = true;
            }
        }

        private bool isAutoData = true;

        private void AutoDataToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.isAutoData = !this.isAutoData;
            this.autoDataToolStripMenuItem.Checked = this.isAutoData;
        }

        private void FetchCmdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.debugConsole.Text = "";

            this.agent.FetchCommands();
        }

        private void OnNotify(DataAgent agent, NotifyEvents notifyEvent, Notify p)
        {
            this.SafeInvoke(() =>
            {
                this.OnNotifyAtUIThread(agent, notifyEvent, p);
            });
        }

        private void OnNotify2(DataAgent agent, NotifyEvents notifyEvent, Notify p)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="notifyEvent"></param>
        /// <param name="msg"></param>
        private void OnNotifyAtUIThread(DataAgent agent, NotifyEvents ne, Notify p)
        {
            string msg = string.Format("{0}: {1}", DateTime.Now, p.Message);
            if (ne == NotifyEvents.UploadFileOK)
            {
                fileUploadInfoListBox.Items.Add(msg);
            }
            else if (ne == NotifyEvents.UploadFileFailed)
            {
                fileUploadInfoListBox.Items.Add(msg);
            }
            else if (ne == NotifyEvents.DebugMessage)
            {
                this.debugConsole.Text += string.Format("{0}\n", p.Message);
            }
            else if (ne == NotifyEvents.EventMessage)
            {
                mainListBox.Items.Add(msg);
            }
            else if (ne == NotifyEvents.SendDataOK)
            {
                this.UpdateSendDataRecord(p.DeviceKey, false);
            }
            else if (ne == NotifyEvents.SendDataFailed)
            {
                //this.UpdateSendDataRecord(p.DeviceKey, false);
            }
            else if (ne == NotifyEvents.HistoryData)
            {
                this.HandleHistoryData(p.Payload);
            }
        }

        private void HandleHistoryData(Dictionary<string, string> payload)
        {
            string device = GetValue(payload, "device");
            string start = GetValue(payload, "start");
            string end = GetValue(payload, "end");

            if (device == "hpge")
            {
                string sid = GetValue(payload, "sid");
                Thread thread = new Thread(new ParameterizedThreadStart((o) => 
                {
                    while (true)
                    {
                        string filePath = DataSource.Instance.GetNewHpGeFile(sid);
                        if (string.IsNullOrEmpty(filePath))
                            break;

                        Packet p = builder.GetFilePacket(filePath, "hpge");
                        if (p != null)
                        {
                            p.DeviceKey = Devices.HPGe;
                            p.Id = "";
                            p.setHistory();

                            this.agent.SendPacket(p);
                        }
                    }
                }));
                thread.Start();

            }
            else
            {
                string timesStr = GetValue(payload, "times");
                string[] timesArray = timesStr.Split(',');
                Dictionary<long, bool> dict = new Dictionary<long, bool>();
                foreach (var time in timesArray)
                {
                    dict.Add(long.Parse(time), true);
                }

                if (timesArray != null && timesArray.Length > 0)
                {
                    string deviceKey = this.GetDeviceKey(device);
                    DateTime from = DateTime.Parse(start);
                    DateTime time = DateTime.Parse(end);
                    var data = new List<Dictionary<string, object>>();
                    string errorMsg;

                    if (this.MySqlCmd == null)
                    {
                        this.ConnectToMySQL();
                    }

                    ReadResult d = DataSource.GetData(this.MySqlCmd, deviceKey, from, time, RangeType.CloseOpen, data, out errorMsg);
                    if (d == ReadResult.ReadDataOK)
                    {
                        // If have data!
                        int len = data.Count;
                        if (timesArray.Length > 0)
                        {
                            // 有请求的时间集合
                            List<Dictionary<string, object>> group = new List<Dictionary<string, object>>();
                            for (var i = 0; i < len; i++)
                            {
                                // !
                                var item = data[i];
                                long unixtime = Packet.GetUnixTime2((string)item["time"]);
                                if (dict.ContainsKey(unixtime))
                                {
                                    group.Add(item);
                                }

                                if (group.Count >= 20 || i + 1 == len)
                                {
                                    Packet p = builder.GetPacket(deviceKey, group, true);
                                    p.DeviceKey = deviceKey;
                                    p.Id = "";
                                    p.setHistory();

                                    this.agent.SendPacket(p);

                                    group.Clear();
                                }
                            } // End for
                        }
                        else
                        {
                            // 没有明确的时间集合
                            for (var i = 0; i < len; i += 20)
                            {
                                var part = data.GetRange(i, Math.Min(20, len - i));
                                Packet p = builder.GetPacket(deviceKey, part, true);
                                p.DeviceKey = deviceKey;
                                p.Id = "";
                                p.setHistory();

                                this.agent.SendPacket(p);
                            }
                        }
                    }
                }
            }
            
        }

        private string GetDeviceKey(string device)
        {
            device = device.ToLower();
            if (device == "hpic")
            {
                return Devices.Hpic;
            }
            else if (device == "weather")
            {
                return Devices.Weather;
            }
            else if (device == "cinderella")
            {
                return Devices.CinderellaData;
            }
            else if (device == "labr")
            {
                return Devices.Labr;
            }
            else if (device == "hpge")
            {
                return Devices.HPGe;
            }
            else if (device == "environment")
            {
                return Devices.Shelter;
            }
            return string.Empty;
        }

        private static string GetValue(Dictionary<string, string> payload, string key, string value = null)
        {
            if (payload.ContainsKey(key))
            {
                return payload[key];
            }
            return value;
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PerformQuitByUser();
        }

        private void DBTestStripButton_Click(object sender, EventArgs e)
        {
            if (this.ConnectToMySQL() != true)
            {
                this.IsDBEnable = false;
                this.statusLabel.Text = "数据库未启动";
                this.statusLabel.ForeColor = Color.Red;
                this.DBTestStripButton.Enabled = true;
                this.StartUpdateStripButton.Enabled = false;
            }
            else
            {
                this.IsDBEnable = true;
                this.statusLabel.Text = "正常";
                this.statusLabel.ForeColor = Color.Black;
                this.DBTestStripButton.Enabled = false;
                this.StartUpdateStripButton.Enabled = true;
            }
        }

        private void StartUpdateStripButton_Click(object sender, EventArgs e)
        {
            this.InitializeTimer();
            this.StartUpdateStripButton.Enabled = false;
        }

        private void SendSycnToolStripMenuItem_Click(object sender, EventArgs e)
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

        public bool IsSecond { get; set; }
    }
}

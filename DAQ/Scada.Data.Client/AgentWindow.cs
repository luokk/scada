using Scada.Data.Client;
using Scada.Data.Client.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scada.Data.Client
{
    public partial class MainDataAgentWindow : Form
    {
        private Timer sendDataTimer;

        private Timer recvDataTimer;

        private DataAgent agent;

        private PacketBuilder builder = new PacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private RealTimeForm detailForm;

        private const int TimerInterval = 4000;

        private ToolStripLabel statusLabel = new ToolStripLabel();

        private ToolStripLabel addressLabel = new ToolStripLabel();

        private ToolStripLabel counterLabel = new ToolStripLabel();

        private ToolStripLabel pingLabel = new ToolStripLabel();

        private bool started = false;

        public MainDataAgentWindow()
        {
            InitializeComponent();
        }

        private void AgentWindow_Load(object sender, EventArgs e)
        {
            this.CancelQuit = true;
            InitSysNotifyIcon();
            this.statusStrip.Items.Add(this.statusLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.addressLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.counterLabel);
            this.statusStrip.Items.Add(new ToolStripSeparator());
            this.statusStrip.Items.Add(this.pingLabel);

            this.Start();            
        }
        #if DEBUG
        private void TestSendPacket()
        {
            string deviceKey = "scada.shelter";
            DateTime sendTime = DateTime.Parse("2013-11-30 20:33:00");
            int errorCode = 0;
            Packet packet = this.GetPacket(sendTime, deviceKey, out errorCode);
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
            int errorCode = 0;
            Packet packet = this.GetPacket(sendTime, deviceKey, out errorCode);
            if (packet != null)
            {
                string msg = packet.ToString();
                this.agent.SendFilePacket(packet);
            }
        }
        #endif

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            sysNotifyIcon.Text = "数据上传程序v2.0";
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
                // this.started = true;

                string line = string.Format("{0} starts at {1}.", Program.DataClient, DateTime.Now);
                Log.GetLogFile(Program.DataClient).Log(line);


                this.TestSendPacket();
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

            this.agent = new DataAgent(s.DataCenters[0]);
            this.agent.DoAuth();

            this.statusLabel.Text = string.Format("开始:[{0}]", DateTime.Now);
            return true;
        }

        private DataAgent CreateAgent(Settings.DataCenter2 dataCenter)
        {
            DataAgent agent = new DataAgent(dataCenter);
            return agent;
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

        private void HttpSendDataTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            // For NaI Device.
            if (!IsNaISendFileTimeOK(now))
            {
                SendDevicePackets(Settings.Instance.FileDeviceKeys, now);
            }

            // For other Devices.
            if (IsSendTimeOK(now))
            {
                SendDevicePackets(Settings.Instance.DataDeviceKeys, now);
            }
        }

        private void SendDevicePackets(string[] deviceKeys, DateTime now)
        {
            List<Packet> packets = new List<Packet>();
            foreach (var deviceKey in deviceKeys)
            {
                DateTime sendTime = GetDeviceSendTime(now, deviceKey);

                if (!this.lastDeviceSendData.ContainsKey(deviceKey))
                {
                    this.lastDeviceSendData[deviceKey] = default(DateTime);
                }

                if (sendTime == this.lastDeviceSendData[deviceKey])
                {
                    continue;
                }

                this.lastDeviceSendData[deviceKey] = sendTime;

                int errorCode = 0;
                Packet packet = GetPacket(sendTime, deviceKey, out errorCode);
                if (packet != null)
                {
                    packets.Add(packet);
                }
            }

            // Send ...
            packets = this.CombinePackets(packets);
            this.SendPackets(packets);
        }

        private void SendPackets(List<Packet> packets)
        {
            foreach (var packet in packets)
            {
                this.agent.SendPacket(packet);
            }
        }

        private List<Packet> CombinePackets(List<Packet> packets)
        {
            if (packets.Count > 1)
            {
                List<Packet> ret = new List<Packet>();
                Packet pn = null;
                foreach (var p in packets)
                {
                    if (string.IsNullOrEmpty(p.Path))
                    {
                        // 目前认为Path为空的Packet是Data Packet
                        pn = this.builder.CombinePacket(pn, p);
                    }
                    else
                    {
                        ret.Add(p); // p is file packet
                    }
                }
                if (pn != null)
                {
                    ret.Add(pn);
                }
                return ret;
            }
            return packets;
        }

        private Packet GetPacket(DateTime time, string deviceKey, out int errorCode)
        {
            errorCode = 0;
            if (!this.agent.SendDataStarted)
            {
                // return null;
            }

            var d = DBDataSource.Instance.GetData(deviceKey, time);
            if (d != null && d.Count > 0)
            {
                Packet p = builder.GetPacket(deviceKey, d, true);
                return p;
            }
            errorCode = 100;
            return null;

            /*
             *  if (!this.checkBoxUpdateNaI.Checked)
                {
                    return null;
                }

                string fileName = DBDataSource.Instance.GetNaIDeviceFile(time);
                if (!string.IsNullOrEmpty(fileName))
                {
                    return builder.GetFilePacket(fileName);
                }
             */
        }
  


        private DateTime lastSendTime;

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

        private bool IsNaISendFileTimeOK(DateTime dt)
        {
            return (dt.Minute - 1) % 5 == 0;
        }

        private static bool IsSendTimeOK(DateTime dt)
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


        public bool CancelQuit { get; set; }
    }
}

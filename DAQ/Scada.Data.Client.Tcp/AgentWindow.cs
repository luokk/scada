using Microsoft.Win32;
using Scada.DataCenterAgent.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scada.Data.Client.Tcp
{
    public partial class AgentWindow : Form
    {
        private Timer timer;

        private Timer keepAliveTimer;
        //private DataHandler handler;

        private List<Agent> agents = new List<Agent>();

        private Agent countryCenterAgent;

        private DataPacketBuilder builder = new DataPacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private RealTimeForm detailForm;

        public bool StartState
        {
            get;
            set;
        }

        private bool started = false;

        public AgentWindow()
        {
            this.StartState = false;
            InitializeComponent();
        }

        private void AgentWindow_Load(object sender, EventArgs e)
        {
            this.Restart = false;
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEventsSessionEnding);

            InitSysNotifyIcon();
            this.ShowInTaskbar = false;
            this.statusStrip1.Items.Add("状态: 等待");
            this.statusStrip1.Items.Add(new ToolStripSeparator());
            this.statusStrip1.Items.Add("IP ADDR:PORT");
            ToolStripLabel label = new ToolStripLabel();
            label.Alignment = ToolStripItemAlignment.Right;
            label.Text = "";
            this.statusStrip1.Items.Add(label);

            // Start if have the --start args.
            if (this.StartState)
            {
                Start();
            }
        }

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            sysNotifyIcon.Text = "系统设备管理器";
            sysNotifyIcon.Icon = new Icon(Resources.AppIcon, new Size(16, 16));
            sysNotifyIcon.Visible = true;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            sysNotifyIcon.Click += new EventHandler(OnSysNotifyIconContextMenu);
        }

        private void OnSysNotifyIconContextMenu(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void Start()
        {
            if (this.started)
            {
                return;
            }
            this.InitializeAgents();
            this.InitializeTimer();
            this.started = true;
            Log.GetLogFile(Program.DataClient).Log("Data (upload) Agent starts at " + DateTime.Now);
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
                    this.agents.Add(agent);
                }
            }

            this.statusStrip1.Items[0].Text = string.Format("状态: 开始 ({0})", DateTime.Now);
        }

        // 先连接有线的线路
        private Agent CreateAgent(string serverAddress, int serverPort, bool wireless)
        {
            Agent agent = new Agent(serverAddress, serverPort);
            agent.Type = Type.Province;
            agent.Wireless = wireless;
            agent.Connect();
            agent.OnReceiveMessage += this.OnReceiveMessage;
            agent.OnNotifyEvent += this.OnNotifyEvent;
            return agent;
        }

        // 国家中心
        private Agent CreateCountryCenterAgent(string serverAddress, int serverPort)
        {
            Agent agent = new Agent(serverAddress, serverPort);
            agent.Type = Type.Country;
            agent.Wireless = false;
            agent.OnReceiveMessage += this.OnReceiveMessage;
            agent.OnNotifyEvent += this.OnNotifyEvent;
            return agent;
        }

        private void InitializeTimer()
        {
            this.timer = new Timer();
            this.timer.Interval = 4000;
            this.timer.Tick += this.SendDataTick;
            this.timer.Start();

            // KeepAlive timer per 30 sec
            this.keepAliveTimer = new Timer();
            this.keepAliveTimer.Interval = 1000 * 30;
            this.keepAliveTimer.Tick += this.KeepAliveTick;
            this.keepAliveTimer.Start();
        }

        private void KeepAliveTick(object sender, EventArgs e)
        {
            DataPacket p = builder.GetKeepAlivePacket();
            foreach (var agent in this.agents)
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
            bool willSend = false;
            foreach (var agent in this.agents)
            {
                willSend |= agent.SendDataStarted;
            }

            if (!willSend) //// TODO: !
            {
                return;
            }

            if (deviceKey.Equals("Scada.NaIDevice", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.checkBoxUpdateNaI.Checked)
                {
                    return;
                }
                // 分包
                string content = DBDataSource.Instance.GetNaIDeviceData(time);
                if (!string.IsNullOrEmpty(content))
                {
                    List<DataPacket> pks = builder.GetDataPackets(deviceKey, time, content);
                    foreach (var p in pks)
                    {
                        // Sent by each agent.s
                        foreach (var agent in this.agents)
                        {
                            agent.SendDataPacket(p, time);
                        }
                    }

                    if (pks.Count > 0)
                    {
                        Logger logger = Log.GetLogFile(deviceKey);
                        logger.Log("---- A Group of NaI file-content ----");
                        foreach (var p in pks)
                        {
                            string msg = p.ToString();
                            logger.Log(msg);
                        }
                        logger.Log("---- ---- ---- ---- ---- ---- ---- ----");

                        this.SendDetails(deviceKey, pks[0].ToString());
                    }
                }
                else
                {
                    Log.GetLogFile(deviceKey).Log("<Real-Time> NaI file Content is empty!");
                }
            }
            else
            {
                var d = DBDataSource.Instance.GetData(deviceKey, time);
                if (d != null && d.Count > 0)
                {
                    DataPacket p = null;
                    // By different device.

                    if (deviceKey.Equals("Scada.HVSampler", StringComparison.OrdinalIgnoreCase) ||
                        deviceKey.Equals("Scada.ISampler", StringComparison.OrdinalIgnoreCase))
                    {
                        p = builder.GetFlowDataPacket(deviceKey, d, true);
                    }
                    else
                    {
                        p = builder.GetDataPacket(deviceKey, d, true);
                    }

                    // Sent by each agent.s
                    foreach (var agent in this.agents)
                    {
                        agent.SendDataPacket(p, time);
                    }

                    string msg = string.Format("{0}: {1}", DateTime.Now, p.ToString());
                    Log.GetLogFile(deviceKey).Log(msg);
                    this.SendDetails(deviceKey, msg);
                }
                else
                {
                    string logger = string.Format("{0}: No data found from the table", DateTime.Now);
                    Log.GetLogFile(deviceKey).Log(logger);

                }
            }
            
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


        private void OnReceiveMessage(Agent agent, string msg)
        {
            if ("6031" == Value.Parse(msg, "CN"))
            {
                // No KeepAlive message from now on;
                return;
            }

            this.SafeInvoke(() => 
            {
                string line = string.Format("{0}: {1}", agent.ToString(false), msg);
                this.listBox1.Items.Add(line);
            });
        }

        private void OnNotifyEvent(Agent agent, NotifyEvent ne, string msg)
        {
            this.SafeInvoke(() =>
            {
                if (NotifyEvent.Connected == ne)
                {
                    string logger = agent.ToString() + " 已连接";
                    this.statusStrip1.Items[1].Text = logger;
                    Log.GetLogFile(Program.DataClient).Log(logger);
                }
                else if (NotifyEvent.ConnectError == ne)
                {
                    this.statusStrip1.Items[1].Text = msg;
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                else if (NotifyEvent.ConnectToCountryCenter == ne)
                {
                    this.StartConnectCountryCenter();
                    this.listBox1.Items.Add(msg);
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                else if (NotifyEvent.DisconnectToCountryCenter == ne)
                {
                    this.StopConnectCountryCenter();
                    this.listBox1.Items.Add(msg);
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
            });
        }


        // 开始
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        // 暂停
        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void StartConnectCountryCenter()
        {
            if (this.countryCenterAgent != null)
            {
                this.countryCenterAgent.Connect();
                this.agents.Add(this.countryCenterAgent);
            }
            else
            {
                this.SafeInvoke(() =>
                {
                    string line = string.Format("请检查国家数据中心的配置");
                    this.listBox1.Items.Add(line);

                    Log.GetLogFile(Program.DataClient).Log("Error: StartConnectCountryCenter(); Check the config.");
                });
            }
        }

        private void StopConnectCountryCenter()
        {
            if (this.countryCenterAgent != null)
            {
                this.countryCenterAgent.Disconnect();
                this.agents.Remove(this.countryCenterAgent);
            }
        }

        private void detailsButton_Click(object sender, EventArgs e)
        {
            this.detailForm = new RealTimeForm(() => 
            {
                this.detailForm = null;
            });
            this.detailForm.Show();
        }

        private void SystemEventsSessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                case SessionEndReasons.SystemShutdown:
                    this.Restart = true;
                    break;
                default:
                    break;
            }
        }

        private void AgentWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Restart)
            {
                return;
            }

            DialogResult dr = MessageBox.Show("退出数据上传程序?", "数据上传", MessageBoxButtons.YesNo);
            if (DialogResult.No == dr)
            {
                e.Cancel = true;
            }
        }

        private void SendDetails(string deviceKey, string msg)
        {
            if (this.detailForm != null)
            {
                this.detailForm.OnSendDetails(deviceKey, msg);
            }
        }

        private void AgentWindow_MinimumSizeChanged(object sender, EventArgs e)
        {
            // Not invoked.
        }

        private void AgentWindow_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        public bool Restart { get; set; }
    }
}

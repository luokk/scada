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
    public partial class AgentWindow : Form
    {
        private Timer sendDataTimer;

        private Timer recvDataTimer;

        private List<Agent> agents = new List<Agent>();

        private PacketBuilder builder = new PacketBuilder();

        private Dictionary<string, DateTime> lastDeviceSendData = new Dictionary<string, DateTime>();

        private RealTimeForm detailForm;

        private const int TimerInterval = 4000; 

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
                this.agents[0].SendPacket(packet);
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
                this.agents[0].SendFilePacket(packet);
            }
        }
        #endif

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
            // TODO: Create Agent for DataCenter2
            if (s.DataCenters.Count() > 0)
            {
                foreach (var dc in s.DataCenters)
                {
                    Agent agent = this.CreateAgent(dc);
                    agent.Connect();
                    this.agents.Add(agent);
                }
                this.statusStrip1.Items[0].Text = string.Format("状态: 开始 ({0})", DateTime.Now);
            }
        }

        private Agent CreateAgent(Settings.DataCenter2 dataCenter)
        {
            Agent agent = new Agent(dataCenter.Ip, dataCenter.Port);
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
            foreach (var agent in this.agents)
            {
                agent.FetchCommands();
            }
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
            foreach (var agent in this.agents)
            {
                foreach (var packet in packets)
                {
                    agent.SendPacket(packet);
                }
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
            bool couldSend = false;
            foreach (var agent in this.agents)
            {
                couldSend |= agent.SendDataStarted;
            }

            if (!couldSend) //// TODO: !
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

        /*
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
        }*/


        private void OnReceiveMessage(Agent agent, string msg)
        {
            this.SafeInvoke(() => 
            {
                string line = string.Format("{0}: {1}", "Agent.ToString()", msg);
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
                    // this.StartConnectCountryCenter();
                    this.listBox1.Items.Add(msg);
                    Log.GetLogFile(Program.DataClient).Log(msg);
                }
                else if (NotifyEvent.DisconnectToCountryCenter == ne)
                {
                    // this.StopConnectCountryCenter();
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

        private void detailsButton_Click(object sender, EventArgs e)
        {
            this.detailForm = new RealTimeForm(() => 
            {
                this.detailForm = null;
            });
            this.detailForm.Show();
        }


        private void AgentWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("退出数据上传程序?", "数据上传", MessageBoxButtons.YesNo);
            if (DialogResult.No == dr)
            {
                e.Cancel = true;
            }
            else
            {

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
    }
}

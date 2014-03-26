using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Scada.Main.Properties;
using System.Threading;
using System.Diagnostics;
using Scada.Common;
using Scada.Declare;
using Scada.Config;
using Microsoft.Win32;
using System.Security.Principal;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Scada.Main
{
    public partial class MainForm : Form
    {
		private System.Windows.Forms.Timer timer = null;

        private bool deviceRunning = false;

        private bool quitByUser = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.InitSysNotifyIcon();
            this.SetStatusText("系统就绪");

            bool recover = false;
            bool runAll = false;

            string[] args = Program.DeviceManager.Args;
            if (args != null && args.Length > 0)
            {
                string options = args[0];
                options = options.ToUpper();
                if (options == "/R")
                {
                    recover = true;
                }
                else if (options == "/ALL")
                {
                    runAll = true;
                }
            }

			////////////////////////////////////////////////////////////////
			// Device List in Group.
            deviceListView.Columns.Add("设备", 280);
			deviceListView.Columns.Add("版本", 80);
			deviceListView.Columns.Add("状态", 100);

            deviceListView.ShowGroups = true;

            Dictionary<string, ListViewGroup> groups = new Dictionary<string, ListViewGroup>();

            foreach (string deviceName in Program.DeviceManager.DeviceNames)
            {
                string deviceKey = deviceName.ToLower();
                string displayName = Program.DeviceManager.GetDeviceDisplayName(deviceKey);
                if (displayName != null)
                {
                    ListViewGroup g = null;
                    if (groups.ContainsKey(displayName))
                    {
                        g = groups[displayName];
                    }
                    else
                    {
                        g = deviceListView.Groups.Add("", displayName);
                        groups.Add(displayName, g);
                    }

                    List<string> versions = Program.DeviceManager.GetVersions(deviceKey);
                    ListViewItem lvi = this.AddDeviceToList(deviceName, versions[0], "就绪");

                    g.Items.Add(lvi);
                }
            }

            // Auto start
            // runAll = true;
            if (runAll)
            {
                this.StartDevices(true);
            }
            else if (recover)
            {
                if (this.RecoverCheck())
                {
                    this.StartDevices(false);
                }
                else
                {
                    this.StartDevices(true);
                }
            }

            this.OpenLocalCommandReceiver();
        }

        private void OpenLocalCommandReceiver()
        {
            try
            {
                CommandReceiver cr = new CommandReceiver(3000);
                cr.Start(OnReceivedCommandLine);
            }
            catch(Exception e)
            {
            }
        }

        private void OnReceivedCommandLine(string line)
        {
            SynchronizationContext.Current.Post(new SendOrPostCallback((cmd) => 
            {

            }), line);
        }

        private bool RecoverCheck()
        {
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (!Directory.Exists(statusPath))
            {
                return false;
            }

            string runningDevicesFile = Path.Combine(statusPath, "running.devices");
            if (!File.Exists(runningDevicesFile))
            {
                return false;
            }

            try
            {
                using (FileStream fs = File.Open(runningDevicesFile, FileMode.Open))
                {
                    // Recover
                    this.CheckAllDevices(false);

                    long len = fs.Length;
                    byte[] bytes = new byte[len];
                    fs.Read(bytes, 0, (int)len);

                    StringReader sr = new StringReader(Encoding.ASCII.GetString(bytes));

                    string deviceKeyLine = sr.ReadLine();
                    while (deviceKeyLine != null)
                    {
                        string deviceKey = deviceKeyLine.ToLower().Trim();

                        if (!string.IsNullOrEmpty(deviceKey))
                        {
                            foreach (ListViewItem item in this.deviceListView.Items)
                            {
                                string itemTextLower = item.Text.ToLower();
                                if (itemTextLower == deviceKey)
                                {
                                    item.Checked = true;
                                }
                            }

                        }

                        deviceKeyLine = sr.ReadLine();
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                
            }
            return false;
        }

        private void SetStatusText(string status)
        {
            this.statusLabel.Text = status;
        }

        private void PressVBFormConnectToCPUButtons()
        {
            const string Version = "0.9";
            string path;
            path = DeviceManager.GetDeviceConfigPath("Scada.HVSampler", Version);
            FormProxyDevice.PressConnectToCPU("MDS.exe", path);

            path = DeviceManager.GetDeviceConfigPath("Scada.ISampler", Version);
            FormProxyDevice.PressConnectToCPU("AIS.exe", path);
        }

        private void RunDevices()
        {
            // Update 
            var hasSelectedDevices = this.UpdateDevicesRunningStatus();
            if (!hasSelectedDevices)
            {
                MessageBox.Show("请选择要启动的设备！");
                return;
            }
            this.deviceRunning = true;
            this.startToolBarButton.Enabled = false;
            
            RecordManager.Initialize();
            Program.DeviceManager.DataReceived = this.OnDataReceived;
            Program.DeviceManager.Run(SynchronizationContext.Current, this.OnDataReceived);

            this.WindowState = FormWindowState.Minimized;
            this.ShowAtTaskBar(false);

            this.SetStatusText("系统运行中...");

            // Keep-Alive timer
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = Defines.RescueCheckTimer;
            this.timer.Tick += timerKeepAliveTick;
            this.timer.Start();
        }

        private ListViewItem AddDeviceToList(string deviceName, string version, string status)
        {
            ListViewItem lvi = deviceListView.Items.Add(new ListViewItem(deviceName));
            // Subitems;
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, version));
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, status));

            string deviceKey = deviceName.ToLower();
            if (deviceKey != "scada.hvsampler" && deviceKey != "scada.isampler")
                lvi.Checked = true;
            return lvi;
        }

        private bool BeforeShowAtTaskBar
        {
            get;
            set;
        }

        private void ShowAtTaskBar(bool shown)
        {
            this.BeforeShowAtTaskBar = true;
            this.ShowInTaskbar = shown;
            this.BeforeShowAtTaskBar = false;
        }

		void timerKeepAliveTick(object sender, EventArgs e)
		{
            // Not use this method to send keep alive message.
			// Program.SendKeepAlive();

            // Check the Last Modify time of each device.
            Program.DeviceManager.CheckLastModifyTime();
		}

		private void InitSysNotifyIcon()
		{
			// Notify Icon
            sysNotifyIcon.Text = "系统设备管理器";
			sysNotifyIcon.Icon = new Icon(Resources.AppIcon, new Size(16, 16));
			sysNotifyIcon.Visible = true;
            ContextMenu notifyContextMenu = new ContextMenu();

            MenuItem exitMenuItem = new MenuItem("退出");
            exitMenuItem.Click += (s, e) => 
            {
                this.PerformQuitByUser();
            };
            MenuItem dataUploadMenuItem = new MenuItem("数据上传程序");
            dataUploadMenuItem.Click += (s, e) => 
            {
                this.BringDataUploadUI();
            };
            MenuItem mainProcMenuItem = new MenuItem("主程序");
            mainProcMenuItem.Click += (s, e) =>
            {
                this.ShowMainUI();
            };
            notifyContextMenu.MenuItems.Add(dataUploadMenuItem);
            notifyContextMenu.MenuItems.Add(mainProcMenuItem);
            notifyContextMenu.MenuItems.Add(exitMenuItem);

            sysNotifyIcon.ContextMenu = notifyContextMenu;
			sysNotifyIcon.DoubleClick += new EventHandler(OnSysNotifyIconContextMenu);
		}

        private void BringDataUploadUI()
        {
            MessageBox.Show("等待功能实现");
        }

        private void ShowMainUI()
        {
            this.ShowAtTaskBar(true);
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

		private void OnSysNotifyIconContextMenu(object sender, EventArgs e)
		{
            this.ShowMainUI();
		}

        private void MakeWindowShownFront()
        {
            this.TopMost = true;
            this.Activate();
            this.TopMost = false;
        }
		// Menu Entries
		private void fileMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void exitMenuItem_Click(object sender, EventArgs e)
		{
            this.PerformQuitByUser();
		}

        private void PerformQuitByUser()
        {
            this.quitByUser = true;

            this.UpdateDevicesWaitStatus();
            Program.DeviceManager.CloseAllDevices();

            RecordManager.DoSystemEventRecord(Device.Main, "Scada.Main Closed by Admin!");
            Program.Exit();
            Application.Exit();
        }

		private void aboutMenuItem_Click(object sender, EventArgs e)
		{
            this.OpenProcessByName("Scada.About.exe");
		}

		private void docMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void stopMenuItem_Click(object sender, EventArgs e)
		{
            this.deviceRunning = false;
            this.startToolBarButton.Enabled = true;
            // deviceListView.Enabled = true;
            this.UpdateDevicesWaitStatus();
            Program.DeviceManager.CloseAllDevices();
            RecordManager.DoSystemEventRecord(Device.Main, "User Stopped the devices");
            Program.Exit(); // For quit Mutex;
            Program.DeviceManager.OpenMainProgram();
            Application.Exit();
		}

		private void startMainVisionMenuItem_Click(object sender, EventArgs e)
		{
            // Show MainVision
            this.OpenProcessByName("Scada.MainVision.exe");
		}

		private void logToolMenuItem_Click(object sender, EventArgs e)
		{
            this.OpenProcessByName("Scada.Logger.Server.exe", true);
		}

		private void logBankMenuItem_Click(object sender, EventArgs e)
		{
            // TODO: Bank Log files;
		}

		private void logDelMenuItem_Click(object sender, EventArgs e)
		{
            // TODO: Delete Log files;
		}

        private void MainForm_Resize(object sender, EventArgs e)
        {

        }

        private void deviceListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void SelectDevices()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                if (item.Checked)
                {
                    string deviceName = item.SubItems[0].Text;
                    string version = item.SubItems[1].Text;
                    Program.DeviceManager.SelectDevice(deviceName, version, true);

                    sb.AppendLine(deviceName);
                }
            }

            // Dump the running devices.
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (!Directory.Exists(statusPath))
            {
                Directory.CreateDirectory(statusPath);
            }

            string runningDevicesFile = Path.Combine(statusPath, "running.devices");
            if (File.Exists(runningDevicesFile))
            {
                File.Delete(runningDevicesFile);
            }

            using (FileStream fs = File.Open(runningDevicesFile, FileMode.CreateNew))
            {
                byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private void CheckAllDevices(bool check = true)
        {
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                item.Checked = check;
            }
        }

        private bool UpdateDevicesRunningStatus()
        {
            bool hasSelectedDevices = false;
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                if (item.Checked)
                {
                    hasSelectedDevices = true;
                    item.SubItems[2].Text = "运行中...";
                }
            }
            return hasSelectedDevices;
        }

        private void UpdateDevicesWaitStatus()
        {
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                item.SubItems[2].Text = "就绪";
            }
        }

        private void deviceListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (this.deviceRunning)
            {
                if (!this.BeforeShowAtTaskBar)
                {
                    e.NewValue = e.CurrentValue;
                }
            }
        }

        private void deviceListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!this.deviceRunning)
            {
                bool itemChecked = e.Item.Checked;
                if (itemChecked)
                {
                    // TODO: If there are multi-items, only one can be selected.    
                }
            }

        }

        private void settingClick(object sender, EventArgs e)
        {
            this.OpenProcessByName("Scada.MainSettings.exe");
        }

        private void startToolBarButton_Click(object sender, EventArgs e)
        {
            this.StartDevices(false);
        }

        private void startMenuItem_Click(object sender, EventArgs e)
        {
            this.StartDevices(false);
        }

        // Toolbar Menu
        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.StartDevices(true);
        }

        // Menu
        private void StartAllToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.StartDevices(true);
        }

        private void StartDevices(bool all)
        {
            if (all)
            {
                this.CheckAllDevices();
            }
            this.SelectDevices();
            this.RunDevices();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenProcessByName("Scada.MainSettings.exe");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.quitByUser)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowAtTaskBar(false);
                e.Cancel = true;
            }
        }

        private void loggerServer_Click(object sender, EventArgs e)
        {
            this.OpenProcessByName("Scada.Logger.Server.exe", true);
            RecordManager.ResetDetectTime();
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
            catch (Exception e)
            {
                MessageBox.Show(string.Format("文件'{0}'不存在，或者需要管理员权限才能运行。", name));
            }
        }

        private void dataUploadMenuItem_Click(object sender, EventArgs e)
        {
            this.BringDataUploadUI();
        }

        private void dataUploadItem_Click(object sender, EventArgs e)
        {
            this.BringDataUploadUI();
        }

        private void dataUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.BringDataUploadUI();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!selectAllToolStripMenuItem.Checked)
            {
                this.CheckAllDevices(true);
                selectAllToolStripMenuItem.Checked = true;
            }
            else
            {
                this.CheckAllDevices(false);
                selectAllToolStripMenuItem.Checked = false;
            }
        }
    }
}

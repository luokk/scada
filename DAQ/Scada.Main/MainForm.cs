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

namespace Scada.Main
{
    public partial class MainForm : Form
    {
		private System.Windows.Forms.Timer timer = null;

        private bool started = false;

        //private Process mainVisionProcess;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitSysNotifyIcon();
            this.SetStatusText("系统就绪");
            new LoggerClient().Send("Scada.Main started");

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

            foreach (string deviceName in Program.DeviceManager.DeviceNames)
            {
                string deviceKey = deviceName.ToLower();
                string displayName = Program.DeviceManager.GetDeviceDisplayName(deviceKey);
                if (displayName != null)
                {
                    ListViewGroup g = deviceListView.Groups.Add(deviceKey, displayName);

                    List<string> versions = Program.DeviceManager.GetVersions(deviceKey);
                    ListViewItem lvi = this.AddDeviceToList(deviceName, versions[0], "就绪");

                    g.Items.Add(lvi);
                }
            }

            // runAll = true;
            if (runAll)
            {
                this.CheckAllDevices();
                Thread.Sleep(1000);
                this.PressVBFormConnectToCPUButtons();
                
                this.SelectDevices();
                this.RunDevices();
            }
            else if (recover)
            {
                // TODO: Load devices selected from a file, insert them into ListView.
                for (; ; )
                {
                    // Select the devices.
                    Program.DeviceManager.SelectDevice("", "", true);

                    // TODO: List Them into ListView
                }
                //
                this.RunDevices();
            }
            
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
            this.started = true;
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

			sysNotifyIcon.Click += new EventHandler(OnSysNotifyIconContextMenu);
		}

		private void OnSysNotifyIconContextMenu(object sender, EventArgs e)
		{
            this.ShowAtTaskBar(true);
            this.WindowState = FormWindowState.Normal;
		}


		// Menu Entries
		private void fileMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void addDeviceFileMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void exitMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void aboutMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void docMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void stopMenuItem_Click(object sender, EventArgs e)
		{
            this.started = false;
            this.startToolBarButton.Enabled = true;
            // deviceListView.Enabled = true;
            this.UpdateDevicesWaitStatus();
			Program.DeviceManager.CloseAllDevices();
            Application.Exit();
		}

		private void startMainVisionMenuItem_Click(object sender, EventArgs e)
		{
            // Show MainVision
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = false;    //设定不显示窗口
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "Scada.MainVision.exe"; //设定程序名  
                process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
                process.StartInfo.RedirectStandardError = true;//重定向错误输出
                process.Start();

            }
		}

		private void logToolMenuItem_Click(object sender, EventArgs e)
		{
			RecordManager.OpenRecordAnalysis();
		}

		private void logBankMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void logDelMenuItem_Click(object sender, EventArgs e)
		{

		}


		//////////////////////////////////////////////////////////////////////////
		private void func()
		{
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {

        }

        private void deviceListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void SelectDevices()
        {
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                if (item.Checked)
                {
                    string deviceName = item.SubItems[0].Text;
                    string version = item.SubItems[1].Text;
                    Program.DeviceManager.SelectDevice(deviceName, version, true);
                }
            }
        }

        private void CheckAllDevices()
        {
            foreach (ListViewItem item in this.deviceListView.Items)
            {
                item.Checked = true;
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
            if (this.started)
            {
                if (!this.BeforeShowAtTaskBar)
                {
                    e.NewValue = e.CurrentValue;
                }
            }
        }

        private void deviceListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!this.started)
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

        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Select the device in the list;
            this.SelectDevices();
            this.RunDevices();
        }

        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Select the device in the list;
            this.CheckAllDevices();
            this.SelectDevices();
            this.RunDevices();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show MainVision
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = false;    //设定不显示窗口
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "Scada.MainSettings.exe"; //设定程序名  
                process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
                process.StartInfo.RedirectStandardError = true;//重定向错误输出
                process.Start();
            }
        }





    }
}

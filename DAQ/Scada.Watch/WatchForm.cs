using Scada.Common;
using Scada.Watch.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Scada.Watch
{
	public partial class WatchForm : Form
	{
        private Timer checkTimer = null;

        private int mainTimeCounter = 0;

        private int dataClientTimeCounter = 0;

        private FileSystemWatcher fsw = null;

		public WatchForm()
		{
			InitializeComponent();
        }

        private void InitSysNotifyIcon()
        {
            // Notify Icon
            watchNotifyIcon.Text = "系统设备管理器";
            watchNotifyIcon.Icon = new Icon(Resources.Monitor, new Size(16, 16));
            watchNotifyIcon.Visible = true;

            watchNotifyIcon.Click += new EventHandler(OnSysNotifyIconContextMenu);
        }

        private void OnSysNotifyIconContextMenu(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
        }


		private void WatchForm_Load(object sender, EventArgs e)
		{
            this.InitSysNotifyIcon();

            this.ShowInTaskbar = false;
            this.Visible = false;
            // Watch Main 
            this.checkTimer = new Timer();
            this.checkTimer.Interval = 30 * 1000;    // Defines.KeepAliveInterval;
            this.checkTimer.Tick += Per30secTimerTick;
            this.checkTimer.Start();

            // Initial Path
            this.textPath.Text = @"C:\Install";
		}

        private void SetAutoUpdateSearchDir(string dir)
        {
            if (this.fsw != null)
            {
                this.fsw.Dispose();
            }

            if (Directory.Exists(dir))
            {
                this.fsw = new FileSystemWatcher(dir, "*.zip");
                this.fsw.Created += (object sender, FileSystemEventArgs e) =>
                {
                    this.UpdateBin(e.FullPath);
                };
                this.fsw.EnableRaisingEvents = true;
            }
        }

        private void UpdateBin(string filePath)
        {
            this.OpenProcessByName("Scada.Update", string.Format("\"{0}\"", filePath));
        }


        private void Per30secTimerTick(object sender, EventArgs e)
		{
            if (mainTimeCounter % 3 == 0)
            {
                this.WatchMainExe();
            }

            if (dataClientTimeCounter % 3 == 0)
            {
                // this.WatchDataClientExe();
            }


            this.mainTimeCounter++;
		}

        private void WatchMainExe()
        {
            const string ScadaMain = @"Scada.Main";
            Process[] ps = Process.GetProcessesByName(ScadaMain);
            if (ps == null || ps.Length == 0)
            {
                // this.OpenProcessByName(ScadaMain, "/R");
            }
        }

        private void WatchDataClientExe()
        {
            // TODO:
            const string ScadaDataClient = @"??";
            Process[] ps = Process.GetProcessesByName(ScadaDataClient);
            if (ps == null || ps.Length == 0)
            {
                this.OpenProcessByName(ScadaDataClient, "/R");
            }

        }


        private void OpenProcessByName(string name, string arg)
        {
            string fileName = name + ".exe";
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = fileName;
                processInfo.Arguments = arg;
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
            }
        }

        private void buttonWatch_Click(object sender, EventArgs e)
        {
            string dir = this.textPath.Text;
            this.textPath.Enabled = false;
            this.SetAutoUpdateSearchDir(dir);
        }

        private void buttonPathClick(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = @"C:\";
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                this.textPath.Enabled = true;
                this.textPath.Text = fbd.SelectedPath;
            }
        }

        private void watchNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void WatchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("您确定要退出[进程监控程序]吗？", "Scada.Watch", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                // TODO:
            }
            else
            {
                e.Cancel = true;
            }


        }

        private void WatchForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }


	}
}

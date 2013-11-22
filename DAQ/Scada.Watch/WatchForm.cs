using Scada.Common;
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
        private DateTime theDate = DateTime.Now;
        // Timer for check KeepAlive
		private Timer timer = null;

        private Dictionary<string, LogWatcher> watchers = new Dictionary<string, LogWatcher>();

		public WatchForm()
		{
			InitializeComponent();
            
        }

        private string GetTodayLogFile(string deviceName, string version)
        {
            DateTime now = DateTime.Now;
            string fileName = string.Format("{0}-{1}-{2}.log", now.Year, now.Month, now.Day);
            string path = string.Format("{0}\\devices\\{1}\\{2}\\log\\{3}", GetInstallPath(), deviceName, version, fileName);

            return path;
        }

        private void AddWatch()
        {
            /*
            string stateFile = string.Format("{0}\\devices.stt", GetInstallPath());
            if (File.Exists(stateFile))
            {
                using (StreamReader sr = new StreamReader(stateFile))
                {
                    string line = string.Empty;
                    do
                    {
                        line = sr.ReadLine();
                        if (line == null)
                            break;
                        line = line.Trim();

                        string[] kv = line.Split('=');
                        if (kv.Length == 2)
                        {
                            string deviceKey = kv[0].Trim();
                            string version = kv[1].Trim();

                            if (deviceKey.Length > 0)
                            {
                                string path = this.GetTodayLogFile(deviceKey, version);
                                if (File.Exists(path))
                                {
                                    watchers.Add(deviceKey, new LogWatcher(path));
                                }
                            }
                        }
                    }
                    while (line != null);
                }
            }
           */
        }


		private void WatchForm_Load(object sender, EventArgs e)
		{
            AddWatch();
            this.AddBinZipWatcher();

			this.timer = new Timer();
            this.timer.Interval = 60 * 1000;    // Defines.KeepAliveInterval;
			this.timer.Tick += timerTick;
			this.timer.Start();

		}

		void timerTick(object sender, EventArgs e)
		{
            DateTime now = DateTime.Now;
            if (now.Day != this.theDate.Day)
            {
                // A new day comes!
                this.theDate = now;
                this.watchers.Clear();
                this.AddWatch();
                return;
            }

            // TODO: foreach then check.

		}

        private string GetInstallPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }



        private void AddBinZipWatcher()
        {
            string updatePath = @"C:\Users\HealerKx\Projects\DAQ-Proj\DAQ\Bin\Debug\update";
            FileSystemWatcher fsw = new FileSystemWatcher(updatePath);
            fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            fsw.Changed += new FileSystemEventHandler(this.OnBinZipChanged);
            fsw.EnableRaisingEvents = true;
        }

        private void OnBinZipChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // TODO: Invoke Installer to update bin.zip
            }
        }

	}
}

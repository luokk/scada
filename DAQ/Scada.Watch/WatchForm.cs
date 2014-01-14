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
        private Timer checkMainTimer = null;

		public WatchForm()
		{
			InitializeComponent();
            
        }

		private void WatchForm_Load(object sender, EventArgs e)
		{
            this.checkMainTimer = new Timer();
            this.checkMainTimer.Interval = 60 * 1000;    // Defines.KeepAliveInterval;
            this.checkMainTimer.Tick += timerTick;
            this.checkMainTimer.Start();
		}

		void timerTick(object sender, EventArgs e)
		{
            const string ScadaMain = @"Scada.Main.exe";
            Process[] ps = Process.GetProcessesByName(ScadaMain);
            if (ps == null || ps.Length == 0)
            {
                this.OpenProcessByName(ScadaMain, false);
            }
		}

        private void OpenProcessByName(string name, bool uac = false)
        {
            string fileName = name;
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                if (uac)
                {
                    processInfo.Verb = "runas";
                }
                processInfo.FileName = fileName;
                processInfo.Arguments = "/R";
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("文件'{0}'不存在，或者需要管理员权限才能运行。", name));
            }
        }


	}
}

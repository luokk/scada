using Scada.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Logger.Server
{
    public partial class LoggerForm : Form
    {
        List<ListBox> listBoxes = new List<ListBox>();

        public LoggerForm()
        {
            InitializeComponent();
        }

        private ListBox CreateListBox(string deviceKey)
        {
            deviceKey = deviceKey.ToLower();
            ListBox listBox = new ListBox();
            listBox.Tag = deviceKey;
            listBox.Dock = DockStyle.Fill;

            listBoxes.Add(listBox);
            return listBox;
        }

        private ListBox GetListBox(string deviceKey)
        {
            foreach (ListBox lb in listBoxes)
            {
                if (string.Equals((string)lb.Tag, deviceKey, StringComparison.OrdinalIgnoreCase))
                {
                    return lb;
                }
            }
            return null;
        }

        private void InitMenuStatus()
        {
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (Directory.Exists(statusPath))
            {
                string[] filePaths = Directory.GetFiles(statusPath, @"@*");
                var deviceKeys = filePaths.Select((string path) =>
                {
                    string fileName = Path.GetFileName(path);
                    return fileName.ToLower().Substring(1);
                });

                var menus = SettingsToolStripMenuItem.DropDownItems;
                foreach (ToolStripMenuItem m in menus)
                {
                    string tag = m.Tag as string;
                    if (string.IsNullOrEmpty(tag))
                        continue;
                    m.Checked = false;
                    tag = tag.ToLower();
                    foreach (var deviceKey in deviceKeys)
                    {
                        if (deviceKey == tag)
                        {
                            m.Checked = true;
                        }
                    }
                }
            }
        }

        internal bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void FormOnLoad(object sender, EventArgs e)
        {
            if (IsRunAsAdmin())
            {
            }

            this.InitMenuStatus();

            this.tabPageMain.Controls.Add(this.CreateListBox("Scada.Main"));
            this.tabPageMainVision.Controls.Add(this.CreateListBox("Scada.MainVision"));
            this.dataUploadPage.Controls.Add(this.CreateListBox("Scada.Data.Client"));

            this.tabPage1.Controls.Add(this.CreateListBox("Scada.HPIC"));
            this.tabPage2.Controls.Add(this.CreateListBox("Scada.NaIDevice"));
            this.tabPage3.Controls.Add(this.CreateListBox("Scada.Weather"));
            this.tabPage4.Controls.Add(this.CreateListBox("Scada.MDS"));
            this.tabPage5.Controls.Add(this.CreateListBox("Scada.AIS"));
            this.tabPage6.Controls.Add(this.CreateListBox("Scada.Shelter"));
            this.tabPage7.Controls.Add(this.CreateListBox("Scada.DWD"));
            LoggerServer server = new LoggerServer();

            SynchronizationContext sc = SynchronizationContext.Current;
            server.Start((content) => 
            {
                sc.Post(new SendOrPostCallback(this.OnReceiveMessage), content);
            });
        }

        private void OnReceiveMessage(object state)
        {
            string content = (string)state;
            this.HandleMessage(content);
        }

        private void HandleMessage(string content)
        {
            if (content.StartsWith("["))
            {
                int e = content.IndexOf("]:");
                if (e > 0)
                {
                    string deviceKey = content.Substring(1, e - 1);
                    ListBox listBox = this.GetListBox(deviceKey);
                    if (listBox != null)
                    {
                        string logMsg = content.Substring(e + 2);
                        listBox.Items.Add(logMsg);
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                        listBox.SelectedIndex = -1;
                    }
                }
            }
        }

        internal void OnSendDetails(string deviceKey, string msg)
        {
            ListBox listBox = this.GetListBox(deviceKey);
            if (listBox != null)
            {
                listBox.Items.Add(msg);
            }
        }

        private void HandleCheckedChanged(string deviceKey, bool check)
        {
            string relFileName = string.Format("status\\@{0}", deviceKey);
            string fileName = ConfigPath.GetConfigFilePath(relFileName);
            if (check)
            {
                if (!File.Exists(fileName))
                {
                    using (File.Create(fileName))
                    { }
                }
            }
            else
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        private void OnStripMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            bool c = mi.Checked;

            mi.Checked = !c;

            string deviceName = (string)mi.Tag;
            this.HandleCheckedChanged(deviceName.ToLower(), mi.Checked);
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tab = this.tabControl.SelectedTab;
            object c = tab.Controls[0];
            if (c is ListBox)
            {
                ((ListBox)c).Items.Clear();
            }
        }

    }
}

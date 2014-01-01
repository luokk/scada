using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            ListBox lb = new ListBox();
            lb.Tag = deviceKey;
            lb.Dock = DockStyle.Fill;

            listBoxes.Add(lb);
            return lb;
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

        private void FormOnLoad(object sender, EventArgs e)
        {
            this.tabPageMain.Controls.Add(this.CreateListBox("Scada.Main"));
            this.tabPageMainVision.Controls.Add(this.CreateListBox("Scada.MainVision"));

            this.tabPage1.Controls.Add(this.CreateListBox("Scada.HPIC"));
            this.tabPage2.Controls.Add(this.CreateListBox("Scada.NaIDevice"));
            this.tabPage3.Controls.Add(this.CreateListBox("Scada.Weather"));
            this.tabPage4.Controls.Add(this.CreateListBox("Scada.HVSampler"));
            this.tabPage5.Controls.Add(this.CreateListBox("Scada.ISampler"));
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
                    ListBox box = this.GetListBox(deviceKey);
                    if (box != null)
                    {
                        string logMsg = content.Substring(e + 2);
                        box.Items.Add(logMsg);
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

        private void HandleCheckedChanged(string device)
        {

        }

        private void OnStripMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            bool c = mi.Checked;

            mi.Checked = !c;

            this.HandleCheckedChanged((string)mi.Tag);
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}

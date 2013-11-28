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
            // TODO:
        }

        internal void OnSendDetails(string deviceKey, string msg)
        {
            ListBox listBox = this.GetListBox(deviceKey);
            if (listBox != null)
            {
                listBox.Items.Add(msg);
            }
        }
    }
}

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
    public partial class RealTimeForm : Form
    {
        private Action CloseAction
        {
            get;
            set;
        }

        List<ListBox> listBoxes = new List<ListBox>();

        public RealTimeForm(Action action)
        {
            this.CloseAction = action;
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

        private void RealTimeForm_Load(object sender, EventArgs e)
        {
            this.tabPage1.Controls.Add(this.CreateListBox("Scada.HPIC"));
            this.tabPage2.Controls.Add(this.CreateListBox("Scada.NaIDevice"));
            this.tabPage3.Controls.Add(this.CreateListBox("Scada.Weather"));
            this.tabPage4.Controls.Add(this.CreateListBox("Scada.HVSampler"));
            this.tabPage5.Controls.Add(this.CreateListBox("Scada.ISampler"));
            this.tabPage6.Controls.Add(this.CreateListBox("Scada.Shelter"));
            this.tabPage7.Controls.Add(this.CreateListBox("Scada.DWD"));
        }

        private bool closed = false;

        private void RealTimeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.closed = true;
            this.CloseAction();
        }

        internal void OnSendDetails(string deviceKey, string msg)
        {
            if (this.closed)
                return;

            ListBox listBox = this.GetListBox(deviceKey);
            if (listBox != null)
            {
                listBox.Items.Add(msg);
            }
        }
    }
}

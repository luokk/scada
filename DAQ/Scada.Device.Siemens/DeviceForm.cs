using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Scada.Config;

namespace Scada.Device.Siemens
{
    public partial class DeviceForm : Form
    {
        private MDSDevice mdsDevice;

        private AISDevice aisDevice;


        public DeviceForm()
        {
            InitializeComponent();
        }

        private void btnConnMDS_Click(object sender, EventArgs e)
        {
            DeviceEntry entry = new DeviceEntry();
            entry[DeviceEntry.Name] = new StringValue("MDS");
            entry[DeviceEntry.Identity]= new StringValue( "Scada.MDS");
            entry[DeviceEntry.Path] = new StringValue("");
            entry[DeviceEntry.Version] = new StringValue("0.9");

            entry["IPADDR"] = new StringValue("192.168.0.5");

            this.mdsDevice = new MDSDevice(entry);
            this.mdsDevice.Start("S7200.OPCServer");

            this.mdsDevice.Send(Encoding.ASCII.GetBytes("connect;Sid=SAMPLE_ID;"), default(DateTime));
        }

        private void btnDisconnMDS_Click(object sender, EventArgs e)
        {
            if (this.mdsDevice != null)
            {
                this.mdsDevice.Send(Encoding.ASCII.GetBytes("disconnect"), default(DateTime));
            }
        }

        private void btnConnAIS_Click(object sender, EventArgs e)
        {
            DeviceEntry entry = new DeviceEntry();
            entry[DeviceEntry.Name] = new StringValue("AIS");
            entry[DeviceEntry.Identity] = new StringValue("Scada.AIS");
            entry[DeviceEntry.Path] = new StringValue("");
            entry[DeviceEntry.Version] = new StringValue("0.9");

            entry["IPADDR"] = new StringValue("192.168.0.6");
            this.aisDevice = new AISDevice(entry);
            this.aisDevice.Start("S7200.OPCServer");

            this.aisDevice.Send(Encoding.ASCII.GetBytes("connect"), default(DateTime));
        }

        private void btnDisconnAIS_Click(object sender, EventArgs e)
        {
            if (this.aisDevice != null)
            {
                this.aisDevice.Send(Encoding.ASCII.GetBytes("disconnect"), default(DateTime));
            }
        }
    }
}

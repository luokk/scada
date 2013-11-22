using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Scada.Config;
using System.IO.Ports;

namespace Scada.MainSettings
{
    public partial class DwdCfgForm : SettingFormBase, IApply
    {
        const string TheDeviceKey = "scada.dwd";

        private DwdSettings settings = new DwdSettings();

        public DwdCfgForm()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            this.settings = (DwdSettings)this.Apply(new Dictionary<string, string>
            {
                {DeviceEntry.SerialPort, this.settings.SerialPort},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()}
            });
        }

        public void Cancel()
        {
            this.settings = (DwdSettings)this.Reset();
        }

        private void DwdCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (DwdSettings)this.Reset();
        }

        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            DwdSettings settings = new DwdSettings();
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            settings.SerialPort = (StringValue)entry[DeviceEntry.SerialPort];
            
            return settings;
        }



    }
}

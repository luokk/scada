using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Scada.Config;

namespace Scada.MainSettings
{
    public partial class HpicCfgForm : SettingFormBase, IApply
    {
        const string TheDeviceKey = "scada.hpic";

        private HpicSettings settings = null;

        public HpicCfgForm()
        {
            InitializeComponent();
        }

        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        private void HpicCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (HpicSettings)this.Reset();
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            HpicSettings settings = new HpicSettings();
            settings.SerialPort = (StringValue)entry[DeviceEntry.SerialPort];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            settings.Factor = (StringValue)entry["factor1"];
            settings.AlarmValue = (StringValue)entry[DeviceEntry.Alarm1];
            return settings;
        }



        public void Apply()
        {
            this.settings = (HpicSettings)this.Apply(new Dictionary<string, string> 
            {
                {DeviceEntry.SerialPort, this.settings.SerialPort},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()},
                {DeviceEntry.Alarm1, this.settings.AlarmValue.ToString()},
                {"factor1", this.settings.Factor.ToString()}
            });
        }

        public void Cancel()
        {
            this.settings = (HpicSettings)this.Reset();
        }
    }

}

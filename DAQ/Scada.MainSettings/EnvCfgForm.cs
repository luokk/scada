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
    public partial class EnvCfgForm : SettingFormBase, IApply
    {
        const string TheDeviceKey = "scada.shelter";


        private ShelterSettings settings = new ShelterSettings();

        public EnvCfgForm()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            this.settings = (ShelterSettings)this.Apply(new Dictionary<string, string>
            {
                {DeviceEntry.SerialPort, this.settings.SerialPort},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()}
            });

        }

        public void Cancel()
        {
            this.settings = (ShelterSettings)this.Reset();
        }

        private void EnvCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (ShelterSettings)this.Reset();
        }


        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            ShelterSettings settings = new ShelterSettings();
            settings.SerialPort = (StringValue)entry[DeviceEntry.SerialPort];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            return settings;
        }
    }
}

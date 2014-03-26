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
    public partial class CinderlCfgForm : SettingFormBase, IApply
    {
        const string TheDeviceKey = "scada.weather";

        private WeatherSettings settings = new WeatherSettings();


        public CinderlCfgForm()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            this.settings = (WeatherSettings)this.Apply(new Dictionary<string, string>
            {
                {DeviceEntry.SerialPort, this.settings.SerialPort},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()}
            });
        }

        public void Cancel()
        {
            this.settings = (WeatherSettings)this.Reset();
        }

        private void WeatherCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (WeatherSettings)this.Reset();
        }


        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            WeatherSettings settings = new WeatherSettings();
            settings.SerialPort = (StringValue)entry[DeviceEntry.SerialPort];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            return settings;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Scada.Config;

namespace Scada.MainSettings
{
    public partial class NaICfgForm : SettingFormBase, IApply
    {
        private const string TheDeviceKey = "scada.naidevice";

        private NaISettings settings = new NaISettings();

        public NaICfgForm()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            this.settings = (NaISettings)this.Apply(new Dictionary<string, string>
            {
                {DeviceEntry.IPAddress, this.settings.IPAddress},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()},
                {DeviceEntry.DeviceSn, this.settings.DeviceSn},
                {"MinuteAdjust", this.settings.MinuteAdjust.ToString()}
            });
        }

        public void Cancel()
        {
            this.settings = (NaISettings)this.Reset();
        }

        

        private void NaICfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (NaISettings)this.Reset();
        }

        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            NaISettings settings = new NaISettings();
            settings.DeviceSn = (StringValue)entry[DeviceEntry.DeviceSn];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            settings.MinuteAdjust = (StringValue)entry["MinuteAdjust"];
            settings.IPAddress = (StringValue)entry[DeviceEntry.IPAddress];
            return settings;
        }

    }
}

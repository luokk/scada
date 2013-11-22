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
    public partial class IsCfgForm : SettingFormBase, IApply
    {
        public IsCfgForm()
        {
            InitializeComponent();
        }

        const string TheDeviceKey = "scada.isampler";

        private AisSettings settings = new AisSettings();

        public void Apply()
        {
            this.settings = (AisSettings)this.Apply(new Dictionary<string, string>
            {
                {"factor1", this.settings.Factor.ToString()},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()}
            });

        }

        public void Cancel()
        {
            this.settings = (AisSettings)this.Reset();
        }

        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            AisSettings settings = new AisSettings();
            settings.Factor = (StringValue)entry["factor1"];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            return settings;
        }

        private void IsCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (AisSettings)this.Reset();
        }
    }
}

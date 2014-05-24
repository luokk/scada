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
    public partial class MdsCfgForm : SettingFormBase, IApply
    {
        public MdsCfgForm()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            this.settings = (MdsSettings)this.Apply(new Dictionary<string, string>
            {
                {"factor1", this.settings.Factor.ToString()},
                {DeviceEntry.RecordInterval, this.settings.Frequence.ToString()}
            });
 
        }

        public void Cancel()
        {
            this.settings = (MdsSettings)this.Reset();
        }

        private void MdsCfgForm_Load(object sender, EventArgs e)
        {
            this.Loaded();
            this.settings = (MdsSettings)this.Reset();
        }

        const string TheDeviceKey = "scada.mds";

        private MdsSettings settings = new MdsSettings();

        protected override string GetDeviceKey()
        {
            return TheDeviceKey;
        }

        protected override object BuildSettings(DeviceEntry entry)
        {
            MdsSettings settings = new MdsSettings();
            settings.Factor = (StringValue)entry["factor1"];
            settings.Frequence = (StringValue)entry[DeviceEntry.RecordInterval];
            return settings;
        }
    }
}

using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scada.MainSettings
{
    public class SettingFormBase : UserControl
    {
        public const string SerialPortCOM1 = "COM1";

        // PropertyGrid~
        private PropertyGrid propertyGrid = null;

        public SettingFormBase()
        {
        }

        protected void Loaded()
        {
            this.Width = 640;
            this.Height = 400;

            this.propertyGrid = new PropertyGrid();
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.Dock = DockStyle.Fill;
            this.Controls.Add(this.propertyGrid);
        }

        private void UpdateSettings(object settings)
        {
            InitializePropertyGrid(settings);
        }

        private void InitializePropertyGrid(object settings)
        {
            this.propertyGrid.SelectedObject = settings;
        }

        // Must override
        protected virtual string GetDeviceKey()
        {
            return null;
        }

        protected virtual object BuildSettings(DeviceEntry entry)
        {
            return null;
        }

        protected object Reset()
        {
            string deviceKey = this.GetDeviceKey();
            Debug.Assert(deviceKey != null);
            string filePath = Program.GetDeviceConfigFile(deviceKey);
            DeviceEntry entry = DeviceEntry.GetDeviceEntry(deviceKey, filePath);
            object settings = this.BuildSettings(entry);
            this.UpdateSettings(settings);
            return settings;
        }

        protected object Apply(Dictionary<string, string> items)
        {
            var r = MessageBox.Show("是否修改配置？", "配置", MessageBoxButtons.YesNo);
            if (r == DialogResult.Yes)
            {
                string filePath = Program.GetDeviceConfigFile(this.GetDeviceKey());
                using (ScadaWriter sw = new ScadaWriter(filePath))
                {
                    foreach (var kv in items)
                    {
                        sw.WriteLine(kv.Key, kv.Value);
                    }

                    sw.Commit();
                }
                MessageBox.Show("配置已经更改，部分配置需要重启主程序才能生效！");
            }
            return this.Reset();
        }
    }
}

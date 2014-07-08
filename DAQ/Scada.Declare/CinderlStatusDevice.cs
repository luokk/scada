using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    public class CinderlStatusDevice : StandardDevice
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public CinderlStatusDevice(DeviceEntry entry)
            :base(entry)
        {

        }

        private int lastStatus = 0;

        public override bool OnReceiveData(byte[] line)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in line)
            {
                if (b >= 0x30 && b <= 0x39)
                {
                    sb.Append((char)b);
                }
            }

            string record = sb.ToString();
            int status = 0;
            if (int.TryParse(record, out status))
            {
                bool stateChanged = (this.lastStatus != status);
                this.lastStatus = status;

                this.CheckStatus(status);

                string statusLine = string.Format("STATUS:{0}", status);
                RecordManager.DoSystemEventRecord(this, statusLine);
                return stateChanged;
            }
            else
            {
                return false;
            }

        }

        private void CheckStatus(int status)
        {
            string statusBin = "00000000" + Convert.ToString(status, 2);
            statusBin = statusBin.Substring(statusBin.Length - 24);

            if (statusBin == "") 
            {
            }
            else if (statusBin == "010000000100001100111100")
            {
                // TODO: start
            }
            else if (statusBin == "110000000100001100111010")
            {
                // QAMeasure.bat
                this.ExecQAMeasure();
            }
            else if (statusBin == "110000000100001100111100")
            {
                // SampleMeasure24.bat
                this.ExecSample24HourMeasure();
            }

        }

        private void ExecQAMeasure()
        {
            var bat = ConfigPath.GetConfigFilePath("devices/Scada.HPGE/0.9/script/QAMeasure.bat");
            using (Process p = Process.Start(bat))
            {
            }
        }

        private void ExecSample24HourMeasure()
        {
            var bat = ConfigPath.GetConfigFilePath("devices/Scada.HPGE/0.9/script/SampleMeasure24.bat");
            using (Process p = Process.Start(bat))
            {
            }
        }
    }
}

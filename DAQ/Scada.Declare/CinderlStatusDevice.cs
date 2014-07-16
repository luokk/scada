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
            // Cinderella status标准输出是10，不等于10时，不做处理      by Kaikai
            if (line.Length != 10 && line.Length != 9)
            {
                RecordManager.DoSystemEventRecord(this, " cinderella status output bits exception!");
                return false;
            }

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

                //important 状态判断！ by Kaikai
                if (stateChanged)
                {
                    this.CheckStatus(status);
                }

                //string statusLine = string.Format("STATUS:{0}", status);
                //RecordManager.DoSystemEventRecord(this, statusLine);
                return true;
            }
            else
            {
                return false;
            }

        }

        private bool CheckStatus(int status)
        {
            // 转成2进制的string
            string str = Convert.ToString(status, 2);

            //
            if (str.Length == 23)
            {
                str = "0" + str;
            }

            if (str.Length != 24)
            {
                return false;
            }

            //取相反的序列，方便数位数
            int datalen = str.Length;
            string[] data = new string[datalen];
            for (int i = 0; i < datalen; i++)
            {
                data[i] = str.Substring(datalen - 1 - i, 1);
            }
            
            // 自动模式
            if (data[15] == "0")
            {
                // 24小时模式
                if (data[14] == "1" && data[13] == "0" && data[12] == "0" && data[11] == "0")
                {
                    return true;
                }

                if (data[14] == "0" && data[13] == "1" && data[12] == "0" && data[11] == "0")
                {
                    return true;
                }

                if (data[14] == "0" && data[13] == "0" && data[12] == "1" && data[11] == "0")
                {

                }

                if (data[14] == "0" && data[13] == "0" && data[12] == "0" && data[11] == "1")
                {

                }
            
            
            }





            return true;

            
            /*
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
            */
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

using Scada.Config;
using System;
using System.Collections.Generic;
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

                string statusLine = string.Format("STATUS:{0}", status);
                RecordManager.DoSystemEventRecord(this, statusLine);
                return stateChanged;
            }
            else
            {
                return false;
            }

        }
    }
}

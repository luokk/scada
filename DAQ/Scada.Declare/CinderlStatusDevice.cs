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

        private string lastRecord = string.Empty;

        public override bool OnReceiveData(byte[] line)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in line)
            {
                if (b == 0)
                {
                    sb.Append("0,");
                }
                else
                {
                    sb.Append("1,");
                }
            }

            string record = sb.ToString();
            record = record.Trim(',');

            bool stateChanged = (this.lastRecord != record);
            this.lastRecord = record;
            return stateChanged;
            //string statusLine = string.Format("COUNT:{0} {1}", line.Length, record);
            //RecordManager.DoSystemEventRecord(this, statusLine);
        }
    }
}

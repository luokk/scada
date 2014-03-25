using Scada.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    public class CinderlDataDevice : StandardDevice
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public CinderlDataDevice(DeviceEntry entry)
            :base(entry)
        {
        }

        public override bool OnReceiveData(byte[] line)
        {
            RecordManager.DoSystemEventRecord(this, Encoding.ASCII.GetString(line));
            return true;
        }
    }
}

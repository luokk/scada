using Scada.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    public class CinderlDataDevice : StandardDevice
    {
        private string CurrentSid
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public CinderlDataDevice(DeviceEntry entry)
            :base(entry)
        {
        }

        public override bool OnReceiveData(byte[] data)
        {
            // Cinderella data标准输出是203，不等于203时，不做处理      by Kaikai
            if (data.Length != 203) 
            {
                RecordManager.DoSystemEventRecord(this, " cinderella data output bits exception!");
                return false;
            }

            bool start = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; ++i)
            {
                byte b = data[i];
                if (!start && b == (byte)0x09)
                {
                    start = true;
                    continue;
                }
                else if (b == (byte)0x2f)
                {
                    break;
                }

                if (start)
                {
                    if (b == (byte)0x09)
                    {
                        sb.Append(',');
                    }
                    else
                    {
                        sb.Append((char)b);
                    }
                }
            }
            string content = sb.ToString();
            content = content.Trim(',');

            string[] ret = content.Split(',');

            string sid = ret[0];
            if (this.CurrentSid != sid)
            {
                this.CurrentSid = sid;
                if (!string.IsNullOrEmpty(this.CurrentSid))
                {
                    this.UpdateSidFile(this.CurrentSid);
                }
            }

            return true;
        }

        private void UpdateSidFile(string sid)
        {
            string path = LogPath.GetDeviceLogFilePath("scada.hpge");
            string sidFile = Path.Combine(path, "SID");
            using (FileStream fs = File.OpenWrite(sidFile))
            {
                var bs = Encoding.ASCII.GetBytes(sid);
                fs.Write(bs, 0, bs.Length);

                string sidFolder = Path.Combine(path, sid);
                Directory.CreateDirectory(sidFolder);
            }
        }
    }
}

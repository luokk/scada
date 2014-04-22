using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    class Value
    {
        public const int SysSend = 38;

        public const int SysReply = 91;


        internal static string Parse(string msg, string key)
        {
            string tof = string.Format("{0}=", key);
            int p = msg.IndexOf(tof);
            if (p > 0)
            {
                int p1 = msg.IndexOf(";", p);
                if (p1 < 0)
                    p1 = int.MaxValue;
                int p2 = msg.IndexOf(",", p);
                if (p2 < 0)
                    p2 = int.MaxValue;
                int e = Math.Min(p1, p2);

                if (e == int.MaxValue)
                {
                    e = msg.IndexOf("&&", p);
                    if (e < 0)
                    {
                        e = msg.Length;
                    }
                }
                int len = tof.Length;
                // 3 is CN='s length
                string value = msg.Substring(p + len, e - p - len);
                return value;
            }
            return string.Empty;
        }

        internal static string ParseInContent(string msg, string key)
        {
            msg = msg.Substring(msg.IndexOf("CP=&&"));
            return Parse(msg, key);
        }
            
    }

    /// <summary>
    /// 
    /// </summary>
    class DeviceTime
    {
        internal static DateTime Parse(string deviceTime)
        {
            // 2009 05 06 08 30 30
            try
            {
                int y = int.Parse(deviceTime.Substring(0, 4));
                int m = int.Parse(deviceTime.Substring(4, 2));
                int d = int.Parse(deviceTime.Substring(6, 2));
                int h = int.Parse(deviceTime.Substring(8, 2));
                int min = int.Parse(deviceTime.Substring(10, 2));
                int sec = int.Parse(deviceTime.Substring(12, 2));
                DateTime dt = new DateTime(y, m, d, h, min, sec);
                return dt;
            }
            catch (FormatException)
            {
                return (default(DateTime));
            }
        }

        internal static string Convert(DateTime time)
        {
            DateTime n = time;
            string value = string.Format("{0}{1:d2}{2:d2}{3:d2}{4:d2}{5:d2}", n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);

            return value;
        }
    }
}

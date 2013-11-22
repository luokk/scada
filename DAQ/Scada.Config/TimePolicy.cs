using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Config
{
    public class TimePolicy
    {
        public const int Every30Sec = 30;

        public const int Every1Min = 60;

        public const int Every5Min = 60 * 5;

        public TimePolicy()
        {
            this.Interval = TimePolicy.Every30Sec;
        }

        public int Interval
        {
            get;
            set;
        }

        public bool NowAtRightTime(out DateTime rightTime)
        {
            DateTime now = DateTime.Now;
            switch (this.Interval)
            {
                case TimePolicy.Every1Min:
                    return TimePolicy.AtVery1Min(now, out rightTime);
                case TimePolicy.Every5Min:
                    return TimePolicy.AtVery5Min(now, out rightTime);
                case TimePolicy.Every30Sec:
                default:
                    return TimePolicy.At30Sec(now, out rightTime);
            }
        }

        public static bool At30Sec(DateTime now, out DateTime rightTime)
        {
            const int MaxDelay = 10;
            rightTime = default(DateTime);
            if (now.Second >= 0 && now.Second <= MaxDelay)
            {
                int second = (now.Second < 30) ? 0 : 30;
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            else if (now.Second >= 30 && now.Second <= (30 + MaxDelay))
            {
                int second = (now.Second < 30) ? 0 : 30;
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            return false;
        }

        private static bool AtVery1Min(DateTime now, out DateTime rightTime)
        {
            const int MaxDelay = 15;
            
            rightTime = default(DateTime);
            if (now.Second >= 0 && now.Second <= MaxDelay)
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                return true;
            }
            return false;
        }

        private static bool AtVery5Min(DateTime now, out DateTime rightTime)
        {
            const int MaxDelay = 15;
            rightTime = default(DateTime);
            if (now.Minute % 5 == 0 && (now.Second >= 0 && now.Second <= MaxDelay))
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                return true;
            }
            return false;
        }

    }
}

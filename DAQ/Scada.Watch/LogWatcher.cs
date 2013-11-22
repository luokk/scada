using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Watch
{


    class LogWatcher
    {
        private DateTime lastModifyTime;

        private string logFile = null;

        private long timeout = 0;

        private int invalidCounter = 0;

        public LogWatcher(string logFile)
        {
            this.logFile = logFile;
        }

        public bool Check()
        {
            FileInfo fi = new FileInfo(this.logFile);
            if (fi.Exists)
            {
                this.lastModifyTime = fi.LastWriteTime;
            }
            else
            {
                DateTime n = DateTime.Now;
                DateTime today = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);
                this.lastModifyTime = today;
            }

            if (DateTime.Now.Ticks - this.lastModifyTime.Ticks >= timeout)
            {
                return true;
            }
            return false;
        }


    }
}

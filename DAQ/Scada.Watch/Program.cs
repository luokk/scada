using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Watch
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
#if !DEBUG
            if (processName.ToLower() != "watcher")
            {
                return;
            }
#endif
            bool createNew = false;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    //----------------------------------
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new WatchForm());
                }
                else
                {
                    MessageBox.Show("系统监控程序已经在运行中...");
                }
            }
        }
    }
}

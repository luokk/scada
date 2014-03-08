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
            if (processName.ToLower() != "watcher")
            {
                return;
            }
            bool createNew = false;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    WatchForm wf = new WatchForm();
                    wf.Text = processName + " in running";
                    Application.Run(wf);
                }
                else
                {
                    MessageBox.Show("系统监控程序已经在运行中...");
                }
            }
        }
    }
}

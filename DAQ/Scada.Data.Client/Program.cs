
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Data.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool second = false;
            bool createNew = false;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createNew))
            {
                if (!createNew)
                {
                    var r = MessageBox.Show("数据上传程序（HTTP）已经在运行中...", "选择是否允许第二个数据上传代理", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes)
                    {
                        second = true;
                    }
                    else
                    {
                        return;
                    }
                }

                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    MainDataAgentWindow form = new MainDataAgentWindow();
                    form.IsSecond = second;
                    Application.Run(form);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                
            }
        }

        public static string GetInstallPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }

        public static string GetDatePath(DateTime time)
        {
            return string.Format("{0}-{1:D2}", time.Year, time.Month);
        }

        public static string GetLogPath(string deviceKey)
        {
            string p = string.Format("{0}\\logs\\{1}\\{2}", GetInstallPath(), deviceKey, GetDatePath(DateTime.Now));
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            return p;
        }

        public const string DataClient = "Scada.Data.Client";

    }
}

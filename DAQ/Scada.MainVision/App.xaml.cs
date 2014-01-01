using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Scada.MainVision
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        private Mutex mutex;
        public App()
        {
            bool createNew;

            this.mutex = new Mutex(true, @"Scada.MainVision", out createNew);
            if (!createNew)
            {
                MessageBox.Show("应用程序[Scada.MainVision.exe]已经在运行中...");
                this.Shutdown();
            }
        }
	}
}

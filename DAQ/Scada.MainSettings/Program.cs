using Scada.Config;
using Scada.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Scada.MainSettings
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            /*
             * Test Code:
            string a = GetDeviceConfigFile("scada.hpic");
            ScadaWriter sw = new ScadaWriter(a);

            sw.WriteLine("factor1", new StringValue("2"));
            sw.Commit();
            */

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new SettingsForm());
		}

        public static string GetDeviceConfigFile(string deviceKey)
        {
            return ConfigPath.GetDeviceConfigFilePath(deviceKey, "0.9");
        }

	}
}

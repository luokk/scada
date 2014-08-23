using MySql.Data.MySqlClient;
using Scada.Chart;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for EnergyPanel.xaml
    /// </summary>
    public partial class EnergyPanel : UserControl
    {
        // private string deviceNaIFilesPath;
        // private DeviceEntry entry;

        private static int[] EmptyIntArray = new int[0];

        public EnergyPanel()
        {
            InitializeComponent();
            var p = this.Height;
            /*
            this.deviceNaIFilesPath = GetDeviceNaiPath();
            this.entry = DeviceEntry.GetDeviceEntry("Scada.NaIDevice", deviceNaIFilesPath + "\\device.cfg");
            **/
        }

        /*
        public static string InstallPath
        {
            get 
            {
                string p = Assembly.GetExecutingAssembly().Location;
                return System.IO.Path.GetDirectoryName(p);
            }
        }

        private static string GetDeviceNaiPath()
        {
            return string.Format("{0}\\devices\\Scada.NaIDevice\\0.9", InstallPath);
        }

        private string GetFileName(DateTime time)
        {
            int minuteAdjust = (StringValue)this.entry["MinuteAdjust"];
            string deviceSn = (StringValue)this.entry["DeviceSn"];
            string fileName;
            DateTime t = time;
            t = t.AddHours(-8).AddMinutes(minuteAdjust);
            fileName = string.Format("{0}_{1}-{2:D2}-{3:D2}T{4:D2}_{5:D2}_00Z-5min.n42",
                deviceSn, t.Year, t.Month, t.Day, t.Hour, t.Minute / 5 * 5);
            return fileName;
        }

        private string GetDatePath(DateTime date)
        {
            return string.Format("{0}-{1:D2}", date.Year, date.Month);
        }
        */
        
        public int[] GetNaICannelData(DateTime time, MySqlCommand cmd)
        {
            string cdLine = DBDataProvider.Instance.GetNaIDeviceChannelData(time, cmd);
            cdLine = cdLine.Trim();
            if (cdLine.Length > 0)
            {
                string[] cd = cdLine.Split(' ');

                int[] ret = new int[cd.Length];
                for (int i = 0; i < cd.Length; ++i)
                {
                    int v;
                    if (int.TryParse(cd[i], out v))
                    {
                        ret[i] = v;
                    }
                    else
                    {
                        ret[i] = 0;
                    }
                }
                return ret;
            }
            return EmptyIntArray;
        }


        //private
        internal void UpdateEnergyGraphByTime(DateTime time, MySqlCommand cmd)
        {
            int[] data = this.GetNaICannelData(time, cmd);
            if (data.Length > 0)
            {
                this.EnergyView.ClearPoints();
                this.EnergyView.SetDataPoints(data);
                // TODO: show the energy data in chart.
                // TODO: Chart...
            }
            else
            {
                // TODO: 文件不存在; But maybe data exists in DB.
            }
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {

        }

    }
}

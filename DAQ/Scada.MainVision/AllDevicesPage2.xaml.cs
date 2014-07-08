using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for AllDevicesPage.xaml
    /// </summary>
    public partial class AllDevicesPage2 : UserControl
    {
        public AllDevicesPage2()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.hpicPane.Initialize(new string[] { "剂量率"});
            this.weatherPane.Initialize(new string[] { "温度", "湿度", "雨量", "风速", "风向" });
            this.naiPane.Initialize(new string[] { "总剂量率" });
            this.shelterPane.Initialize(new string[] { "市电状态", "备电时间", "舱内温度" });

            Random ra = new Random(unchecked((int)DateTime.Now.Ticks)); 
            var _v6 = 100;
            var _v8 = 60;
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, evt) => 
            {
                string v1 = string.Format("{0} nGy/h", ra.Next(80, 90));
                this.hpicPane.SetData(new string[] { v1 });

                string v2 = string.Format("{0} ℃", ra.Next(16, 17));
                string v3 = string.Format("{0} %", ra.Next(23, 25));
                this.weatherPane.SetData(new string[] { v2, v3, "24 mm", "1-3级微风", "东南风" });

                string v4 = string.Format("{0} nSv/h", ra.Next(52, 60));
                this.naiPane.SetData(new string[] { v4 });

                this.shelterPane.SetData(new string[] { "市电", "5 H", "25 ℃" });

            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();
        }
    }
}

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
    public partial class AllDevicesPage : UserControl
    {
        public AllDevicesPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.hpicPane.Initialize(new string[] { "剂量率"});
            this.hpicPane.SetData(new string[] { "12 nGy/h" });


            this.weatherPane.Initialize(new string[] { "温度", "湿度", "雨量" });
            this.weatherPane.SetData(new string[] { "17 ℃", "25%", "24 mm" });


            this.naiPane.Initialize(new string[] { "总剂量率" });
            this.naiPane.SetData(new string[] { "89 nSv/h" });


            this.mdsPane.Initialize(new string[] { "累计采样体积", "瞬时采样流量" });
            this.mdsPane.SetData(new string[] { "10 m³", "4 m³/h" });

            this.aisPane.Initialize(new string[] { "累计采样体积", "瞬时采样流量" });
            this.aisPane.SetData(new string[] { "10 m³", "4 m³/h" });

            this.dwdPane.Initialize(new string[] { "采样状态" });
            this.dwdPane.SetData(new string[] { "<盖子打开>" });

            this.shelterPane.Initialize(new string[] { "市电状态", "备电时间", "舱内温度" });
            this.shelterPane.SetData(new string[] { "市电", "5 H", "25 ℃" });

        }
    }
}

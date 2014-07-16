using Scada.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for CinderellaPage.xaml
    /// </summary>
    public partial class CinderellaPage : UserControl
    {
        public CinderellaPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.cinderellaPane.Initialize(new string[]{"条形码", "开始时间", "工作时间", "压差值", "大气压", "瞬时流量", "累计流量", "温度"});
            this.hpgePane.Initialize(new string[] { "上次测量时间", "生成文件" });
            this.statusPane.Initialize(new string[] { 
                "当前状态", "工作模式", "循环时间", 
                "前玻璃门状态", "后玻璃门状态", "新滤纸夹仓状态", "旧滤纸夹仓状态", "切割位置状态", 
                "新滤纸夹具方向", "新率纸盒状态", "抽屉状态", "采样流量状态", "新滤纸夹仓门状态", "旧滤纸夹仓门状态", "紧急停止状态"});
        }

        private void UpdateCinderellaPanel(SmartDataPane panel)
        {
            var d = new Dictionary<string, object>();
            if (d == null)
            {
                return;
            }

            // "温度", "湿度", "雨量", "风速", "风向" "气压"
            panel.SetData(
                Get(d, "Temperature", "℃"),
                Get(d, "Humidity", "%"),
                Get(d, "Raingauge", "mm"),
                Get(d, "windspeed", "m/s"),
                Get(d, "direction", ""),
                Get(d, "pressure", "P"));
        }

        private string GetDisplayString(Dictionary<string, object> d, string key)
        {
            if (d.ContainsKey(key))
            {
                return (string)d[key];
            }
            return string.Empty;
        }

        private string Get(Dictionary<string, object> d, string key, string s)
        {
            return this.GetDisplayString(d, key.ToLower()) + " " + s;
        }

        internal void OnReceivedCommand(Common.Command cmd)
        {
            string statusBin = cmd.Content;
            // TODO: Parse statusBin

        }

        private void AutoClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }

        private void LoopClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }

        private void FillClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }

        private void CoverClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }

        private void PumpClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }

        private void hourClick(object sender, RoutedEventArgs e)
        {
            this.h1.IsChecked = false;
            this.h1.Foreground = Brushes.Gray;
            this.h6.IsChecked = false;
            this.h6.Foreground = Brushes.Gray;
            this.h8.IsChecked = false;
            this.h8.Foreground = Brushes.Gray;
            this.h24.IsChecked = false;
            this.h24.Foreground = Brushes.Gray;

            ToggleButton b = (ToggleButton)sender;
            b.IsChecked = true;
            b.Foreground = Brushes.Green;

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", "???"));
        }
    }
}

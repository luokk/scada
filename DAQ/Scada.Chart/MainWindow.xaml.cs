using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Scada.Chart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CurveView view2;
        CurveDataContext c2;

        private double i = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonReset(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ChartView.SetValueRange(-110, 110);

            DateTime t = DateTime.Parse("2014-10-02");
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (long i = 0; i <= 3600 * 24 * 1; i += 30)
            {
                if (i > 3600 * 4.2 && i < 3600 * 6.3)
                {
                    var item1 = new Dictionary<string, object>(3);
                    item1.Add("time", t.AddSeconds(i).ToString());
                    //item.Add("doserate", (double) (3600 * 24 - i) / 3600.0);
                    item1.Add("doserate", null);
                    data.Add(item1);
                    continue;
                }
                var item = new Dictionary<string, object>(3);
                item.Add("time", t.AddSeconds(i).ToString());
                //item.Add("doserate", (double) (3600 * 24 - i) / 3600.0);
                item.Add("doserate", (double)Math.Sin(3.14 / 288 * i / 30) * 100);
                data.Add(item);
            }

            this.ChartView.SetDataSource(data, "doserate");
        }
    }
}


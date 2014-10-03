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
        private PerformanceCounter cpuPerformance = new PerformanceCounter();

        CurveView view1;
        CurveView view2;

        CurveDataContext c1;
        CurveDataContext c2;

        private double i = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.view1 = ChartView.AddCurveView("a", "A");
            this.view1.Max = 150;
            this.view1.Min = 0;
            c1 = this.view1.AddCurveDataContext("a", "Hello");
            c1.ChartView = ChartView;


            this.view2 = ChartView.AddCurveView("b", "B");
            this.view2.Max = 100;
            this.view2.Min = -100;
            //this.view2.Background = new SolidColorBrush(Colors.Green);
            c2 = this.view2.AddCurveDataContext("a", "World");
            c2.ChartView = ChartView;

            //view2.Height = 200;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(AnimatedPlot);
            timer.IsEnabled = true;

            cpuPerformance.CategoryName = "Processor";
            cpuPerformance.CounterName = "% Processor Time";
            cpuPerformance.InstanceName = "_Total";
        }

        void AnimatedPlot(object sender, EventArgs e)
        {
            double x = i;
            i += 2;
            double y = cpuPerformance.NextValue();
            // c1.AddPoint(DateTime.Now, y * 2 + 20);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime t = DateTime.Parse("2014-10-02");
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (long i = 0; i <= 3600 * 48; i += 30)
            {
                if (i > 3600 * 4 && i < 3600 * 6)
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

            c2.SetDataSource(data, "doserate");
        }
    }
}


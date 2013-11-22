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
            c1 = this.view1.CreateDataContext("a", "Hello");
            //view1.Height = 200;
            this.view2 = ChartView.AddCurveView("b", "B");
            //this.view2.Background = new SolidColorBrush(Colors.Green);
            c2 = this.view2.CreateDataContext("a", "World");

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
            c1.AddTimeValuePair((int)x, y * 2 + 20, 3.0);
        }
    }
}

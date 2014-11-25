
using Scada.Controls.Data;
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
using Scada.Chart;
using System.Windows.Forms;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for GraphViewPanel.xaml
    /// </summary>
    public partial class GraphView : System.Windows.Controls.UserControl
    {
        public const string TimeKey = "Time";

        private DataListener dataListener;

        public GraphView()
        {
            InitializeComponent();
        }

        public int Interval
        {
            get
            {
                return this.ChartView.Interval;
            }

            set
            {
                this.ChartView.Interval = value;
            }
        }

        public void AddLineName(string deviceKey, string lineName, string displayName)
        {
            this.DeviceKey = deviceKey;
            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];

            ConfigItem item = entry.GetConfigItem(lineName);

            if (deviceKey == DataProvider.DeviceKey_AIS)
            {
                this.ChartView.SetCurveDisplayName("瞬时流量(L/h)");
            }
            else if (deviceKey == DataProvider.DeviceKey_MDS)
            {
                this.ChartView.SetCurveDisplayName("瞬时流量(m³/h)");
            }
            this.ChartView.SetValueRange(item.Min, item.Max);
            this.ChartView.HideResetButton();
            this.StartRealTimeChart();
        }

        // Save chart into BMP file
        internal void SaveChart()
        {
            string filePath = string.Empty;
            System.Windows.Forms.SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
                this.ChartView.SaveChart(filePath);
            }
            
        }

        public DateTime lastTime { get; set; }

        private void StartRealTimeChart()
        {
            /*
            this.dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            var dbConn = DBDataProvider.Instance.GetMySqlConnection();
            if (dbConn == null)
            {
                return;
            }

            dispatcherTimer.Tick += (s, evt) =>
            {
                DateTime fromTime = DateTime.Now.AddMinutes(-48);

                using (var dbCmd = dbConn.CreateCommand())
                {
                    var data = DBDataProvider.Instance.RefreshTimeRange2(this.DeviceKey, fromTime, DateTime.Now, dbCmd);
                    if (data.Count == 0)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 1, 30);
                    }
                    else
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
                    }
                    this.ChartView.SetDataSource2(data, "flow");
                }
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.dispatcherTimer != null)
                return;

            this.StartRealTimeChart();
            this.StartChart = true;
        }

        public bool StartChart { get; set; }

        public string DeviceKey { get; set; }

        public System.Windows.Threading.DispatcherTimer dispatcherTimer { get; set; }

        internal void SetDataSource2(List<Dictionary<string, object>> data, string p)
        {
            this.ChartView.SetDataSource2(data, p);
        }
    }

}

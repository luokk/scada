
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Scada.Chart;
using System.Windows.Forms;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for GraphViewPanel.xaml in MainVision
    /// </summary>
    public partial class SearchGraphView : System.Windows.Controls.UserControl
    {
        public const string TimeKey = "Time";

        DateTime now = DateTime.Now;

        static Color[] colors = { Colors.Green, Colors.Red, Colors.Blue, Colors.OrangeRed, Colors.Purple };
        
        private Dictionary<string, CurveDataContext> dataSources = new Dictionary<string, CurveDataContext>();

        public SearchGraphView()
        {
            InitializeComponent();

        }

        public void SetDataSource(List<Dictionary<string, object>> dataSource, string valueKey, int interval, int index, DateTime beginTime, DateTime endTime)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }
            if (valueKey == "temperature")
            {
                this.SearchChartView.SetCurveDisplayName("温度");
            }
            else if (valueKey == "pressure")
            {
                this.SearchChartView.SetCurveDisplayName("气压");
            }
            else if (valueKey == "windspeed")
            {
                this.SearchChartView.SetCurveDisplayName("风速");
            }

            this.Interval = interval;
            this.SearchChartView.Interval = this.Interval;
            this.SearchChartView.SetDataSource(dataSource, valueKey, beginTime, endTime);
        }

        public void AppendDataSource(List<Dictionary<string, object>> dataSource, string valueKey, int index)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }
            if (valueKey == "temperature")
            {
                this.SearchChartView.SetCurveDisplayName("温度");
            }
            else if (valueKey == "pressure")
            {
                this.SearchChartView.SetCurveDisplayName("气压");
            }
            else if (valueKey == "windspeed")
            {
                this.SearchChartView.SetCurveDisplayName("风速");
            }
            this.SearchChartView.AppendDataSource(dataSource, valueKey);
        }

        public int Interval
        {
            get;
            set;
        }

        public void AddLineName(string deviceKey, string lineName, string displayName)
        {
            // TODO:
            if (lineName.IndexOf("DoseRate") >= 0)
            {
                displayName = displayName.Replace("μSv/h", "nSv/h");
            }

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];

            ConfigItem item = entry.GetConfigItem(lineName);
            if (deviceKey == DataProvider.DeviceKey_AIS || deviceKey == DataProvider.DeviceKey_MDS)
            {
                this.SearchChartView.SetCurveDisplayName("瞬时流量");
            }
            else if (deviceKey == DataProvider.DeviceKey_NaI)
            {
                this.SearchChartView.SetCurveDisplayName("剂量率");
            }
            else if (deviceKey == DataProvider.DeviceKey_Weather)
            {
                this.SearchChartView.SetCurveDisplayName("温度");
            }
            if (this.Interval == 0)
            {
                this.Interval = 30;
                if (deviceKey == DataProvider.DeviceKey_NaI)
                {
                    this.Interval = 300;
                }
            }
            this.SearchChartView.SetValueRange(item.Min, item.Max);

        }

        internal void SaveChart()
        {
            string filePath = string.Empty;
            System.Windows.Forms.SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "曲线图片 (*.bmp)|*.bmp|All files (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
                this.SearchChartView.SaveChart(filePath);
            }            
        }

        internal void SelectChanged(string lineName)
        {
            Config cfg = Config.Instance();
            ConfigEntry entry = cfg["scada.weather"];

            ConfigItem item = entry.GetConfigItem(lineName);
            this.SearchChartView.SetValueRange(item.Min, item.Max);

        }
    }

}

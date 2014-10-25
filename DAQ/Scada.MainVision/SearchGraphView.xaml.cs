
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

        public void SetDataSource(List<Dictionary<string, object>> dataSource, string valueKey, int index, DateTime beginTime, DateTime endTime)
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
            get
            {
                return this.SearchChartView.Interval;
            }

            set
            {
                this.SearchChartView.Interval = value;
            }
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
            this.SearchChartView.SetValueRange(item.Min, item.Max);

        }

        private void AddTimePoint(DateTime time, Dictionary<string, object> entry)
        {
            foreach (string key in dataSources.Keys)
            {
                // 存在这条曲线
                if (entry.ContainsKey(key))
                {
                    string v = (string)entry[key];
                    double r = 0.0;
                    if (v.Length > 0)
                    {
                        if (!double.TryParse(v, out r))
                        {
                            return;
                        }
                    }

                    //this.CurveDataContext dataContext = dataSources[key];
                    //dataContext.AddPoint(time, r);
                }
            }
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
    }

}

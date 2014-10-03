
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
using System.Diagnostics;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for GraphViewPanel.xaml in MainVision
    /// </summary>
    public partial class SearchGraphView : UserControl
    {
        public const string TimeKey = "Time";

        private CurveDataContext defaultDataContext;

        DateTime now = DateTime.Now;

        static Color[] colors = { Colors.Green, Colors.Red, Colors.Blue, Colors.OrangeRed, Colors.Purple };
        
        private Dictionary<string, CurveDataContext> dataSources = new Dictionary<string, CurveDataContext>();

        public SearchGraphView()
        {
            InitializeComponent();

        }

        public void SetDataSource(List<Dictionary<string, object>> dataSource)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }

            this.defaultDataContext.SetDataSource(dataSource, "doserate");
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
            if (lineName.IndexOf("Doserate") >= 0)
            {
                displayName = displayName.Replace("μSv/h", "nSv/h");
            }

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];

            ConfigItem item = entry.GetConfigItem(lineName);

            CurveView curveView = this.SearchChartView.AddCurveView(lineName, displayName);
            
            curveView.Max = item.Max;
            curveView.Min = item.Min;
            curveView.Height = item.Height;
            CurveDataContext dataContext = curveView.AddCurveDataContext(lineName, displayName);
            dataContext.ChartView = this.SearchChartView;
            this.defaultDataContext = dataContext;
            this.dataSources.Add(lineName.ToLower(), dataContext);
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

                    CurveDataContext dataContext = dataSources[key];
                    dataContext.AddPoint(time, r);
                }
            }
        }

        internal void SaveChart()
        {
            this.SearchChartView.SaveChart();
        }
    }

}

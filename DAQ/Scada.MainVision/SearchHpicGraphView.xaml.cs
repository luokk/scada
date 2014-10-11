
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
using System.Windows.Forms;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for GraphViewPanel.xaml in MainVision
    /// </summary>
    public partial class SearchHpicGraphView : System.Windows.Controls.UserControl
    {
        public const string TimeKey = "Time";

        DateTime now = DateTime.Now;

        static Color[] colors = { Colors.Green, Colors.Red, Colors.Blue, Colors.OrangeRed, Colors.Purple };
        
        private Dictionary<string, CurveDataContext> dataSources = new Dictionary<string, CurveDataContext>();

        public SearchHpicGraphView()
        {
            InitializeComponent();

        }

        private List<Dictionary<string, object>> GetDataByInterval(List<Dictionary<string, object>> data, int interval)
        {
            int count = interval / 30;
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();

            var first = data[0];
            string beginTimeStr = (string)first["time"];
            DateTime beginTime = DateTime.Parse(beginTimeStr);
            DateTime endTime = beginTime.AddSeconds(interval);

            GroupValue gv = new GroupValue();
            for (int i = 0; i < data.Count; i++)
            {
                string time = (string)data[i]["time"];
                DateTime itemTime = DateTime.Parse(time);

                if (itemTime >= beginTime && itemTime < endTime)
                {
                    gv.AddValue(data[i], "doserate");
                }
                else
                {
                    Dictionary<string, object> newItem = gv.GetValue("doserate");
                    if (newItem != null)
                    {
                        newItem.Add("time", beginTime.ToString());
                        ret.Add(newItem);
                        gv.Clear();

                        beginTime = endTime;
                        endTime = endTime.AddSeconds(interval);
                    }
                }
            }

            return ret;
        }

        public void SetDataSource(List<Dictionary<string, object>> dataSource, string valueKey, int interval)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }

            List<Dictionary<string, object>> data = null;
            if (interval == 30)
            {
                data = dataSource;
            }
            else
            {
                data = this.GetDataByInterval(dataSource, interval);
            }
            this.SearchChartView.SetDataSource(data, valueKey);
            this.SearchChartView.SetUpdateRangeHandler((begin, end) => 
            {
                this.SearchChartView2.UpdateRange(begin, end);
            });
            this.SearchChartView2.SetDataSource(dataSource, "ifrain");
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
            this.SearchChartView.SetValueRange(item.Min, item.Max);
            this.SearchChartView.HideTimeAxis();

            this.SearchChartView2.Interval = 30;
            this.SearchChartView2.SetValueRange(0, 0.5);
            this.SearchChartView2.HideResetButton();
            this.SearchChartView2.DisableTrackingLine();
            this.SearchChartView2.DisableGridLine();
            // 方波颜色
            this.SearchChartView2.SetCurveColor(Color.FromRgb(0x00, 0x00, 0xCC));
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
            System.Windows.Forms.OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C://";
            fileDialog.Filter = "曲线图片 (*.bmp)|*.bmp|All files (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }
            this.SearchChartView.SaveChart(filePath);
        }
    }

}

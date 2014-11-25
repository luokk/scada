
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
            if (interval == 30)
            {
                return data;
            }
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
                    gv.AddValue(data[i], "doserate", "ifrain");
                }
                else
                {
                    Dictionary<string, object> newItem = gv.GetValue("doserate", "ifrain");
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

        public void SetDataSource(List<Dictionary<string, object>> dataSource, string valueKey, int interval, int index, DateTime beginTime, DateTime endTime)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }

            var data = dataSource;// this.GetDataByInterval(dataSource, interval);

            this.SearchChartView.Interval = interval;
            this.SearchChartView2.Interval = interval;
            this.SearchChartView.SetDataSource(data, valueKey, beginTime, endTime);
            this.SearchChartView.SetUpdateRangeHandler((begin, end) => 
            {
                this.SearchChartView2.UpdateRange(begin, end);
            });
            this.SearchChartView.SetResetHandler(() =>
            {
                this.SearchChartView2.Reset();
            });
            this.SearchChartView2.SetDataSource(data, "ifrain", beginTime, endTime);
        }

        public void SetDataSourceInterval(List<Dictionary<string, object>> dataSource, string valueKey, int interval, int expectedInterval, DateTime beginTime, DateTime endTime)
        {
            if (dataSource == null || dataSource.Count == 0)
            {
                return;
            }

            List<Dictionary<string, object>> data = null;
            if (expectedInterval > interval)
            {
                data = this.GetDataByInterval(dataSource, expectedInterval);
            }
            else if (expectedInterval == interval)
            {
                data = dataSource;
            }
            else { return; }


            this.SearchChartView.Interval = expectedInterval;
            this.SearchChartView2.Interval = expectedInterval;
            this.SearchChartView.SetDataSource(data, valueKey, beginTime, endTime);
            this.SearchChartView.SetUpdateRangeHandler((begin, end) =>
            {
                this.SearchChartView2.UpdateRange(begin, end);
            });
            this.SearchChartView.SetResetHandler(() =>
            {
                this.SearchChartView2.Reset();
            });
            this.SearchChartView2.SetDataSource(data, "ifrain", beginTime, endTime);
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
            this.SearchChartView.SetCurveDisplayName("剂量率(nGy/h)");
            this.SearchChartView.SetValueRange(item.Min, item.Max);
            this.SearchChartView.HideTimeAxis();

            this.SearchChartView2.Interval = 30;
            this.SearchChartView2.SetCurveDisplayName("感雨");
            this.SearchChartView2.SetValueRange(0, 100);
            this.SearchChartView2.HideResetButton();
            this.SearchChartView2.DisableTrackingLine();
            this.SearchChartView2.DisableGridLine();
            this.SearchChartView2.DisplayNameTop = 180;
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

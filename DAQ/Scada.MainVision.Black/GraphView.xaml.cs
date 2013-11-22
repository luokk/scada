
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

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for GraphViewPanel.xaml
    /// </summary>
    public partial class GraphView : UserControl
    {
        public const string TimeKey = "Time";

        int i = 0;

        DateTime now = DateTime.Now;

        private DataListener dataListener;

        private bool realTime = true;

        static Color[] colors = { Colors.Green, Colors.Red, Colors.Blue, Colors.OrangeRed, Colors.Purple };


        //private List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

        private Dictionary<string, CurveDataContext> dataSources = new Dictionary<string, CurveDataContext>();
        
        // private DataArrivalConfig config;

        private Dictionary<string, object> lastEntry;

        private bool baseTimeSet = false;

        public GraphView(bool realTime)
        {
            InitializeComponent();
            this.realTime = realTime;
            this.ChartView.RealTimeMode = realTime;
        }

        public void AddDataListener(DataListener listener)
        {
            this.dataListener = listener;
            if (this.dataListener != null)
            {
                this.dataListener.OnDataArrivalBegin += this.OnDataArrivalBegin;
                this.dataListener.OnDataArrival += this.OnDataArrival;
                this.dataListener.OnDataArrivalEnd += this.OnDataArrivalEnd;
            }
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
            // TODO:
            if (lineName.IndexOf("Doserate") >= 0)
            {
                displayName = displayName.Replace("μSv/h", "nSv/h");
            }

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];

            ConfigItem item = entry.GetConfigItem(lineName);

            CurveView curveView = this.ChartView.AddCurveView(lineName, displayName);
            
            curveView.Max = item.Max;
            curveView.Min = item.Min;
            curveView.Height = item.Height;
            CurveDataContext dataContext = curveView.CreateDataContext(lineName, displayName);

            this.dataSources.Add(lineName.ToLower(), dataContext);
        }


        private void OnDataArrivalBegin(DataArrivalConfig config)
        {
            if (config == DataArrivalConfig.TimeRange)
            {
                if (!this.realTime)
                {
                    // Clear
                    foreach (string key in dataSources.Keys)
                    {
                        CurveDataContext dataContext = dataSources[key];
                        i = 0;
                        dataContext.Clear();
                        dataContext.UpdateCurves();
                    }

                    // Reset the Base time.
                    this.baseTimeSet = false;

                    /*
                    foreach (string key in dataSources.Keys)
                    {
                        CurveDataContext dataContext = dataSources[key];
                        //dataContext.UpdateCurves();
                    }
                    */
                    
                }
            }
            else if (config == DataArrivalConfig.TimeNew)
            {
                if (this.realTime)
                {
                    // Do nothing with dataContext
                }
            }
        }

        private void OnDataArrival(DataArrivalConfig config, Dictionary<string, object> entry)
        {
            if (!entry.ContainsKey(TimeKey.ToLower()))
            {
                return;
            }

            string dataTime = (string)entry[TimeKey.ToLower()];
            if (config == DataArrivalConfig.TimeNew)
            {
                if (this.realTime)
                {
                    if (this.lastEntry != null)
                    {
                        string a = (string)this.lastEntry["time"];
                        if (a == dataTime)
                        {
                            return;
                        }
                    }

                    if (!this.baseTimeSet)
                    {
                        DateTime baseTime = DateTime.Parse(dataTime);
                        this.ChartView.UpdateTimeAxis(baseTime);
                        this.baseTimeSet = true;
                    }

                    this.AddTimePoint(i, entry);
                    this.lastEntry = entry;
                    i++;
                }
            }
            else if (config == DataArrivalConfig.TimeRange)
            {
                if (!this.realTime)
                {
                    if (!this.baseTimeSet)
                    {
                        DateTime baseTime = DateTime.Parse(dataTime);
                        this.ChartView.UpdateTimeAxis(baseTime);
                        this.baseTimeSet = true;
                    }

                    this.AddTimePoint(i, entry);
                    i++;
                }
            }

        }

        private void AddTimePoint(int index, Dictionary<string, object> entry)
        {
            UpdateResult result = UpdateResult.None;
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
                    result = dataContext.AddTimeValuePair(index * 5, r);
                }
            }

            if (UpdateResult.Overflow == result)
            {
                this.ChartView.UpdateTimeAxis(1);
            }
        }


        private void OnDataArrivalEnd(DataArrivalConfig config)
        {
            if (config == DataArrivalConfig.TimeRange)
            {
            }
        }

        private void ChartView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) // Shown
            {
                foreach (string key in dataSources.Keys)
                {
                    CurveDataContext dataContext = dataSources[key];
                    dataContext.UpdateCurves();
                }
            }

        }


        internal void SaveChart()
        {
            this.ChartView.SaveChart();
        }
    }

}

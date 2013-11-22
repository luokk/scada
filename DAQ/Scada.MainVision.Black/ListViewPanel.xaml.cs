
namespace Scada.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.Generic;

    using Scada.Controls.Data;
    using Scada.MainVision;
    using Microsoft.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.IO;
    using System.Text;
    using System.Diagnostics;
	/// <summary>
	/// Interaction logic for ListViewPanel.xaml
	/// </summary>
	public partial class ListViewPanel : UserControl
	{
        private Control listView = null;

        private Control searchView = null;

        private Control graphView = null;

        private Control graphSearchView = null;

        private Control ctrlView = null;

		private DataListener dataListener;

        private DataProvider dataProvider;

        private string deviceKey;

		private List<Dictionary<string, object>> dataSource;

        private List<Dictionary<string, object>> searchDataSource;

        // Data For Render
        private List<Dictionary<string, object>> searchData;

        private int searchDataIndex = 0;

        private DataArrivalConfig config;

        private const string Time = "time";

        const int MaxCountPage = 300;

        private const int MaxListCount = 26;

        // private bool ShowChartViewBySearch = true;

        // Must Use the <Full Name>
        private System.Windows.Forms.Timer refreshDataTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Display Name</param>
        /// <param name="interval">In Seconds</param>
        public ListViewPanel(DataProvider dataProvider, ConfigEntry entry)
		{
			InitializeComponent();
            this.deviceKey = entry.DeviceKey;
            this.DisplayName = entry.DisplayName;
            this.dataProvider = dataProvider;

            this.refreshDataTimer = new System.Windows.Forms.Timer();
            this.refreshDataTimer.Interval = (entry.Interval * 1000);
            this.refreshDataTimer.Tick += RefreshDataTimerTick;
            this.refreshDataTimer.Start();
		}


        internal void ListRecentData()
        {
            this.dataProvider.RefreshTimeline(this.deviceKey);
        }

        private void RefreshDataTimerTick(object sender, EventArgs e)
        {
            // TODO: Current settings? if show current, continue.
            // If filter by start -> end time, returns.

            // TODO: Check Whether the DeviceKey is in current...

            /*
            if (this.deviceKey != null)
            {
                if (this.deviceKey == this.dataProvider.CurrentDeviceKey)
                {
                    // this.dataProvider.RefreshTimeline(this.deviceKey);
                }
                else
                {
                    string msg = "Not current device key.";
                }
            }
            */
        }

        public Control ListView
		{
			get
			{
                return this.listView;
			}
			set
			{
                this.listView = value;
                if (this.listView != null)
                {
                    this.ListViewContainer.Content = this.listView;

                    ListView theListView = (ListView)this.listView;
                    // theListView.ItemsSource = this.dataSource;
                    this.ApplyListStyle(theListView);
                }
			}
		}

        public Control SearchView
        {
            get
            {
                return this.searchView;
            }

            set
            {
                this.searchView = value;
                if (this.listView != null)
                {
                    this.SearchViewContainer.Content = this.searchView;
                    this.ApplyListStyle((ListView)this.searchView);
                }
            }
        }

        public Control GraphView
		{
			get
			{
                return this.graphView;
			}
			set
			{
                this.graphView = value;
                if (this.graphView != null)
                {
                    this.GraphViewContainer.Content = this.graphView;
                }
			}
		}

        public Control GraphSearchView
        {
            get
            {
                return this.graphSearchView;
            }
            set
            {
                this.graphSearchView = value;
                if (this.graphSearchView != null)
                {
                    this.SearchGraphViewContainer.Content = this.graphSearchView;
                }
            }
        }


        public Control ControlPanel
        {
            get
            {
                return this.ctrlView;
            }

            set
            {
                this.ctrlView = value;
                if (this.ctrlView != null)
                {
                    this.ControlPanelTabItem.Visibility = Visibility.Visible;
                    this.ControlPanelContainer.Content = this.ctrlView;
                }
            }
        }

        public string DisplayName
        {
            get;
            set;
        }

		[Category("Behavior")]
		public event RoutedEventHandler CloseClick;

        private void ApplyListStyle(ListView listView)
        {
            Color c = Color.FromRgb(83, 83, 83);
            listView.Background = new SolidColorBrush(c);

            // listView.ItemContainerStyle = (Style)this.Resources["ListViewItemKey"];
            listView.Style = (Style)this.Resources["ListViewKey"];
        }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            DateTime now = DateTime.Now;
            this.FromDate.SelectedDate = now.AddDays(-2);
            this.ToDate.SelectedDate = now.AddDays(-1);
			// Can NOT Find Element in Template;
		}

		private void ContentLoaded(object sender, RoutedEventArgs e)
		{
            this.Title.Text = this.DisplayName;

            DateTime yestoday = DateTime.Now.AddDays(-1);
            DateTime from = new DateTime(yestoday.Year, yestoday.Month, yestoday.Day);

            DateTime to = from.AddDays(1).AddSeconds(-1);
            this.FromDateText.Text = from.ToString();
            this.ToDateText.Text = to.ToString();

            this.CloseButton.Click += (s, c) =>  {
		        this.CloseClick(this, c);
            };

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

            // TODO: !!
            this.dataSource = new List<Dictionary<string, object>>();

            this.searchDataSource = new List<Dictionary<string, object>>();
		}

        // BEGIN
        private void OnDataArrivalBegin(DataArrivalConfig config)
		{
            if (config == DataArrivalConfig.TimeNew)
            {
                // DO nothing for the realtime data-source
            }
            else if (config == DataArrivalConfig.TimeRange)
            {
                // For show new data source, so clear the old data source.
                this.searchDataSource.Clear();
            }
            else if (config == DataArrivalConfig.TimeRecent)
            {

            }
		}


        // ARRIVAL
		private void OnDataArrival(DataArrivalConfig config, Dictionary<string, object> entry)
		{
            if (config == DataArrivalConfig.TimeRecent)
            {
                // Debug.Assert(false, "Time Recent should not be here.");
                this.dataSource.Add(entry); 
            }
            else if (config == DataArrivalConfig.TimeRange)
            {
                this.searchDataSource.Add(entry);
            }
            else if (config == DataArrivalConfig.TimeNew)
            {
                const string Time = "time";
                if (!entry.ContainsKey(Time))
                {
                    return;
                }
                
                if (this.dataSource.Count > 0)
                {
                    Dictionary<string, object> latest = this.dataSource[0];
                    DateTime latestDateTime = DateTime.Parse((string)latest[Time]);

                    DateTime dt = DateTime.Parse((string)entry[Time]);
                    if (dt > latestDateTime)
                    {
                        this.dataSource.Insert(0, entry);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    this.dataSource.Add(entry);
                }

                ListView listView = (ListView)this.ListView;
                int selected = listView.SelectedIndex;
                listView.ItemsSource = null;
                listView.ItemsSource = this.dataSource;
                listView.SelectedIndex = selected;

                if (this.dataSource.Count > MaxListCount)
                {
                    int p = MaxListCount;
                    int l = this.dataSource.Count - p;
                    this.dataSource.RemoveRange(p, l);
                }
            }
		}

        // END
        private void OnDataArrivalEnd(DataArrivalConfig config)
		{
            if (config == DataArrivalConfig.TimeRecent)
            {

                if (this.ListView == null || !(this.ListView is ListView))
                    return;

                this.dataSource.Sort(DBDataProvider.DateTimeCompare);

                ListView listView = (ListView)this.ListView;
                // Remember the Selected item.
                int selected = listView.SelectedIndex;
                listView.ItemsSource = null;
                // List can only hold 100 items.
                if (this.dataSource.Count > MaxListCount)
                {
                    int p = 100;
                    int l = this.dataSource.Count - p;
                    this.dataSource.RemoveRange(p, l);
                }
                listView.ItemsSource = this.dataSource;
                listView.SelectedIndex = selected;

            }
            else if (config == DataArrivalConfig.TimeRange)
            {
                if (this.SearchView == null || !(this.SearchView is ListView))
                    return;

                this.searchDataSource.Sort(DBDataProvider.DateTimeCompare);

                ListView searchListView = (ListView)this.SearchView;
                searchListView.ItemsSource = null;
                searchListView.ItemsSource = this.searchDataSource;

            }
			
		}
        
        ////////////////////////////////////////////////////////////////////////////
        // When click the Search Button.
        private void SearchByDateRange(object sender, RoutedEventArgs e)
        {
            if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
            {
                this.FromDateText.Background = Brushes.Pink;
                this.ToDateText.Background = Brushes.Pink;

                return;
            }
            else
            {
                this.FromDateText.Background = Brushes.White;
                this.ToDateText.Background = Brushes.White;
            }

            var dt1 = DateTime.Parse(this.FromDateText.Text);
            var dt2 = DateTime.Parse(this.ToDateText.Text);

            this.searchDataSource = this.dataProvider.RefreshTimeRange(this.deviceKey, dt1, dt2);
            // int interval = this.currentInterval;
            this.searchData = this.Filter(this.searchDataSource, this.currentInterval);

            ListView searchListView = (ListView)this.SearchView;
            searchListView.ItemsSource = null;

            if (this.searchData != null && this.searchData.Count > 0)
            {
                // Show the searched data.
                searchListView.ItemsSource = this.searchData;
                // Enable the chart button.
                this.ButtonShowChart.IsEnabled = true;
            }

        }

        private List<Dictionary<string, object>> Filter(List<Dictionary<string, object>> data, int page)
        {
            this.searchDataIndex = page * MaxCountPage;
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            for (int i = 0; i < Math.Min(data.Count - this.searchDataIndex, MaxCountPage); ++i)
            {
                ret.Add(data[this.searchDataIndex + i]);
            }
            return ret;
        }

        private bool ValidTimeRange(string fromDate, string toDate)
        {
            try
            {
                return DateTime.Parse(fromDate) < DateTime.Parse(toDate);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void DatePickerCalendarClosed(object sender, RoutedEventArgs e)
        {
            DatePicker picker = (DatePicker)sender;
            if (picker.Name == "FromDate")
            {
                DateTime? dt = picker.SelectedDate;
                if (dt.HasValue)
                {
                    this.FromDateText.Text = dt.Value.ToString();
                }

                if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
                {
                    this.FromDateText.Background = Brushes.Pink;
                }
            }
            else if (picker.Name == "ToDate")
            {
                DateTime? dt = picker.SelectedDate;
                if (dt.HasValue)
                {
                    DateTime to = dt.Value.AddDays(1).AddSeconds(-1);
                    this.ToDateText.Text = to.ToString();
                }

                if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
                {
                    this.FromDateText.Background = Brushes.Pink;
                }
            }
    
        }

        private void OnPrevButton(object sender, RoutedEventArgs e)
        {
            this.OnNavigateTo(-1);
        }

        private void OnNextButton(object sender, RoutedEventArgs e)
        {
            this.OnNavigateTo(1);
        }

        private int currentPage = 0;

        private void OnNavigateTo(int nav)
        {
            int pageCount = this.searchDataSource.Count / MaxCountPage + 1;
            this.currentPage += nav;
            if (this.currentPage < 0)
            {
                this.currentPage = 0;
            }
            else if (this.currentPage >= pageCount) 
            {
                this.currentPage = pageCount - 1;
            }

            this.searchData = this.Filter(this.searchDataSource, this.currentPage);

            ListView searchListView = (ListView)this.SearchView;
            searchListView.ItemsSource = null;

            if (this.searchData != null && this.searchData.Count > 0)
            {
                // Show the searched data.
                searchListView.ItemsSource = this.searchData;
                // Enable the chart button.
                this.ButtonShowChart.IsEnabled = true;
            }
 

        }

        // Select the ChartView to show.
        private void ShowChartView(object sender, RoutedEventArgs e)
        {
            this.ChartViewTabItem.Visibility = Visibility.Visible;
            this.SearchChartViewTabItem.Visibility = Visibility.Collapsed;
            this.TabCtrl.SelectedItem = this.ChartViewTabItem;
            // this.ShowChartViewBySearch = false;
        }

        private void ShowSearchChartView(object sender, RoutedEventArgs e)
        {
            this.SearchChartViewTabItem.Visibility = Visibility.Visible;
            this.ChartViewTabItem.Visibility = Visibility.Collapsed;
            this.TabCtrl.SelectedItem = this.SearchChartViewTabItem;
            // this.ShowChartViewBySearch = true;
        }

        private void ExportDataList(object sender, RoutedEventArgs e)
        {
            this.ExportDataListToFile(this.dataSource);
        }

        private void ExportSearchDataList(object sender, RoutedEventArgs e)
        {
            this.ExportDataListToFile(this.searchDataSource);
        }

        private void ExportDataListToFile(List<Dictionary<string, object>> dataList)
        {
            DateTime now = DateTime.Now;
            string fileName = string.Format("{0}-{1}-{2}-{3}.csv", now.Year, now.Month, now.Day, now.Ticks);
            string filePath = string.Format("./csv/{0}", fileName);
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                foreach (Dictionary<string, object> i in dataList)
                {
                    StringBuilder sb = new StringBuilder();
                    
                    foreach (object item in i.Values)
                    {
                        sb.Append(item.ToString()).Append(",");
                    }
                    string line = sb.ToString(0, sb.Length - 1);
                    sw.WriteLine(line);
                }

                Window1 alert = new Window1("成功导出CSV文件。");
                alert.ShowDialog();
                
            }
        }

        private void SaveChart(object sender, RoutedEventArgs e)
        {
            ((GraphView)this.GraphView).SaveChart();
            Window1 alert = new Window1("成功保存曲线。");
            alert.ShowDialog();
        }

        private void SaveSearchChart(object sender, RoutedEventArgs e)
        {
            ((GraphView)this.GraphSearchView).SaveChart();
            Window1 alert = new Window1("成功保存曲线。");
            alert.ShowDialog();
        }


        public int currentInterval { get; set; }

        private void IntervalSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            switch (this.FrList.SelectedIndex)
            {
                case 0:
                    this.currentInterval = 30;
                    break;
                case 1:
                    this.currentInterval = 60 * 5;
                    break;
                case 2:
                    this.currentInterval = 60 * 60;
                    break;
                default:
                    this.currentInterval = 30;
                    break;
            }   
            */
        }

        private void SearchChartViewTabItemIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ((SearchGraphView)this.graphSearchView).SetDataSource(this.searchData);
            }
            
        }
    }
}

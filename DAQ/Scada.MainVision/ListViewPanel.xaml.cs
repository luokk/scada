
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
    using System.Windows.Media.Imaging;
    using System.Windows.Input;
    using System.Threading;
    using MySql.Data.MySqlClient;

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

        private Control energyView = null;

        private DataListener dataListener;

        private DBDataProvider dataProvider;

        private string deviceKey;

        private static string currentDeviceKey;

        private List<Dictionary<string, object>> dataSource;

        private List<Dictionary<string, object>> searchDataSource;

        // Data For Render
        private List<Dictionary<string, object>> searchData;

        private int searchDataIndex = 0;

        //private DataArrivalConfig config;

        private const string Time = "time";

        const int MaxCountPage = 2880;

        private const int MaxListCount = 26;

        private object currentSelectedItem;

        // private bool ShowChartViewBySearch = true;

        private MySqlConnection dbConn;

        private SynchronizationContext SynchronizationContext
        {
            get;
            set;
        }

        public bool HasSerachDataChart { get; set; }

        public bool HasRealTimeChart { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Display Name</param>
        /// <param name="interval">In Seconds</param>
        public ListViewPanel(DBDataProvider dataProvider, ConfigEntry entry)
        {
            InitializeComponent();
            this.deviceKey = entry.DeviceKey;
            this.DisplayName = entry.DisplayName;
            this.dataProvider = dataProvider;

            this.dbConn = this.dataProvider.GetMySqlConnection();
            if (dbConn == null)
            {
                return;
            }

            this.HasSerachDataChart = false;
            this.HasRealTimeChart = false;

            var dbCmd = this.dbConn.CreateCommand();
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            SynchronizationContext sc = SynchronizationContext.Current;
            dispatcherTimer.Tick += (s, evt) =>
            {
                if (this.Shown && this.deviceKey != currentDeviceKey)
                {
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
                    currentDeviceKey = this.deviceKey;
                    sc.Post(new SendOrPostCallback((o)=>
                    {
                        this.ListRecentData(dbCmd);
                    }), null);
                    
                }
                
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        internal void ListRecentData(MySqlCommand dbCmd)
        {
            this.dataProvider.RefreshTimeline(this.deviceKey, dbCmd);
        }

        public Control ListView
        {
            get { return this.listView; }
            set
            {
                this.listView = value;
                this.SetupListView();
            }
        }

        public Control SearchView
        {
            get { return this.searchView; }
            set
            {
                this.searchView = value;
                this.SetupSearchListView();
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

        public Control EnergyPanel
        {
            get
            {
                return this.energyView;
            }

            set
            {
                this.energyView = value;
                if (this.energyView != null)
                {
                    this.EnergyPanelTabItem.Visibility = Visibility.Visible;
                    this.EnergyPanelContainer.Content = this.energyView;
                }
            }
        }

        private void SetupListView()
        {
            if (this.listView != null)
            {
                this.ListViewContainer.Content = this.listView;

                ListView theListView = (ListView)this.listView;
                // theListView.ItemsSource = this.dataSource;
                this.ApplyListStyle(theListView);
                theListView.MouseRightButtonUp += OnListViewMouseRightButton;
            }
        }

        // Public method for setup the Context menu for ListView.
        public void SetupContextMenu(ListView listView)
        {
            ContextMenu cm = new ContextMenu();
            MenuItem mi1 = new MenuItem();
            mi1.Header = "显示能谱图";
            mi1.Click += this.ShowMenuItemClick;
            cm.Items.Add(mi1);
            /*
            MenuItem mi2 = new MenuItem();
            mi2.Header = "比较能谱图";
            mi2.Click += this.CompareMenuItemClick;
            cm.Items.Add(mi2);
            */
            listView.ContextMenu = cm;
        }

        void ShowMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (this.currentSelectedItem != null)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)this.currentSelectedItem;
                if (d.ContainsKey("time"))
                {
                    string t = (string)d["time"];
                    DateTime time;
                    if (DateTime.TryParse(t, out time))
                    {
                        this.ShowEnergy(time);
                    }
                }
            }
        }

        void CompareMenuItemClick(object sender, RoutedEventArgs e)
        {
            
        }

        void OnListViewMouseRightButton(object sender, MouseButtonEventArgs e)
        {
            this.currentSelectedItem = null;
            ListView listView = (ListView)sender;

            if (this.HasItemClicked(listView, e))
            {
                this.currentSelectedItem = listView.SelectedItem;
            }
        }

        private void SetupSearchListView()
        {
            if (this.searchView != null)
            {
                ListView theSearchView = (ListView)this.searchView;
                this.SearchViewContainer.Content = theSearchView;
                this.ApplyListStyle((ListView)theSearchView);
                theSearchView.MouseRightButtonUp += OnSearchListViewMouseRightButton;
            }
        }

        void OnSearchListViewMouseRightButton(object sender, MouseButtonEventArgs e)
        {
            ListView listView = (ListView)sender;
            this.currentSelectedItem = null;

            if (this.HasItemClicked(listView, e))
            {
                this.currentSelectedItem = listView.SelectedItem;
            }
        }

        private bool HasItemClicked(ListView listView, MouseButtonEventArgs e)
        {
            int index = listView.SelectedIndex;
            if (index < 0)
                return false;

            ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
            Rect rect = VisualTreeHelper.GetDescendantBounds(item);

            Point p = e.GetPosition(item);
            bool b = rect.Contains(p);
            return b;
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

            this.CloseButton.Click += (s, c) =>
            {
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
                this.dataSource.Clear();
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
                    int p = MaxListCount;
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

            this.FromDateText.Background = Brushes.White;
            this.ToDateText.Background = Brushes.White;
            var dt1 = DateTime.Parse(this.FromDateText.Text);
            var dt2 = DateTime.Parse(this.ToDateText.Text);

            using (var cmd = this.dbConn.CreateCommand())
            {
                this.searchDataSource = this.dataProvider.RefreshTimeRange(this.deviceKey, dt1, dt2, cmd);

                if (this.searchDataSource.Count == 0)
                {
                    MessageBox.Show("没有找到数据");
                    return;
                }
            }
            // int interval = this.currentInterval;
            this.searchData = this.Filter(this.searchDataSource, this.currentInterval);

            ListView searchListView = (ListView)this.SearchView;
            searchListView.ItemsSource = null;

            if (this.searchData != null && this.searchData.Count > 0)
            {

                searchListView.ItemsSource = this.searchData;
                if (this.deviceKey == DataProvider.DeviceKey_Hpic)
                {
                    ((SearchHpicGraphView)this.graphSearchView).SetDataSource(this.searchData, this.selectedField, 30);
                }
                else
                {
                    if (this.graphSearchView != null)
                    {
                        ((SearchGraphView)this.graphSearchView).SetDataSource(this.searchData, this.selectedField);
                    }
                }
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
            // From .Net3.5 => .Net4.0, 
            Microsoft.Windows.Controls.DatePicker picker = (Microsoft.Windows.Controls.DatePicker)sender;
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
                else
                {
                    this.FromDateText.Background = Brushes.White;
                    this.ToDateText.Background = Brushes.White;
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
                    this.ToDateText.Background = Brushes.Pink;
                }
                else
                {
                    this.FromDateText.Background = Brushes.White;
                    this.ToDateText.Background = Brushes.White;
                }

            }

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
            if (!Directory.Exists("./csv"))
            {
                Directory.CreateDirectory("./csv");
            }

            string filePath = string.Empty;
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.InitialDirectory = "./csv";
            fileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }
            else
            {
                return;
            }

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

                // Window1 alert = new Window1("成功导出CSV文件。");
                // alert.ShowDialog();
                MainWindow.Status = "成功导出CSV文件。";
            }
        }

        private void ShowEnergy(DateTime time)
        {
            this.EnergyPanelTabItem.Visibility = Visibility.Visible;
            this.TabCtrl.SelectedItem = this.EnergyPanelTabItem;
            EnergyPanel energyPanel = (EnergyPanel)this.EnergyPanel;

            using (var cmd = this.dbConn.CreateCommand())
            {
                energyPanel.UpdateEnergyGraphByTime(time, cmd);
            }

        }

        private void SaveChart(object sender, RoutedEventArgs e)
        {
            ((SearchGraphView)this.GraphSearchView).SaveChart();
            MainWindow.Status = "成功保存曲线。";
            /////////////////////////////////////////////////////////
            // Window1 alert = new Window1("成功保存曲线。");
            // alert.ShowDialog();
        }

        private void SaveSearchChart(object sender, RoutedEventArgs e)
        {
            ((SearchGraphView)this.GraphSearchView).SaveChart();
            MainWindow.Status = "成功保存曲线。";
            // Window1 alert = new Window1("成功保存曲线。");
            // alert.ShowDialog();
        }


        public int currentInterval { get; set; }

        private void SearchChartViewTabItemIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ((SearchGraphView)this.graphSearchView).SetDataSource(this.searchData, this.selectedField);
            }

        }

        internal void SetIcon(string icon)
        {
            if (icon != null)
            {
                this.Icon.Source = new BitmapImage(new Uri("pack://application:,,,/" + icon));
            }
        }

        private bool shown = false;

        public int selectedInterval = 30;
        
        public string selectedField;

        public bool Shown
        {
            get
            {
                return this.shown;
            }
            set
            {
                this.shown = value;
                // currentDeviceKey = this.deviceKey;
            }
        }

        private void SearchGraphViewContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.HasSerachDataChart)
            {
                this.SearchChartRow.Height = new GridLength(0);
            }
        }

        private void GraphViewContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.HasRealTimeChart)
            {
                this.ChartRow.Height = new GridLength(0);
            }
        }

        public void SetField(string field)
        {
            this.selectedField = field;
        }

        public void AddField(string field)
        {
            this.FieldSelect.Items.Add(field);
        }

        // Only for HPIC
        private void IntervalSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = (ComboBox)e.Source;
            if (this.searchData == null)
            {
                s.SelectedIndex = 0;
                return;
            }

            if (s.SelectedIndex == 0)
            {
                this.selectedInterval = 30;
            }
            else if (s.SelectedIndex == 1)
            {
                this.selectedInterval = 300;
            }
            else if (s.SelectedIndex == 2)
            {
                this.selectedInterval = 3600;
            }
            else if (s.SelectedIndex == 2)
            {
                this.selectedInterval = 3600 * 24;
            }

            ((SearchHpicGraphView)this.graphSearchView).Interval = this.selectedInterval;
            ((SearchHpicGraphView)this.graphSearchView).SetDataSource(this.searchData, this.selectedField, this.selectedInterval);
        }

        private void FieldSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = (ComboBox)e.Source;
            if (s.SelectedIndex == 0)
            {
                this.selectedField = "temperature";
            }
            if (s.SelectedIndex == 1)
            {
                this.selectedField = "pressure";
            }
            if (s.SelectedIndex == 2)
            {
                this.selectedField = "windspeed";
            }
            ((SearchGraphView)this.graphSearchView).SetDataSource(this.searchData, this.selectedField);

        }
    }
}

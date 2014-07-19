using Scada.Common;
using Scada.Config;
using Scada.Controls;
using Scada.Controls.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private DBDataProvider dataProvider;

		private PanelManager panelManager;

        private bool loaded = false;

        private static TextBlock statusBar;

        private CommandReceiver commandReceiver;

        private SynchronizationContext SynchronizationContext
        { get; set; }


        public static string Status
        {
            set
            {
                MainWindow.statusBar.Text = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
			this.panelManager = new PanelManager(this.Grid);
        }

        /// <summary>
        /// Load data Provider, and would set the provider into every ListViewPanel instance.
        /// </summary>
		private void LoadDataProvider()
		{
            DBDataProvider.Instance = new DBDataProvider();
            this.dataProvider = DBDataProvider.Instance;
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow.statusBar = this.StatusBar;
            this.Title = Settings.Instance.ApplicationName;


			// TODO: Window Loaded.
			this.LoadConfig();
			this.LoadDataProvider();

            // Device List
            this.DeviceList.ClickDeviceItem += this.OnDeviceItemClicked;
            this.DeviceList.MainWindow = this;

			Config cfg = Config.Instance();
			string[] deviceKeys = cfg.DeviceKeys;
			foreach (string deviceKey in deviceKeys)
            {
				string displayName = cfg.GetDisplayName(deviceKey);
				if (!string.IsNullOrEmpty(displayName))
				{
					this.DeviceList.AddDevice(displayName, deviceKey);
				}
            }

            this.AutoStationLabel.Text = Settings.Instance.StationName;
            this.CommStatusLabel.Text = "通信状态";
            this.DataCounterLabel.Text = "数据统计";

            this.AddPageEntry("自动站介绍", PanelManager.StationIntroduction, this.FirstShowTree);
            this.AddPageEntry("设备运行状态", PanelManager.DevicesRunStatus, this.FirstShowTree);
            this.AddPageEntry("特征核素识别系统", PanelManager.CinderellaRunStatus, this.FirstShowTree);

            this.AddPageEntry("当前通信状态", PanelManager.CurrentCommStatus, this.CommStatusTree);
            // this.AddPageEntry("历史通信状态", PanelManager.HistoryCommStatus, this.CommStatusTree);

            this.AddPageEntry("数据统计", PanelManager.DataCounter, this.CounterTree);
            // this.AddPageEntry("数据分析", this.CounterTree);


            this.ShowDataViewPanel("scada.hpic");
            // this.OnDeviceItemClicked(null, null);
            this.loaded = true;
            // Max when startup;
            this.OnMaxButton(null, null);

            this.SynchronizationContext = SynchronizationContext.Current;
            try
            {
                this.commandReceiver = new CommandReceiver(Ports.MainVision);
                this.commandReceiver.Start(this.OnReceivedCommandLine);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Command receiver initialized failed.");
            }
        }

        private void OnReceivedCommandLine(string line)
        {

            this.SynchronizationContext.Post(new SendOrPostCallback((o) => 
            {
                if (line == "Keep-Alive")
                {
                    return;
                }
                try
                {
                    Command cmd = Command.Parse(line);
                    string type = cmd.Type.Trim('"');
                    if (type == "cinderella.status")
                    {
                        this.panelManager.SendCommandToCinderellaPage(cmd);
                    }
                }
                catch (Exception)
                {
                    // Maybe NOT Json string
                }
            }), null);
        }

		private void LoadConfig()
		{
            string fileName = "dsm.cfg";
            if (Settings.Instance.IsCAS)
            {
                fileName = "dsm2.cfg";    
            }
            string filePath = ConfigPath.GetConfigFilePath(fileName);
            Config.Instance().Load(filePath);
		}

        private string HeaderContent
        {
            get;
            set;
        }

        private object ExpanderContent
        {
            get;
            set;
        }

        private static double ConvertDouble(double d, int n)
        {
            if (d == 0.0) return 0;
            if (d > 1 || d < -1)
                n = n - (int)Math.Log10(Math.Abs(d)) - 1;
            else
                n = n + (int)Math.Log10(1.0 / Math.Abs(d));
            if (n < 0)
            {
                d = (int)(d / Math.Pow(10, 0 - n)) * Math.Pow(10, 0 - n);
                n = 0;
            }
            return Math.Round(d, n);
        }

        private static bool ConvertDouble(string d, out double n)
        {
            n = 0.0;
            double v;
            if (double.TryParse(d, out v))
            {
                n = ConvertDouble(v, 3);
                return true;
            }
            return false;
        }

        private string GetDisplayString(Dictionary<string, object> d, string key)
        {
            if (d.ContainsKey(key))
            {
                return (string)d[key];
            }
            return string.Empty;
        }

        private ListViewPanel lastListViewPanel = null;

		private void ShowDataViewPanel(string deviceKey)
		{
            Config cfg = Config.Instance();
            var entry = cfg[deviceKey];

            ListViewPanel panel = this.panelManager.CreateDataViewPanel(this.dataProvider, entry);

            this.dataProvider.CurrentDeviceKey = deviceKey;
            
			panel.CloseClick += this.ClosePanelButtonClick;

			// Manage
            if (!this.Grid.Children.Contains(panel))
            {
                this.Grid.Children.Add(panel);
            }

            if (this.lastListViewPanel != null)
            {
                this.lastListViewPanel.Shown = false;
            }

			this.panelManager.SetListViewPanelPos(panel, 2, 2);
            this.lastListViewPanel = panel;
            this.lastListViewPanel.Shown = true;
		}

		void ClosePanelButtonClick(object sender, RoutedEventArgs e)
		{
			ListViewPanel panel = (ListViewPanel)sender;
			this.panelManager.CloseListViewPanel(panel);
		}

        void OnNaviItemClicked(object sender, EventArgs e)
        {
            NaviLabel nv = (NaviLabel)sender;
            string name = nv.Value;
            System.Windows.Controls.UserControl page = this.panelManager.GetPage(name);
            if (page == null)
            {
                page = this.panelManager.CreatePage(name, this.dataProvider);
            }
            this.panelManager.SetPage(name, page);
        }

        void OnDeviceItemClicked(object sender, EventArgs e)
        {
            if (sender is DeviceItem)
            {
                DeviceItem di = sender as DeviceItem;
                if (di != null)
                {
                    this.ShowDataViewPanel(di.DeviceKey);
                }
            }
            else if (sender is DeviceListPanel)
            {
                string name = "device-summary";
                System.Windows.Controls.UserControl page = this.panelManager.GetPage(name);
                if (page == null)
                {
                    page = this.panelManager.CreatePage(name, this.dataProvider);
                }
                this.panelManager.SetPage(name, page);
            }
        }

        private void OnExpanded(object sender, RoutedEventArgs e)
        {
            if (!this.loaded)
            {
                return;
            }

            if (this.ExpanderContent == null)
            {
                //this.ExpanderContent = this.Expander.Content;
            }

            Expander expander = sender as Expander;
            if (expander != null)
            {
                bool expanded = expander.IsExpanded;

                if (expanded)
                {
                    this.DeviceListColumn.Width = new GridLength(300.0);
                    //this.Expander.Content = this.ExpanderContent;
                    this.DeviceList.Visibility = Visibility.Visible;
                    /*
                    this.Expander.Header = this.HeaderContent;
                    this.Expander.Width = 300;
                     * */
                    //this.DeviceList.Margin = new Thickness(5, 0, 5, 0);
                    //this.Expander.Margin = new Thickness(3, 3, 3, 3);
                }
                else
                {
                    //this.Expander.Header = string.Empty;
                    //this.Expander.Content = null;
                    this.DeviceListColumn.Width = new GridLength(40.0);
                    
                    this.DeviceList.Visibility = Visibility.Hidden;
                    /*
                    this.HeaderContent = (string)this.Expander.Header;
                    
                    this.Expander.Width = 30;
                     * */
                    //this.DeviceList.Margin = default(Thickness);
                    //this.Expander.Margin = default(Thickness);
                }

            }
        }


        // Move the window by mouse-press-down.
        private void WindowMoveHandler(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// System Menu
        /// Close the Window.
        private void OnCloseButton(object sender, RoutedEventArgs e)
        {
            this.dataProvider.Quit = true;
            commandReceiver.Close();
            this.Close();
        }

        private void OnMaxButton(object sender, RoutedEventArgs e)
        {
            this.MaxWidth = SystemParameters.WorkArea.Width + 8;
            this.MaxHeight = SystemParameters.WorkArea.Height + 8;
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;

                this.SideColumn.Width = new GridLength(16.0);
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.SideColumn.Width = new GridLength(10.0);
            }
        }

        private void OnMinButton(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // private bool dataPanelHide = false;

        private bool devicePanelHide = false;


        public void OnHideDeviceButton(object sender, RoutedEventArgs e)
        {
            if (this.devicePanelHide)
            {
                this.devicePanelHide = false;
                this.DeviceListColumn.Width = new GridLength(220.0);
                this.DeviceList.Visibility = Visibility.Visible;
            }
            else
            {
                this.devicePanelHide = true;
                this.DeviceListColumn.Width = new GridLength(0.0);
                this.DeviceList.Visibility = Visibility.Collapsed;
            }
        }

        private const string DeviceItemTemplate = "DeviceTreeViewItem";

        public void AddPageEntry(string entryName, string tag, System.Windows.Controls.TreeView treeRoot)
        {
            Style ct = (Style)this.Resources[DeviceItemTemplate];

            TreeViewItem tvi = new TreeViewItem();
            tvi.Tag = tag;
            tvi.Style = ct;

            tvi.Selected += PageEntrySelected;
            tvi.Header = entryName;
            tvi.FontSize = 14.0;
            tvi.FontFamily = new FontFamily("微软雅黑");
            treeRoot.Items.Add(tvi);
        }

        void PageEntrySelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            string name = (string)tvi.Tag;
            System.Windows.Controls.UserControl page = this.panelManager.GetPage(name);
            if (page == null)
            {
                page = this.panelManager.CreatePage(name, this.dataProvider);
            }
            this.panelManager.SetPage(name, page);

            this.OnSelectionChanged(tvi);
        }

        // 控制多个TreeView Selected变化的代码
        private TreeViewItem currentSelectedTreeViewItem = null;

        internal void OnSelectionChanged(TreeViewItem tvi)
        {
            if (this.currentSelectedTreeViewItem != null)
            {
                this.currentSelectedTreeViewItem.IsSelected = false;
            }

            this.currentSelectedTreeViewItem = tvi;
        }
    }
}

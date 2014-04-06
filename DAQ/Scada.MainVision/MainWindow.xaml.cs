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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
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
		private DataProvider dataProvider;

		private PanelManager panelManager;


        private Timer refreshPanelDataTimer;

		private bool connectedToDataBase = true;

        private bool loaded = false;

        private static TextBlock statusBar;
        

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
			if (connectedToDataBase)
			{
                DBDataProvider.Instance = new DBDataProvider(); ;
                this.dataProvider = DBDataProvider.Instance;
			}
			else
			{
				this.dataProvider = new VirtualDataProvider();
			}
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow.statusBar = this.StatusBar;
            this.Title = "Nuclover-SCADA";


			// TODO: Window Loaded.
			this.LoadConfig();
			this.LoadDataProvider();

            // Device List
            this.DeviceList.ClickDeviceItem += this.OnDeviceItemClicked;
            this.DeviceList.MainWindow = this;

            this.AutoStationLabel.OnClick += OnNaviItemClicked;

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

            this.AutoStationLabel.Text = "山东威海站";
            this.CommStatusLabel.Text = "通信状态";
            this.DataCounterLabel.Text = "数据统计";
            this.ShowDataViewPanel("scada.hpic");
            // this.OnDeviceItemClicked(null, null);
            this.loaded = true;
            // Max when startup;
            this.OnMaxButton(null, null);
        }

		private void LoadConfig()
		{
            string fileName = ConfigPath.GetConfigFilePath("dsm.cfg");
            Config.Instance().Load(fileName);
		}


        /*
		void RefreshDataTimerTick(object sender, EventArgs e)
		{
			if (this.dataProvider != null)
			{
				this.dataProvider.Refresh();

                // this.refreshDataTimer.Stop();
			}
		}
        */

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

        // #007ACC
        /*
        private void CheckAlarm(HerePaneItem panel, string deviceKey, string item, int index, double value)
        {
            var i = Config.Instance()[deviceKey].GetConfigItem(item);
            if (!i.Alarm)
            {
                return;
            }
            TextBlock text = panel[index];
            if (value >= i.Red)
            {
                text.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 44));
            }
            else if (value >= i.Yellow && value < i.Red)
            {
                text.Foreground = Brushes.Orange;
            }
            else
            {
                text.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x7a, 0xCC));
            }
        }

        private void CheckAlarm(HerePaneItem panel, string deviceKey, string item, int index, string value)
        {
            double v;
            if (double.TryParse(value, out v))
            {
                this.CheckAlarm(panel, deviceKey, item, index, v);
            }
        }

        private void DisplayPanelData(HerePaneItem panel, string data1, string data2 = "", string data3 = "", string data4 = "")
        {
            TextBlock text1 = panel[0];
            TextBlock text2 = panel[1];
            TextBlock text3 = panel[2];
            TextBlock text4 = panel[3];

            if (data1 != null && data1.Length > 0)
            {
                text1.Text = data1;
            }
            if (data2 != null && data2.Length > 0)
            {
                text2.Text = data2;
            }
            if (data3 != null && data3.Length > 0)
            {
                text3.Text = data3;
            }
            if (data4 != null && data4.Length > 0)
            {
                text4.Text = data4;
            }
        }

        // 1 剂量率
        private void UpdatePanel_HPIC(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_Hpic);
            if (d == null)
            {
                return;
            }
            const string Doserate = "doserate";
            if (d.ContainsKey(Doserate))
            {
                string doserate = d[Doserate] as string;
                double v;
                if (ConvertDouble(doserate, out v))
                {
                    this.CheckAlarm(panel, DBDataProvider.DeviceKey_Hpic, Doserate, 0, v); 
                    string doserateMsg = "剂量率: " + v + "nGy/h";
                    this.DisplayPanelData(panel, doserateMsg);
                }
            }
            
        }
        */
        
        // 2 总剂量率、发现核素（置信度=100，剂量率>5nSv/h，最好可以设置剂量率的阈值）
        /*
         *  K-40 = K-40; (0, 100, 100) 
            I-131 = I-131; (0, 100, 100)
            Bi-214 = Bi-214; (0, 100, 100)
            Pb-214 = Pb-214; (0, 100, 100)
            Cs-137 = Cs-137; (0, 100, 100)
            Co-60 = Co-60; (0, 100, 100)
            Am-241 = Am-241; (0, 100, 100)
            Ba-140 = Ba-140;(0, 100, 100)
            Cs-134 = Cs-134;(0, 100, 100)
            I-133 = I-133; (0, 100, 100)
            Rh-106m = Rh-106m;(0, 100, 100)
            Ru-103 = Ru-103; (0, 100, 100)
            Te-129 = Te-129;(0, 100, 100)
         */
        /*
        private void UpdatePanel_NaI(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_NaI);
            if (d == null)
            {
                return;
            }
            const string Doserate = "doserate";
            if (!d.ContainsKey(Doserate))
            {
                return;
            }

            string doserate = (string)d[Doserate];
            string[] nuclides = { "K-40", "I-131", "Bi-214", "Pb-214", "Cs-137", "Co-60", "Am-241", "Ba-140", "Cs-134", "I-133", "Rh-106m", "Ru-103", "Te-129" };
            string[] nuclideMsgs = new string[3]{"", "", ""};
            int i = 0;
            
            foreach (string nuclide in nuclides)
            {
                string nuclideKey = nuclide.ToLower();
                if (d.ContainsKey(nuclideKey))
                {
                    string indicationKey = string.Format("Ind({0})", nuclideKey);
                    string indication = (string)d[indicationKey];
                    if (indication == "100")
                    {
                        
                        nuclideMsgs[i / 3] += string.Format("{0}, ", nuclide);
                        i++;
                    }
                }

            }

            string doserateMsg = "总剂量率: " + doserate + "nSv/h";
            double v;
            if (ConvertDouble(doserate, out v))
            {
                this.CheckAlarm(panel, DBDataProvider.DeviceKey_NaI, Doserate, 0, v);
            }

            for (int k = 0; k < 3; ++k)
            {
                nuclideMsgs[k] = nuclideMsgs[k].TrimEnd(' ', ',');
            }
            this.DisplayPanelData(panel, doserateMsg, 
                nuclideMsgs[0], nuclideMsgs[1], nuclideMsgs[2]);
        }
        // 3 // 风速、风向、雨量
        private void UpdatePanel_Weather(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_Weather);
            if (d == null)
            {
                return;
            }
            if (!d.ContainsKey("windspeed"))
            {
                return;
            }
            string windspeed = (string)d["windspeed"];
            string direction = (string)d["direction"];
            string rainspeed = (string)d["rainspeed"];

            string windspeedMsg = string.Format("风速: {0}m/s", windspeed);
            string directionMsg = string.Format("风向: {0}°", direction);
            string rainspeedMsg = string.Format("雨量: {0}mm/min", rainspeed);

            this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "windspeed", 0, windspeed);
            this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "direction", 1, direction);
            this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "rainspeed", 2, rainspeed);

            this.DisplayPanelData(panel, windspeedMsg, directionMsg, rainspeedMsg);
   
        }
        // 4 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_HV(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_HvSampler);
            if (d == null)
            {
                return;
            }

            string status = this.GetDisplayString(d, "status");
            string volume = this.GetDisplayString(d, "volume");
            string hours = this.GetDisplayString(d, "hours");
            string flow = this.GetDisplayString(d, "flow");

            string statusMsg;
            if (status == "1")
            {
                statusMsg = string.Format("采样状态: 运行"); 
            }
            else
            {
                statusMsg = string.Format("采样状态: 停止"); 
            }

            string volumeMsg = string.Format("累计采样体积: {0}m³", volume);
            string hoursMsg = string.Format("累计采样时间: {0}h", hours);
            string flowMsg = string.Format("瞬时采样流量: {0}m³/h", flow);

            this.DisplayPanelData(panel, statusMsg, volumeMsg, hoursMsg, flowMsg);

        }
        // 5 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_I(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_ISampler);
            if (d == null)
            {
                return;
            }
            string status = this.GetDisplayString(d, "status");
            string volume = this.GetDisplayString(d, "volume");
            string hours = this.GetDisplayString(d, "hours");
            string flow = this.GetDisplayString(d, "flow");

            string statusMsg;
            if (status == "1")
            {
                statusMsg = string.Format("采样状态: 运行");
            }
            else
            {
                statusMsg = string.Format("采样状态: 停止");
            }

            string volumeMsg = string.Format("累计采样体积: {0}L", volume);
            string hoursMsg = string.Format("累计采样时间: {0}h", hours);
            string flowMsg = string.Format("瞬时采样流量: {0}L/h", flow);

            this.DisplayPanelData(panel, statusMsg, volumeMsg, hoursMsg, flowMsg);
        }
        // 6 市电状态、备电时间、舱内温度、门禁报警、烟感报警、浸水报警
        private void UpdatePanel_Shelter(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_Shelter);
            if (d == null)
            {
                return;
            }

            string batteryHours = "";
            string mainPowerWay = "";
            string temperature = "";

            const string MainPowKey = "ifmainpoweroff";
            const string BatteryHoursKey = "batteryhours";
            const string TemperatureKey = "temperature";
            if (d.ContainsKey(MainPowKey))
            {
                string m = (string)d[MainPowKey];
                mainPowerWay = (m == "1") ? "市电" : "蓄电池";
            }

            if (d.ContainsKey(BatteryHoursKey))
            {
                batteryHours = (string)d[BatteryHoursKey];
                double v;
                if (double.TryParse(batteryHours, out v))
                {
                    batteryHours = Math.Round(v, 0).ToString();
                }
            }

            if (d.ContainsKey(TemperatureKey))
            {
                temperature = (string)d[TemperatureKey];
                double v;
                if (double.TryParse(temperature, out v))
                {
                    temperature = Math.Round(v, 0).ToString();
                }
                
            }

            string mainPowMsg = string.Format("供电方式: {0}", mainPowerWay);
            string batteryHoursMsg = string.Format("备电时间: {0}h", batteryHours);
            string tempMsg = string.Format("舱内温度: {0}℃", temperature);

            this.DisplayPanelData(panel, mainPowMsg, batteryHoursMsg, tempMsg);
        }
        // 7 仅工作状态
        private void UpdatePanel_DWD(HerePaneItem panel)
        {
            var d = this.dataProvider.GetLatestData(DataProvider.DeviceKey_Dwd);
            if (d == null)
            {
                return;
            }
            if (!d.ContainsKey("islidopen"))
            {
                return;
            }
            string isLidOpen = (string)d["islidopen"];
            string LidOpenMsg = (isLidOpen == "1") ? "雨水采集" : "沉降灰采集";
            this.DisplayPanelData(panel, "采样状态:" + LidOpenMsg);
        }
        */

        private string GetDisplayString(Dictionary<string, object> d, string key)
        {
            if (d.ContainsKey(key))
            {
                return (string)d[key];
            }
            return string.Empty;
        }

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

			this.panelManager.SetListViewPanelPos(panel, 2, 2);
		}

		void ClosePanelButtonClick(object sender, RoutedEventArgs e)
		{
			ListViewPanel panel = (ListViewPanel)sender;
			this.panelManager.CloseListViewPanel(panel);
		}

        void OnNaviItemClicked(object sender, EventArgs e)
        {

        }

        void OnDeviceItemClicked(object sender, EventArgs e)
        {
            DeviceItem di = sender as DeviceItem;
            if (di != null)
            {
                this.ShowDataViewPanel(di.DeviceKey);
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

        private bool dataPanelHide = false;

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



    }
}

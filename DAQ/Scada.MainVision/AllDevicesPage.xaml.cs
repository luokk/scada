using MySql.Data.MySqlClient;
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

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for AllDevicesPage.xaml
    /// </summary>
    public partial class AllDevicesPage : UserControl
    {
        private DBDataProvider dataProvider;

        public AllDevicesPage()
        {
            InitializeComponent();
        }

        public void SetDataProvider(DBDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        private MySqlConnection dbConn = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.hpicPane.Initialize(new string[] { "最近采集时间", "剂量率"});
            this.weatherPane.Initialize(new string[] { "最近采集时间", "温度", "湿度", "雨量", "风速", "风向", "气压" });
            this.naiPane.Initialize(new string[] { "最近采集时间", "总剂量率" });
            this.mdsPane.Initialize(new string[] { "最近采样时间", "瞬时采样流量", "累计采样流量", "累积采样时间", "滤纸报警", "流量报警", "主电源报警"});
            this.aisPane.Initialize(new string[] { "最近采样时间", "瞬时采样流量", "累计采样流量", "累积采样时间", "滤纸报警", "流量报警", "主电源报警" });
            this.dwdPane.Initialize(new string[] { "最近采集时间", "采样状态" });
            this.rainPane.Initialize(new string[] { "最近采集时间", "降雨状态" });
            this.shelterPane.Initialize(new string[] { "最近采集时间", "市电状态", "备电时间", "舱内温度", "门禁报警", "烟感报警", "浸水报警" });


            this.dbConn = this.dataProvider.GetMySqlConnection();

            if (this.dbConn != null)
            {
                MySqlCommand cmd = this.dbConn.CreateCommand();
                var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += (s, evt) =>
                {
                    this.RefreshTick(cmd);
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
                dispatcherTimer.Start();
                this.RefreshTick(cmd);
            }
            else
            { return; }
            
        }

        private void RefreshTick(MySqlCommand cmd)
        {
            this.dataProvider.RefreshTimeNow(cmd);

            UpdatePanel_HPIC(this.hpicPane);
            UpdatePanel_Weather(this.weatherPane);
            UpdatePanel_NaI(this.naiPane);
            UpdatePanel_MDS(this.mdsPane);
            UpdatePanel_AIS(this.aisPane);
            UpdatePanel_Shelter(this.shelterPane);
            UpdatePanel_DWD(this.dwdPane);
            UpdatePanel_Rain(this.rainPane);
        }

        private void UpdatePanel_HPIC(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Hpic);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
            {
                return;
            }

            const string Doserate = "doserate";
            panel.SetData(Get(d, "time", ""), Get(d, Doserate, "nGy/h"));
        }
        
        
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
        private void UpdatePanel_NaI(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_NaI);
            panel.Check(Get(d, "time", ""));

            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
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
                    // string indicationKey = string.Format("Ind({0})", nuclideKey);
                    string indication = (string)d[nuclideKey];
                    if (indication.IndexOf("(100)") > 0)
                    {
                        nuclideMsgs[i / 3] += string.Format("{0}, ", nuclide);
                        i++;
                    }
                }

            }

            for (int k = 0; k < 3; ++k)
            {
                nuclideMsgs[k] = nuclideMsgs[k].TrimEnd(' ', ',');
            }
            
            panel.SetData(Get(d, "time", ""), Get(d, Doserate, "nSv/h"));
        }

        private void UpdatePanel_Weather(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Weather);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
            {
                return;
            }
            
            // "温度", "湿度", "雨量", "风速", "风向" "气压"
            // 风向换算
            string strDirection = Get(d, "direction", "");
            int direction = int.Parse(strDirection.Trim());
            if ( 348 < direction && direction <= 360 )
            {
                strDirection += " (N)";
            }
            else if (direction <= 11)
            {
                strDirection += " (N)";
            }
            else if ( 11 < direction && direction <= 33 )
            {
                strDirection += " (NNE)";
            }
            else if (33 < direction && direction <= 56)
            {
                strDirection += " (NE)";
            }
            else if (56 < direction && direction <= 78)
            {
                strDirection += " (ENE)";
            }
            else if (78 < direction && direction <= 101)
            {
                strDirection += " (E)";
            }
            else if (101 < direction && direction <= 123)
            {
                strDirection += " (ESE)";
            }
            else if (123 < direction && direction <= 146)
            {
                strDirection += " (SE)";
            }
            else if (146 < direction && direction <= 168)
            {
                strDirection += " (SSE)";
            }
            else if (168 < direction && direction <= 191)
            {
                strDirection += " (S)";
            }
            else if (191 < direction && direction <= 213)
            {
                strDirection += " (SSW)";
            }
            else if (213 < direction && direction <= 236)
            {
                strDirection += " (SW)";
            }
            else if (236 < direction && direction <= 258)
            {
                strDirection += " (WSW)";
            }
            else if (258 < direction && direction <= 281)
            {
                strDirection += " (W)";
            }
            else if (281 < direction && direction <= 303)
            {
                strDirection += " (WNW)";
            }
            else if (303 < direction && direction <= 326)
            {
                strDirection += " (NW)";
            }
            else if (326 < direction && direction <= 348)
            {
                strDirection += " (NNW)";
            }

            panel.SetData(
                Get(d, "time", ""), 
                Get(d, "Temperature", "℃"),
                Get(d, "Humidity", "%"),
                Get(d, "Raingauge", "mm"),
                Get(d, "windspeed", "m/s"),
                strDirection,
                Get(d, "pressure", "Pa"));
        }

        // 4 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_MDS(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_MDS);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
            {
                return;
            }

            //"瞬时采样流量", "累计采样流量", "累积采样时间"
            panel.SetData(
                Get(d, "time", ""), 
                Get(d, "flow", "m³/h"),
                Get(d, "volume", "m³"),
                Get(d, "hours", "h"),
                GetAlarm(d, "alarm1", ""),
                GetAlarm(d, "alarm2", ""),
                GetAlarm(d, "alarm3", ""));

            MarkAlarm(d, "alarm1", panel, 4);
            MarkAlarm(d, "alarm2", panel, 5);
            MarkAlarm(d, "alarm3", panel, 6);
        }
        // 5 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_AIS(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_AIS);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
            {
                return;
            }

            //"瞬时采样流量", "累计采样流量", "累积采样时间"
            panel.SetData(
                Get(d, "time", ""), 
                Get(d, "flow", "L/h"),
                Get(d, "volume", "L"),
                Get(d, "hours", "h"),
                GetAlarm(d, "alarm1", ""),
                GetAlarm(d, "alarm2", ""),
                GetAlarm(d, "alarm3", ""));

            MarkAlarm(d, "alarm1", panel, 4);
            MarkAlarm(d, "alarm2", panel, 5);
            MarkAlarm(d, "alarm3", panel, 6);
        }
        // 6 市电状态、备电时间、舱内温度、门禁报警、烟感报警、浸水报警
        private void UpdatePanel_Shelter(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Shelter);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
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

            string mainPowMsg = string.Format("{0}", mainPowerWay);
            string batteryHoursMsg = string.Format("{0}h", batteryHours);
            string tempMsg = string.Format("{0}℃", temperature);

            panel.SetData(
                Get(d, "time", ""), 
                mainPowMsg, 
                batteryHoursMsg, 
                tempMsg,
                GetAlarm(d, "ifdooropen", ""),
                GetAlarm(d, "ifsmoke", ""),
                GetAlarm(d, "ifwater", ""));

            MarkAlarm(d, "ifdooropen", panel, 4);
            MarkAlarm(d, "ifsmoke", panel, 5);
            MarkAlarm(d, "ifwater", panel, 6);

        }
        // 7 仅工作状态
        private void UpdatePanel_DWD(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Dwd);
            panel.Check(Get(d, "time", ""));
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

            panel.SetData(Get(d, "time", ""), LidOpenMsg);
        }

        private void UpdatePanel_Rain(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Dwd);
            panel.Check(Get(d, "time", ""));
            // NOTICE：数据库中没有任何记录时，d的对象仍然可以创建成功，所以需要加入d.Count==0
            if (d == null || d.Count == 0)
            {
                return;
            }

            if (!d.ContainsKey("ifrain"))
            {
                return;
            }
            string ifRain = (string)d["ifrain"];
            string ifRainStr = (ifRain == "1") ? "降雨" : "未降雨";

            panel.SetData(Get(d, "time", ""), ifRainStr);
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

        private string Get(Dictionary<string, object> d, string key, string s)
        {
            string v = this.GetDisplayString(d, key.ToLower());
            double dv;
            if (double.TryParse(v, out dv))
            {
                return dv.ToString("0.0") + " " + s; 
            }
            return v + " " + s; 
        }

        private string GetAlarm(Dictionary<string, object> d, string key, string s)
        {
            string v = this.GetDisplayString(d, key.ToLower());
            bool alarm = (v == "1");
            return alarm ? "报警" : "正常";
        }

        private void MarkAlarm(Dictionary<string, object> d, string key, SmartDataPane pane, int index)
        {
            string v = this.GetDisplayString(d, key.ToLower());
            bool alarm = (v == "1");
            if (alarm)
            {
                pane.SetDataColor(index, Brushes.Red, true);
            }
            else
            {
                pane.SetDataColor(index, Brushes.Green, false);
            }
        }
    }
}

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
        private DataProvider dataProvider;

        public AllDevicesPage()
        {
            InitializeComponent();
        }

        public void SetDataProvider(DataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.hpicPane.Initialize(new string[] { "剂量率"});
            this.weatherPane.Initialize(new string[] { "温度", "湿度", "雨量", "风速", "风向" });
            this.naiPane.Initialize(new string[] { "总剂量率" });
            this.mdsPane.Initialize(new string[] { "瞬时采样流量", "累计采样流量" });
            this.aisPane.Initialize(new string[] { "瞬时采样流量", "累计采样流量" });
            this.dwdPane.Initialize(new string[] { "采样状态" });
            this.shelterPane.Initialize(new string[] { "市电状态", "备电时间", "舱内温度" });

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, evt) => 
            {
                //string v1 = string.Format("{0} nGy/h", ra.Next(80, 90));
                //this.hpicPane.SetData(new string[] { v1 });
                this.UpdatePanel_HPIC(this.hpicPane);

                //string v2 = string.Format("{0} ℃", ra.Next(16, 17));
                //string v3 = string.Format("{0} %", ra.Next(23, 25));
                //this.weatherPane.SetData(new string[] { v2, v3, "24 mm", "1-3级微风", "东南风" });
                UpdatePanel_Weather(this.weatherPane);

                //string v4 = string.Format("{0} nSv/h", ra.Next(52, 60));
                //this.naiPane.SetData(new string[] { v4 });
                UpdatePanel_NaI(this.naiPane);

                //string v5 = string.Format("{0} m³/h", ra.Next(800, 810));
                //string v6 = string.Format("{0} m³", _v6 += 10);
                //this.mdsPane.SetData(new string[] { v5, v6 });
                //UpdatePanel_HV(this.mdsPane);

                //string v7 = string.Format("{0} m³/h", ra.Next(11, 13));
                //string v8 = string.Format("{0} m³", _v8 += 2);
                UpdatePanel_I(this.aisPane);

                //this.dwdPane.SetData(new string[] { "<盖子打开>" });
                //this.shelterPane.SetData(new string[] { "市电", "5 H", "25 ℃" });
                UpdatePanel_Shelter(this.shelterPane);

            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 15);
            dispatcherTimer.Start();

        }


        private void UpdatePanel_HPIC(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Hpic);
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
                    //this.CheckAlarm(panel, DBDataProvider.DeviceKey_Hpic, Doserate, 0, v); 
                    string doserateMsg = v + "nGy/h";
                    // this.DisplayPanelData(panel, doserateMsg);
                    panel.SetData(new string[] { doserateMsg });
                }
            }
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
                //this.CheckAlarm(panel, DBDataProvider.DeviceKey_NaI, Doserate, 0, v);
            }

            for (int k = 0; k < 3; ++k)
            {
                nuclideMsgs[k] = nuclideMsgs[k].TrimEnd(' ', ',');
            }
            panel.SetData(new string[] { doserateMsg });
            // this.DisplayPanelData(panel, doserateMsg, nuclideMsgs[0], nuclideMsgs[1], nuclideMsgs[2]);
        }
        // 3 // 风速、风向、雨量
        private void UpdatePanel_Weather(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Weather);
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

            //this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "windspeed", 0, windspeed);
            //this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "direction", 1, direction);
            //this.CheckAlarm(panel, DBDataProvider.DeviceKey_Weather, "rainspeed", 2, rainspeed);

            //this.DisplayPanelData(panel, windspeedMsg, directionMsg, rainspeedMsg);
   
        }
        // 4 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_HV(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_HvSampler);
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

            //this.DisplayPanelData(panel, statusMsg, volumeMsg, hoursMsg, flowMsg);

        }
        // 5 采样状态（可用颜色表示）、累计采样体积（重要）、累计采样时间、瞬时采样流量、三种故障报警
        private void UpdatePanel_I(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_ISampler);
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

            //this.DisplayPanelData(panel, statusMsg, volumeMsg, hoursMsg, flowMsg);
        }
        // 6 市电状态、备电时间、舱内温度、门禁报警、烟感报警、浸水报警
        private void UpdatePanel_Shelter(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Shelter);
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

            string mainPowMsg = string.Format("{0}", mainPowerWay);
            string batteryHoursMsg = string.Format("{0}h", batteryHours);
            string tempMsg = string.Format("{0}℃", temperature);

            panel.SetData(new string[] { mainPowMsg, batteryHoursMsg, tempMsg });

        }
        // 7 仅工作状态
        private void UpdatePanel_DWD(SmartDataPane panel)
        {
            var d = this.dataProvider.GetLatestEntry(DataProvider.DeviceKey_Dwd);
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
            panel.SetData(new string[] { LidOpenMsg });
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
    }
}

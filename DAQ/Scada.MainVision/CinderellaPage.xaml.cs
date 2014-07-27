using MySql.Data.MySqlClient;
using Scada.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for CinderellaPage.xaml
    /// </summary>
    public partial class CinderellaPage : UserControl
    {
            
        private DBDataProvider dataProvider;

        private MySqlConnection dbConn = null;

        public CinderellaPage()
        {
            InitializeComponent();
        }


        public void SetDataProvider(DBDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.cinderellaPane.Initialize(new string[] { "时间", "条形码", "开始时间", "工作时间", "瞬时流量", "累计流量", "大气压", "温度" });
            this.hpgePane.Initialize(new string[] { "上次测量时间", "生成文件" });
            /*
            this.statusPane.Initialize(new string[] { 
                "当前状态", "工作模式", "循环时间", 
                "前玻璃门状态", "后玻璃门状态", "新滤纸夹仓状态", "旧滤纸夹仓状态", "切割位置状态", 
                "新滤纸夹具方向", "新率纸盒状态", "抽屉状态", "采样流量状态", "新滤纸夹仓门状态", "旧滤纸夹仓门状态", "紧急停止状态"});*/

            this.statusPane.Initialize(new string[] { 
                "工作模式", "循环模式", "当前流程", "报警:", "", "", "", ""});
            this.dbConn = this.dataProvider.GetMySqlConnection();

            MySqlCommand cmd = this.dbConn.CreateCommand();
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, evt) =>
            {
                this.RefreshTick(cmd);
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 15);
            dispatcherTimer.Start();
            this.RefreshTick(cmd);
        }

        private void RefreshTick(MySqlCommand cmd)
        {
            var data = this.dataProvider.RefreshTimeNow("scada.cinderella.data", cmd);
            UpdateCinderellaPanel(this.cinderellaPane, data);
        }

        private void UpdateCinderellaStatusPanel(SmartDataPane panel, Dictionary<string, object> d)
        {
            if (d == null)
            {
                return;
            }
            panel.SetData(
                Get(d, "mode", ""),
                Get(d, "loop", ""),
                Get(d, "step", ""),
                Get(d, "0", ""),
                Get(d, "1", ""),
                Get(d, "2", ""),
                Get(d, "3", ""),
                Get(d, "4", ""));
        }

        private void UpdateCinderellaPanel(SmartDataPane panel, Dictionary<string, object> d)
        {
            if (d == null)
            {
                return;
            }

            panel.SetData(
                Get(d, "time", ""),
                Get(d, "barcode", ""),
                Get(d, "begintime", ""),
                Get(d, "worktime", ""),
                Get(d, "flow", "m³"),
                Get(d, "flowperhour", "m³/h"),
                Get(d, "pressure", "kPa"),
                Get(d, "temperature", "℃"));
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
            return this.GetDisplayString(d, key.ToLower()) + " " + s;
        }

        internal void OnReceivedCommand(Common.Command cmd)
        {
            string c = cmd.Content.Trim('"');
            string[] p = c.Split(';');
            string str = Convert.ToString(int.Parse(p[3]), 2);

            if (str.Length > 24)
            {
                return;
            }

            int index = 24 - str.Length;
            for (int i = 0; i < index; i++)
            {
                str = "0" + str;
            }

            int datalen = str.Length;
            string[] data = new string[datalen];
            for (int i = 0; i < datalen; i++)
            {
                data[i] = str.Substring(datalen - 1 - i, 1);
            }
            
            SearchBitStatus(data, p);
        }


        private enum Mode24_Process
        {
            Mode24_Process_SampleMeasure = 0,	                //初始状态/样品测量
            Mode24_Process_ChangeStart = 1,	                    //机械臂开始移动
            Mode24_Process_MovingPlate = 2,	                    //开始拖动滤纸夹
            Mode24_Process_Cutting = 3,	                        //滤纸夹就位，开始切割
            Mode24_Process_MovingLead_AfterCutting = 4,	        //切割完毕，铅室盖打开中
            Mode24_Process_LeadOpen_AfterCutting = 5,		    //完全打开铅室盖
            Mode24_Process_StartQA = 6,		                    //开始QA测量
            Mode24_Process_QAFinish = 7,		                //QA测量结束
            Mode24_Process_MovingLead_AfterQAFinish = 8,		//QA测量完毕，铅室盖关闭中
            Mode24_Process_LeadClose = 9		                //QA测量完毕，铅室盖完全关闭
        }

        //状态、报警
        private bool Status_BackPlexDoor = false;	    //后玻璃门状态
        private bool Status_Flow = false;	            //采样流量状态
        private bool Status_NewCatridge = false;	    //新滤纸夹舱状态
        private bool Status_CuttingPosition = false;	//切割位置状态
        private bool Status_OldCatridge = false;		//旧滤纸夹舱状态
        private bool Status_CatridgeDirection = false;	//新滤纸夹具方向
        private bool Status_BeakerHolder = false;		//抽屉状态
        private bool Status_FrontPlexDoor = false;	    //前玻璃门状态
        private bool Status_Beaker = false;			    //新滤纸盒状态
        private bool Status_OldCatridgeDoor = false;	//旧滤纸夹舱门状态
        private bool Status_NewCatridgeDoor = false;	//新滤纸夹舱门状态
        private bool Status_Emergency = false;	        //紧急停止状态
        

        private Dictionary<string, object> statusDict = new Dictionary<string, object>(10);

        private int statusKey = 0;

        private void UpdateStatus(string msg)
        {
            this.statusDict.Add(statusKey.ToString(), msg);
            this.statusKey ++;
        }

        private bool SearchBitStatus(string[] data, string [] p)
        {
            statusDict.Clear();
            statusKey = 0;
            
            // 触发后玻璃门开
            if (data[22] == "0")
            {
                Status_BackPlexDoor = true;
                UpdateStatus("后侧玻璃门打开");
            }
            //触发后玻璃门关
            if (data[22] == "1")
            {
                Status_BackPlexDoor = false;
                // UpdateStatus("后侧玻璃门关闭");
            }

            //触发流量低报警
            if (data[21] == "1")
            {
                Status_Flow = true;
                UpdateStatus("流量低");
            }
            if (data[21] == "0")
            {
                Status_Flow = false;
                //UpdateStatus("流量正常");
            }

            //新滤纸夹舱空报警
            if (data[19] == "1")
            {
                Status_NewCatridge = true;
                UpdateStatus("新滤纸夹舱空");
            }
            if (data[19] == "0")
            {
                Status_NewCatridge = false;
                //UpdateStatus("新滤纸夹舱正常");
            }

            //切割位置未找到滤纸夹具
            if (data[18] == "1")
            {
                Status_CuttingPosition = true;
                UpdateStatus("切割位置未找到滤纸夹具");
            }
            if (data[18] == "0")
            {
                Status_CuttingPosition = false;
                //UpdateStatus("切割位置正常");
            }

            //旧滤纸夹舱满报警
            if (data[17] == "1")
            {
                Status_OldCatridge = true;
                UpdateStatus("旧滤纸夹舱满报警");
            }
            if (data[17] == "0")
            {
                Status_OldCatridge = false;
                //UpdateStatus("旧滤纸夹舱正常");
            }

            //滤纸方向放反了
            if (data[10] == "1")
            {
                Status_CatridgeDirection = true;
                UpdateStatus("滤纸夹方向错误");
            }
            if (data[10] == "0")
            {
                Status_CatridgeDirection = false;
                //UpdateStatus("滤纸夹方向正常");
            }

            //抽屉空了
            if (data[9] == "0")
            {
                Status_BeakerHolder = true;
                UpdateStatus("抽屉被抽出");
            }
            if (data[9] == "1")
            {
                Status_BeakerHolder = false;
                //UpdateStatus("抽屉正常");
            }

            //前玻璃门打开
            if (data[8] == "0")
            {
                Status_FrontPlexDoor = true;
                UpdateStatus("前侧玻璃门打开");
            }
            if (data[8] == "1")
            {
                Status_FrontPlexDoor = false;
                //UpdateStatus("前侧玻璃门关闭");
            }

            //新滤纸盒空了
            if (data[6] == "1")
            {
                Status_Beaker = true;
                UpdateStatus("新滤纸盒位置空");
            }
            if (data[6] == "0")
            {
                Status_Beaker = false;
                //UpdateStatus("新滤纸盒位置正常");
            }

            //旧滤纸夹具舱门打开
            if (data[5] == "0")
            {
                Status_OldCatridgeDoor = true;
                UpdateStatus("旧滤纸夹具舱门打开");
            }
            if (data[5] == "1")
            {
                Status_OldCatridgeDoor = false;
                //UpdateStatus("旧滤纸夹具舱门关闭");
            }

            //新滤纸夹具舱门打开
            if (data[4] == "0")
            {
                Status_NewCatridgeDoor = true;
                UpdateStatus("新滤纸夹具舱门打开");
            }
            if (data[4] == "1")
            {
                Status_NewCatridgeDoor = false;
                //UpdateStatus("新滤纸夹具舱门关闭");
            }

            //应急报警
            if (data[0] == "1")
            {
                Status_Emergency = true;
                UpdateStatus("紧急开关报警");
            }
            if (data[0] == "0")
            {
                Status_Emergency = false;
                //UpdateStatus("紧急开关正常");
            }

            if (p[0] == "0")
            {
                this.statusDict.Add("mode", "自动模式");
            }
            else if (p[0] == "1")
            {
                this.statusDict.Add("mode", "手动模式");
            }       
              
            if (p[1] == "0")
            {
                this.statusDict.Add("loop", "24小时模式");
            }
            else if (p[1] == "1")
            {
                this.statusDict.Add("loop", "8小时模式");
            }
            else if (p[1] == "2")
            {
                this.statusDict.Add("loop", "6小时模式");
            }
            else if (p[1] == "3")
            {
                this.statusDict.Add("loop", "1小时模式");
            }

            if (p[2] == "0")
            {
                this.statusDict.Add("step", "初始状态/样品测量");
            }
            else if (p[2] == "1")
            {
                this.statusDict.Add("step", "机械臂开始移动");
            }
            else if (p[2] == "2")
            {
                this.statusDict.Add("step", "开始拖动滤纸夹");
            }
            else if (p[2] == "3")
            {
                this.statusDict.Add("step", "滤纸夹就位，开始切割");
            }
            else if (p[2] == "4")
            {
                this.statusDict.Add("step", "切割完毕，铅室盖打开中");
            }
            else if (p[2] == "5")
            {
                this.statusDict.Add("step", "完全打开铅室盖");
            }
            else if (p[2] == "6")
            {
                this.statusDict.Add("step", "开始QA测量");
            }
            else if (p[2] == "7")
            {
                this.statusDict.Add("step", "QA测量结束");
            }
            else if (p[2] == "8")
            {
                this.statusDict.Add("step", "QA测量完毕，铅室盖关闭中");
            }
            else if (p[2] == "9")
            {
                this.statusDict.Add("step", "QA测量完毕，铅室盖完全关闭");
            }


            this.UpdateCinderellaStatusPanel(this.statusPane, this.statusDict);
            return true;
        }

        private void AutoClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;

            if (b.IsChecked.Value)
            {
                Command.Send(Ports.Main, new Command("mv", "main", "cinderella", "MANUAL"));
            }
            else
            {
                Command.Send(Ports.Main, new Command("mv", "main", "cinderella", "AUTO"));
            }
        }

        private void LoopClick(object sender, RoutedEventArgs e)
        {
            // TODO: Delay 15 min user can push it again.
            Command.Send(Ports.Main, new Command("mv", "main", "cinderella", "STARTCHANGE"));
        }

        private void PumpClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            if (b.IsChecked.Value)
            {
                Command.Send(Ports.Main, new Command("mv", "main", "cinderella", "STARTPUMP"));
            }
            else
            {
                Command.Send(Ports.Main, new Command("mv", "main", "cinderella", "STOPPUMP"));
            }
        }

        private void hourClick(object sender, RoutedEventArgs e)
        {
            this.h1H.IsChecked = false;
            this.h1H.Foreground = Brushes.Gray;
            this.h6H.IsChecked = false;
            this.h6H.Foreground = Brushes.Gray;
            this.h8H.IsChecked = false;
            this.h8H.Foreground = Brushes.Gray;
            this.h24H.IsChecked = false;
            this.h24H.Foreground = Brushes.Gray;

            ToggleButton b = (ToggleButton)sender;
            b.IsChecked = true;
            b.Foreground = Brushes.Green;

            var command = b.Name.Substring(1);

            Command.Send(Ports.Main, new Command("mv", "main", "cinderella.hour", command));
        }

        private Dictionary<string, object> hpgeData = new Dictionary<string, object>();

        internal void OnFileCreated(string filePath)
        {
            hpgeData.Clear();
            hpgeData.Add("time", new FileInfo(filePath).CreationTime.ToString());
            hpgeData.Add("file", System.IO.Path.GetFileName(filePath));
            this.UpdateHpgeStatusPanel(this.hpgePane, this.hpgeData);
        }

        private void UpdateHpgeStatusPanel(SmartDataPane panel, Dictionary<string, object> d)
        {
            if (d == null)
            {
                return;
            }

            panel.SetData(Get(d, "time", ""), Get(d, "file", ""));
        }
    }
}

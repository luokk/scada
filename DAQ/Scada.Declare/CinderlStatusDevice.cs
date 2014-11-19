using Scada.Common;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Scada.Declare
{
    public class CinderlStatusDevice : StandardDevice
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public CinderlStatusDevice(DeviceEntry entry)
            :base(entry)
        {

        }

        private int lastStatus = 0;

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

        // to MV
        // WorkMode =0, AUTO, WorkMode=1, MANUAL
        private int WorkMode = -1;

        // LoopMode = 0, 24h, 1, 8h, 2, 6h, 3, 1h
        private int LoopMode = -1;

        private int Running_Process = -1;

        private int counter = 0;

        public override bool OnReceiveData(byte[] line)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in line)
            {
                if (b >= 0x30 && b <= 0x39)
                {
                    sb.Append((char)b);
                }
            }

            string record = sb.ToString();

            // Cinderella status标准输出是10，不等于10时，不做处理      by Kaikai
            if (line.Length > 10 || line.Length < 9)
            {
                RecordManager.DoSystemEventRecord(this, record + "," + line.Length.ToString());
                return false;
            }

            
            int status = 0;
            if (int.TryParse(record, out status))
            {
                if (counter % 5 == 0)
                {
                    counter = 0;
                    string c = WorkMode.ToString() + ";" + LoopMode.ToString() + ";" + Running_Process.ToString() + ";" + status.ToString();
                    this.UpdateStatusToDataCenter(c);
                    Command.Send(Ports.MainVision, new Command("m", "mv", "cinderella.status", c));
                    RecordManager.DoSystemEventRecord(this, c);
                }
                counter++;


                bool stateChanged = (this.lastStatus != status);
                //important 状态判断！ by Kaikai
                if (!stateChanged)
                {
                    return false;  
                }

                this.lastStatus = status;

                this.CheckStatus(status);

                string statusLine = string.Format("STATUS:{0}", status);
                
                RecordManager.DoSystemEventRecord(this, statusLine);
                return true;
            }
            else
            {
                return false;
            }

        }

        private void UpdateStatusToDataCenter(string status)
        {
            using (WebClient wc = new WebClient())
            {
                byte[] data = Encoding.UTF8.GetBytes(status);
                string uri = this.GetUpdateStatusUri();
                wc.UploadDataCompleted += (object o, UploadDataCompletedEventArgs e) => 
                {
                    if (e.Error == null)
                    {
                        string result = Encoding.UTF8.GetString(e.Result);
                    }
                };
                wc.UploadDataAsync(new Uri(uri), "POST", data, null);
            }
        }

        private string GetAttribute(XmlNode node, string attr)
        {
            try
            {
                var xmlAttr = node.Attributes.GetNamedItem(attr);
                return xmlAttr.Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string GetUpdateStatusUri()
        {
            if (string.IsNullOrEmpty(this.DataCenterBaseUrl))
            {
                const string AgentXml = "agent.http.settings";
                string settingFileName = ConfigPath.GetConfigFilePath(AgentXml);
                XmlDocument doc = new XmlDocument();
                if (File.Exists(settingFileName))
                {
                    doc.Load(settingFileName);
                }

                var datacenters = doc.SelectNodes("//datacenter2");
                foreach (XmlNode dcn in datacenters)
                {
                    this.DataCenterBaseUrl = this.GetAttribute(dcn, "BaseUrl");
                    break;
                }

                var siteNode = doc.SelectNodes("//site")[0];
                this.StationId = this.GetAttribute(siteNode, "station");
            }
            return string.Format("{0}/command/cinderella/{1}", this.DataCenterBaseUrl, this.StationId);
        }

        private bool CheckStatus(int status)
        {
            // 转成2进制的string
            string str = Convert.ToString(status, 2);

            if (str.Length > 24)
            {
                return false;
            }

            int index = 24 - str.Length;
            for (int i = 0; i < index; i++)
            {
                str = "0" + str;
            }

            //取相反的序列，方便数位数
            int datalen = str.Length;
            string[] data = new string[datalen];
            for (int i = 0; i < datalen; i++)
            {
                data[i] = str.Substring(datalen - 1 - i, 1);
            }

            // 判断状态、报警
            SearchBitStatus(data);
            
            // 自动模式
            if (data[15] == "0")
            {
                // AUTO
                WorkMode = 0;
                return AutoMode(data);
            }
            else
            {
                // MANUAL
                WorkMode = 1;
                return ManualMode(data);
            }
        }

        private bool SearchBitStatus(string[] data)
        {
            // 触发后玻璃门开
            if (data[22] == "0" && Status_BackPlexDoor == false)
            {
                Status_BackPlexDoor = true;
                RecordManager.DoSystemEventRecord(this, "后侧玻璃门打开", RecordType.Event);
            }
            //触发后玻璃门关
            if (data[22] == "1" && Status_BackPlexDoor == true)
            {
                Status_BackPlexDoor = false;
                RecordManager.DoSystemEventRecord(this, "后侧玻璃门关闭", RecordType.Event);
            }

            //触发流量低报警
            if (data[21] == "1" && Status_Flow == false)
            {
                Status_Flow = true;
                RecordManager.DoSystemEventRecord(this, "流量低", RecordType.Event);
            }
            if (data[21] == "0" && Status_Flow == true)
            {
                Status_Flow = false;
                RecordManager.DoSystemEventRecord(this, "流量正常", RecordType.Event);
            }

            //新滤纸夹舱空报警
            if (data[19] == "1" && Status_NewCatridge == false)
            {
                Status_NewCatridge = true;
                RecordManager.DoSystemEventRecord(this, "新滤纸夹舱空", RecordType.Event);
            }
            if (data[19] == "0" && Status_NewCatridge == true)
            {
                Status_NewCatridge = false;
                RecordManager.DoSystemEventRecord(this, "新滤纸夹舱正常", RecordType.Event);
            }

            //切割位置未找到滤纸夹具
            if (data[18] == "1" && Status_CuttingPosition == false)
            {
                Status_CuttingPosition = true;
                RecordManager.DoSystemEventRecord(this, "切割位置未找到滤纸夹具", RecordType.Event);
            }
            if (data[18] == "0" && Status_CuttingPosition == true)
            {
                Status_CuttingPosition = false;
                RecordManager.DoSystemEventRecord(this, "切割位置正常", RecordType.Event);
            }

            //旧滤纸夹舱满报警
            if (data[17] == "1" && Status_OldCatridge == false)
            {
                Status_OldCatridge = true;
                RecordManager.DoSystemEventRecord(this, "旧滤纸夹舱满报警", RecordType.Event);
            }
            if (data[17] == "0" && Status_OldCatridge == true)
            {
                Status_OldCatridge = false;
                RecordManager.DoSystemEventRecord(this, "旧滤纸夹舱正常", RecordType.Event);
            }

            //滤纸方向放反了
            if (data[10] == "1" && Status_CatridgeDirection == false)
            {
                Status_CatridgeDirection = true;
                RecordManager.DoSystemEventRecord(this, "滤纸夹方向错误", RecordType.Event);
            }
            if (data[10] == "0" && Status_CatridgeDirection == true)
            {
                Status_CatridgeDirection = false;
                RecordManager.DoSystemEventRecord(this, "滤纸夹方向正常", RecordType.Event);
            }

            //抽屉空了
            if (data[9] == "0" && Status_BeakerHolder == false)
            {
                Status_BeakerHolder = true;
                RecordManager.DoSystemEventRecord(this, "抽屉被抽出", RecordType.Event);
            }
            if (data[9] == "1" && Status_BeakerHolder == true)
            {
                Status_BeakerHolder = false;
                RecordManager.DoSystemEventRecord(this, "抽屉正常", RecordType.Event);
            }

            //前玻璃门打开
            if (data[8] == "0" && Status_FrontPlexDoor == false)
            {
                Status_FrontPlexDoor = true;
                RecordManager.DoSystemEventRecord(this, "前侧玻璃门打开", RecordType.Event);
            }
            if (data[8] == "1" && Status_FrontPlexDoor == true)
            {
                Status_FrontPlexDoor = false;
                RecordManager.DoSystemEventRecord(this, "前侧玻璃门关闭", RecordType.Event);
            }

            //新滤纸盒空了
            if (data[6] == "1" && Status_Beaker == false)
            {
                Status_Beaker = true;
                RecordManager.DoSystemEventRecord(this, "新滤纸盒位置空", RecordType.Event);
            }
            if (data[6] == "0" && Status_Beaker == true)
            {
                Status_Beaker = false;
                RecordManager.DoSystemEventRecord(this, "新滤纸盒位置正常", RecordType.Event);
            }

            //旧滤纸夹具舱门打开
            if (data[5] == "0" && Status_OldCatridgeDoor == false)
            {
                Status_OldCatridgeDoor = true;
                RecordManager.DoSystemEventRecord(this, "旧滤纸夹具舱门打开", RecordType.Event);
            }
            if (data[5] == "1" && Status_OldCatridgeDoor == true)
            {
                Status_OldCatridgeDoor = false;
                RecordManager.DoSystemEventRecord(this, "旧滤纸夹具舱门关闭", RecordType.Event);
            }

            //新滤纸夹具舱门打开
            if (data[4] == "0" && Status_NewCatridgeDoor == false)
            {
                Status_NewCatridgeDoor = true;
                RecordManager.DoSystemEventRecord(this, "新滤纸夹具舱门打开", RecordType.Event);
            }
            if (data[4] == "1" && Status_NewCatridgeDoor == true)
            {
                Status_NewCatridgeDoor = false;
                RecordManager.DoSystemEventRecord(this, "新滤纸夹具舱门关闭", RecordType.Event);
            }

            //应急报警
            if (data[0] == "1" && Status_Emergency == false)
            {
                Status_Emergency = true;
                RecordManager.DoSystemEventRecord(this, "紧急开关报警", RecordType.Event);
            }
            if (data[0] == "0" && Status_Emergency == true)
            {
                Status_Emergency = false;
                RecordManager.DoSystemEventRecord(this, "紧急开关正常", RecordType.Event);
            }

            return true;
        }

        private bool AutoMode(string[] data)
        {
            //RecordManager.DoSystemEventRecord(this, "进入自动模式", RecordType.Event);
            
            // 24小时模式
            if (data[14] == "1" && data[13] == "0" && data[12] == "0" && data[11] == "0")
            {
                LoopMode = 0;
                return Mode24(data);
            }

            // 8小时模式
            if (data[14] == "0" && data[13] == "1" && data[12] == "0" && data[11] == "0")
            {
                LoopMode = 1;
                return Mode8(data);
            }

            // 6小时模式
            if (data[14] == "0" && data[13] == "0" && data[12] == "1" && data[11] == "0")
            {
                LoopMode = 2;
                return Mode6(data);
            }

            // 1小时模式
            if (data[14] == "0" && data[13] == "0" && data[12] == "0" && data[11] == "1")
            {
                LoopMode = 3;
                return Mode1(data);
            }
                
            else
            {
                RecordManager.DoSystemEventRecord(this, "Unknown mode");
                return false;
            }
        }

        private bool ManualMode(string[] data)
        {
            RecordManager.DoSystemEventRecord(this, "进入手动模式", RecordType.Event);

            // to do        by Kaikai

            return true;
        }

        private bool Mode8(string[] data)
        {
            RecordManager.DoSystemEventRecord(this, "进入8小时模式", RecordType.Event);
            return true;
        }

        private bool Mode6(string[] data)
        {
            RecordManager.DoSystemEventRecord(this, "进入6小时模式", RecordType.Event);
            return true;
        }

        private bool Mode1(string[] data)
        {
            RecordManager.DoSystemEventRecord(this, "进入1小时模式", RecordType.Event);
            return true;
        }

        private bool Mode24(string[] data)
        {
            //RecordManager.DoSystemEventRecord(this, "进入24小时模式", RecordType.Event);

            // 状态 = 00110
            if (data[23] == "0" && data[7] == "0" && data[3] == "1" && data[2] == "1" && data[1] == "0")
            {
                Running_Process = (int)Mode24_Process.Mode24_Process_SampleMeasure;

                RecordManager.DoSystemEventRecord(this, "初始状态/样品测量", RecordType.Event);
                return true;
            }

            // 状态 = 10110
            if (data[23] == "1" && data[7] == "0" && data[3] == "1" && data[2] == "1" && data[1] == "0")
            {
                // 判断是否原有状态之一，避免状态重复出现
                if (Running_Process == (int)Mode24_Process.Mode24_Process_ChangeStart ||
                    Running_Process == (int)Mode24_Process.Mode24_Process_Cutting ||
                    Running_Process == (int)Mode24_Process.Mode24_Process_LeadClose)
                {
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_SampleMeasure)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_ChangeStart;

                    RecordManager.DoSystemEventRecord(this, "机械臂开始移动", RecordType.Event);
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_MovingPlate)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_Cutting;

                    RecordManager.DoSystemEventRecord(this, "滤纸夹就位，开始切割", RecordType.Event);
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_MovingLead_AfterQAFinish)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_LeadClose;

                    RecordManager.DoSystemEventRecord(this, "铅室盖完全关闭，准备开始样品测量", RecordType.Event);

                    // to do sample measurement
                    ExecSample24HourMeasure();

                    return true;
                }

                else
                {
                    RecordManager.DoSystemEventRecord(this, "未知状态，状态号10110", RecordType.Event);
                    return false;
                }

            }

            // 状态 = 10010
            if (data[23] == "1" && data[7] == "0" && data[3] == "0" && data[2] == "1" && data[1] == "0")
            {
                // 判断是否原有状态之一，避免状态重复出现
                if (Running_Process == (int)Mode24_Process.Mode24_Process_MovingPlate)
                {
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_ChangeStart)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_MovingPlate;

                    RecordManager.DoSystemEventRecord(this, "开始移动滤纸夹", RecordType.Event);
                    return true;
                }

                else
                {
                    RecordManager.DoSystemEventRecord(this, "未知状态，状态号10010", RecordType.Event);
                    return false;
                }
            }

            // 状态 = 10100
            if (data[23] == "1" && data[7] == "0" && data[3] == "1" && data[2] == "0" && data[1] == "0")
            {
                // 判断是否原有状态之一，避免状态重复出现
                if (Running_Process == (int)Mode24_Process.Mode24_Process_MovingLead_AfterCutting ||
                    Running_Process == (int)Mode24_Process.Mode24_Process_MovingLead_AfterQAFinish)
                {
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_Cutting)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_MovingLead_AfterCutting;

                    RecordManager.DoSystemEventRecord(this, "切割完毕，打开铅室盖中", RecordType.Event);
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_QAFinish)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_MovingLead_AfterQAFinish;

                    RecordManager.DoSystemEventRecord(this, "QA测量结束，关闭铅室盖中", RecordType.Event);
                    return true;
                }

                else
                {
                    RecordManager.DoSystemEventRecord(this, "未知状态，状态号10100", RecordType.Event);
                    return false;
                }
            }

            // 状态 = 10101
            if (data[23] == "1" && data[7] == "0" && data[3] == "1" && data[2] == "0" && data[1] == "1")
            {
                // 判断是否原有状态之一，避免状态重复出现
                if (Running_Process == (int)Mode24_Process.Mode24_Process_LeadOpen_AfterCutting ||
                    Running_Process == (int)Mode24_Process.Mode24_Process_QAFinish)
                {
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_MovingLead_AfterCutting)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_LeadOpen_AfterCutting;

                    RecordManager.DoSystemEventRecord(this, "切割结束，铅室盖打开完毕", RecordType.Event);
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_StartQA)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_QAFinish;

                    RecordManager.DoSystemEventRecord(this, "QA测量结束", RecordType.Event);
                    return true;
                }

                else
                {
                    RecordManager.DoSystemEventRecord(this, "未知状态，状态号10101", RecordType.Event);
                    return false;
                }
            }

            // 状态 = 11101
            if (data[23] == "1" && data[7] == "1" && data[3] == "1" && data[2] == "0" && data[1] == "1")
            {
                // 判断是否原有状态之一，避免状态重复出现
                if (Running_Process == (int)Mode24_Process.Mode24_Process_StartQA)
                {
                    return true;
                }

                if (Running_Process == (int)Mode24_Process.Mode24_Process_LeadOpen_AfterCutting)
                {
                    Running_Process = (int)Mode24_Process.Mode24_Process_StartQA;

                    RecordManager.DoSystemEventRecord(this, "开始QA测量", RecordType.Event);

                    // to do QA Measurement
                    // ExecQAMeasure();
                    return true;
                }

                else
                {
                    RecordManager.DoSystemEventRecord(this, "未知状态，状态号11101", RecordType.Event);
                    return false;
                }
            }

            else
            {
                RecordManager.DoSystemEventRecord(this, "未知状态，24小时模式", RecordType.Event);
                return false;
            }
        }

        private void ExecQAMeasure()
        {
            var bat = ConfigPath.GetConfigFilePath("devices/Scada.HPGE/0.9/script/QAMeasure.bat");
            using (Process p = Process.Start(bat))
            {
            }
        }

        private void ExecSample24HourMeasure()
        {
            // 首先清除原有进程
            try
            {
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    if (process.ProcessName.ToLower() == "gv32")
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception e1)
            {
                string msg = string.Format("Kill GammaVision进程失败, 错误：{0}",e1.Message);
                RecordManager.DoSystemEventRecord(this, msg, RecordType.Event);

                return;
            }

            // 启动新的JOB文件
            var bat = ConfigPath.GetConfigFilePath("devices/Scada.HPGE/0.9/script/SampleMeasure24.bat");
            using (Process p = Process.Start(bat))
            {
            }
        }

        public override void Send(byte[] action, DateTime time)
        {
            this.Write(action);
        }

        public string DataCenterBaseUrl { get; set; }

        public string StationId { get; set; }
    }
}

using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Scada.Declare
{
	public class FormProxyDevice : Device
	{
        public const string ProcessName = "ProcessName";

		[DllImport("user32.dll")]
		static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

		[DllImport("user32.dll")]
		public extern static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, int wParam, StringBuilder lParam, int flags, int timeout, out IntPtr pdwResult);

        [DllImport("kernel32.dll")]
        public extern static int GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private string processName;

        private IntPtr hWnd = IntPtr.Zero;

        private Thread thread = null;

        private Timer timer = null;

        private const int WM_GETTEXT = 0x000D;

        private const int WM_COMMAND = 0x0111;

        private const int BN_CLICKED = 0;

        private const int BufferLength = 1024;

        private const int MaxDelay = 10;

        StringBuilder sb = new StringBuilder(BufferLength);

        private readonly string[] EmptyStringArray = new string[0];

        private string insertIntoCommand;

        private FieldConfig[] fieldsConfig = null;

        private List<int> elemIdList = new List<int>();

        private DateTime lastDateTime = default(DateTime);

        private string beginTime;
        private string endTime;

        private bool lastStartState;

        private double factor1;

        private string tableName;

        private int currentSid = 1;


		public FormProxyDevice(DeviceEntry entry)
		{

			this.Initialize(entry);
		}

		~FormProxyDevice()
        {
        }

        private static int MakeLong(int l, int h)
        {
            // ((LONG)(((WORD)(a & 0xffff)) | ((DWORD)((WORD)(b & 0xffff))) << 16))
            return (int)(short)(l & 0xffff) | ((int)(short)(h & 0xffff)) << 16;
        }

        // TODO: Check...
        public static void PressConnectToCPU(string processName, string devicePath)
        {
            IntPtr hWnd = FetchWindowHandle(processName, devicePath);

            const int buttonId = 0x3C;

            IntPtr btnHwnd = GetDlgItem(hWnd, buttonId);
            int l = MakeLong(buttonId, BN_CLICKED);
            PostMessage(hWnd, WM_COMMAND, l, (int)btnHwnd);
        }

		private bool Initialize(DeviceEntry entry)
		{
            this.Name = entry[DeviceEntry.Name].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
            this.Version = entry[DeviceEntry.Version].ToString();

            this.processName = (StringValue)entry[ProcessName];

            for (int i = 1; i < 50; i++)
            {
                string elemId = string.Format("ElemId{0}", i);
                if (!entry.Contains(elemId))
                {
                    break;
                }
                string controlIdStr = (StringValue)entry[elemId];
                int controlId = 0;
                if (controlIdStr.StartsWith("0x"))
                {
                    controlIdStr = controlIdStr.Substring(2);
                    controlId = int.Parse(controlIdStr, NumberStyles.AllowHexSpecifier);
                    this.elemIdList.Add(controlId);
                }
                else
                {
                    if (int.TryParse(controlIdStr, out controlId))
                    {
                        this.elemIdList.Add(controlId);
                    }
                }
            }

            string tableName = (StringValue)entry[DeviceEntry.TableName];
            string tableFields = (StringValue)entry[DeviceEntry.TableFields];


            if (!Device.GetFactor(entry, 1, out this.factor1))
            {
                // Debug.Assert(false);
            }

            string[] fields = tableFields.Split(',');
            string atList = string.Empty;
            for (int i = 0; i < fields.Length; ++i)
            {
                string at = string.Format("@{0}, ", i + 1);
                atList += at;
            }
            atList = atList.TrimEnd(',', ' ');

            this.tableName = tableName;
            string cmd = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
            this.insertIntoCommand = cmd;


            string fieldsConfigStr = (StringValue)entry[DeviceEntry.FieldsConfig];
            List<FieldConfig> fieldConfigList = ParseDataFieldConfig(fieldsConfigStr);
            this.fieldsConfig = fieldConfigList.ToArray<FieldConfig>();

            this.GenCurrentSid();
			return true;
		}

        private void GenCurrentSid()
        {
            DBConnectionForSid c = new DBConnectionForSid();
            int sid = c.GetCurrentSid(this.tableName);
            this.currentSid = sid + 1;
        }

        private string GetText(IntPtr hWnd, int nCtrlId)
        {
            sb.Length = 0;
            IntPtr edit = GetDlgItem(hWnd, nCtrlId);
            if (edit == IntPtr.Zero)
            {
                string wrongEditId = string.Format("Wrong Edit Id: {0}", nCtrlId);
                RecordManager.DoSystemEventRecord(this, wrongEditId);
            }
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(edit, WM_GETTEXT, BufferLength, sb, 0, 1000, out result);
            return sb.ToString();
        }

        private static IntPtr FetchWindowHandle(string processName, string path)
        {
            string hWndCfgFile = path + "\\HWND.r";

            string exePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string formProxyPath = System.IO.Path.Combine(exePath, "Scada.FormProxy.exe");
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = false;    //设定不显示窗口
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = formProxyPath; //设定程序名  
                process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
                process.StartInfo.RedirectStandardError = true;//重定向错误输出
                
                string args = string.Format("{0} {1}", processName, hWndCfgFile);
                process.StartInfo.Arguments = args;
                process.Start();
            }

            Thread.Sleep(678);

            byte[] bytes = new byte[32];
            
            FileStream fs = File.Open(hWndCfgFile, FileMode.Open);
            int r = fs.Read(bytes, 0, 32);
            fs.Close();

            string line = Encoding.ASCII.GetString(bytes, 0, r);

            return (IntPtr)int.Parse(line);
        }

        private List<string> GetData(IntPtr hWnd)
        {
            List<string> ret = new List<string>();
            if (hWnd != IntPtr.Zero)
            {
                foreach (int elemId in this.elemIdList)
                {
                    string s = GetText(hWnd, elemId);
                    if (s.Contains('.'))
                    {
                        double v;
                        if (double.TryParse(s, out v))
                        {
                            s = v.ToString();
                        }
                    }
                    ret.Add(s);
                }
            }
            return ret;
        }

        private bool GetDeviceData(string[] data, DateTime time, out DeviceData deviceData)
        {
            deviceData = default(DeviceData);
            if (data == null || data.Length == 0)
            {
                return false;
            }
            deviceData.Time = time;
            double value = 0.0;
            if (double.TryParse(data[1], out value))
            {
                data[1] = string.Format("{0:f1}", value * this.factor1);
            }
            else
            {
                data[1] = string.Format("{0:f1}", this.factor1);
            }
            
            object[] fields = Device.GetFieldsData(data, time, this.fieldsConfig);
            deviceData = new DeviceData(this, fields);
            deviceData.InsertIntoCommand = this.insertIntoCommand;
            //deviceData.FieldsConfig = this.fieldsConfig;
            return true;
        }

		public override void Start(string address)
		{
            this.thread = new Thread(new ThreadStart(() => {

                // Fetch Window Handle from HWND.r file.
                this.hWnd = FetchWindowHandle(this.processName, this.DeviceConfigPath);


                //string s = GetText(hWnd, this.elemIdList[0]);
                // this.lastStartState = (s == "1");

                // Start timer to work.
                this.timer = new Timer(new TimerCallback((object o) => {

                    DateTime now = DateTime.Now;
                    if (!this.IsRightTime(now))
                    {
                        return;
                    }

                    int second = (now.Second < 30) ? 0 : 30;
                    DateTime rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                    if (rightTime == this.lastDateTime)
                    {
                        return;
                    }
                    this.lastDateTime = rightTime;

                    if (hWnd == IntPtr.Zero)
                    {
                        // Defensive code.
                        return;
                    }

                    List<string> dataSet = GetData(hWnd);
                    bool startState = (dataSet[0] == "1");
                    if (startState)
                    {
                        if (this.lastStartState == false)
                        {
                            this.lastStartState = true;
                            this.GenCurrentSid();
                            this.beginTime = rightTime.ToString();
                        }
                        this.StoreData(dataSet, this.currentSid.ToString(), beginTime, null, rightTime);
                    }
                    else
                    {
                        if (this.lastStartState)
                        {
                            this.lastStartState = false;
                            this.StoreData(dataSet, this.currentSid.ToString(), beginTime, rightTime.ToString(), rightTime);
                        }
                    }


                }), null, 1000, 2000);
                
            }));

            thread.Start();
		}

        private void StoreData(List<string> dataSet, string currentSid, string beginTime, string endTime, DateTime currentTime)
        {
            dataSet.Add(currentSid);
            dataSet.Add(beginTime);
            dataSet.Add(endTime);

            DeviceData dd = default(DeviceData);
            bool got = this.GetDeviceData(dataSet.ToArray(), currentTime, out dd);
            if (got)
            {
                this.lastDateTime = currentTime;
                this.SynchronizationContext.Post(this.DataReceived, dd);
            }
        }

		public override void Stop()
		{
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
            if (this.thread != null)
            {
                try
                {
                    this.thread.Abort();
                }
                catch (ThreadAbortException)
                {
                    this.thread = null;
                }
            }
		}

		public override void Send(byte[] action, DateTime time)
		{
			throw new NotImplementedException();
		}

        // VB form data every 30 sec.
        // Verify the time (second) is the right time.
        private bool IsRightTime(DateTime now)
        {
            if (now.Second >= 0 && now.Second <= MaxDelay)
            {
                return true;
            }
            else if (now.Second >= 30 && now.Second <= (30 + MaxDelay))
            {
                return true;
            }
            return false;
        }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Scada.Common;
using System.Reflection;
using System.Globalization;
using Scada.Config;

namespace Scada.Declare
{
    /*
     * Weather: Send ':D' every 30s to get the data.
     * Weather: Send ':S' every 24h to reset the weather device, to reset the rain gauge value
     */
    public class WeatherDevice : Device
	{
		private const int ComDataBits = 8;

        private DeviceEntry entry = null;

		private SerialPort serialPort = null;

		private int readTimeout = 12000;		//Receive timeout

		private int baudRate = 9600;

        private int dataBits = 8;

        private StopBits stopBits = StopBits.One;

        private Parity parity = Parity.None;

		private bool isVirtual = false;

        // retrieve data command
		private byte[] actionSend1 = null;

        // reset device command
        private byte[] actionSend2 = null;

        private int actionInterval = 0;

		private string linePattern = string.Empty;

		private string insertIntoCommand = string.Empty;

		private FieldConfig[] fieldsConfig = null;

        private IMessageTimer senderTimer = null;

		private Timer timer = null;

		private bool handled = true;

        private string exampleLine;

		private List<byte> exampleBuffer = new List<byte>();


		private string error = "No Error";

        // 假的风速数据
        private string falsevalue;

        // private static int MaxDelay = 10;

        private DateTime currentRecordTime = default(DateTime);

        private byte[] lastLine;

        // Serial port sleep 400 ms as default before read
        private int bufferSleep = 400;

		public WeatherDevice(DeviceEntry entry)
		{
            this.entry = entry;
			if (!this.Initialize(entry))
			{
				string initFailedEvent = string.Format("Device '{0}' initialized failed. Error is {1}.", entry[DeviceEntry.Identity], error);
				RecordManager.DoSystemEventRecord(this, initFailedEvent);
			}
		}

        ~WeatherDevice()
        {
        }

		private bool Initialize(DeviceEntry entry)
		{
			this.Name = entry[DeviceEntry.Name].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
			this.Version = entry[DeviceEntry.Version].ToString();

            this.baudRate = this.GetValue(entry, DeviceEntry.BaudRate, 9600);
            this.readTimeout = this.GetValue(entry, DeviceEntry.ReadTimeout, 12000);        
            this.dataBits = this.GetValue(entry, DeviceEntry.DataBits, ComDataBits);
            this.stopBits = (StopBits)this.GetValue(entry, DeviceEntry.StopBits, (int)StopBits.One);

			StringValue parity = (StringValue)entry[DeviceEntry.Parity];
			this.parity = SerialPorts.ParseParity(parity);

            // 
            this.actionSend1 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend1"]);
            this.actionSend2 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend2"]);

            // 仅用于假的风速数据
            this.falsevalue = (StringValue)entry["Falsevalue"];
			
			// Virtual On
			string isVirtual = (StringValue)entry[DeviceEntry.Virtual];
			if (isVirtual != null && isVirtual.ToLower() == "true")
			{
				this.isVirtual = true;
			}

            string bufferSleepString = (StringValue)entry["BufferSleep"];
            if (bufferSleepString != null)
            {
                this.bufferSleep = int.Parse(bufferSleepString);
            }

			// this.actionDelay = (StringValue)entry[DeviceEntry.ActionDelay];

            const int DefaultRecordInterval = 30;
            this.actionInterval = this.GetValue(entry, DeviceEntry.ActionInterval, DefaultRecordInterval);
            this.RecordInterval = this.GetValue(entry, DeviceEntry.RecordInterval, DefaultRecordInterval);
            this.recordTimePolicy.Interval = this.RecordInterval;

            string tableName = (StringValue)entry[DeviceEntry.TableName];
            if (!string.IsNullOrEmpty(tableName))
            {
                string tableFields = (StringValue)entry[DeviceEntry.TableFields];

                string[] fields = tableFields.Split(',');
                string atList = string.Empty;
                for (int i = 0; i < fields.Length; ++i)
                {
                    string at = string.Format("@{0}, ", i + 1);
                    atList += at;
                }
                atList = atList.TrimEnd(',', ' ');

                // Insert into
                string cmd = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
                this.insertIntoCommand = cmd;
            }

			string fieldsConfigStr = (StringValue)entry[DeviceEntry.FieldsConfig];
            List<FieldConfig> fieldConfigList = ParseDataFieldConfig(fieldsConfigStr);
			this.fieldsConfig = fieldConfigList.ToArray<FieldConfig>();

			if (!this.IsRealDevice)
			{
				string el = (StringValue)entry[DeviceEntry.ExampleLine];
				el = el.Replace("\\r", "\r");
				el = el.Replace("\\n", "\n");

				this.exampleLine = el;
			}
			return true;
		}

        public bool IsRealDevice
        {
            get
            {
                return !this.isVirtual;
            }
        }

		private bool IsOpen
		{
			get
			{
                return this.IsRealDevice ? this.serialPort.IsOpen : true;
			}
		}

		public bool Connect(string portName)
		{
            try
            {
                this.serialPort = new SerialPort(portName);

                this.serialPort.BaudRate = this.baudRate;

                this.serialPort.Parity = this.parity;       //Parity none
                this.serialPort.StopBits = this.stopBits;    //(StopBits)this.stopBits;    //StopBits 1
                this.serialPort.DataBits = this.dataBits;               // this.dataBits;   // DataBits 8bit
                this.serialPort.ReadTimeout = 10000;        // this.readTimeout;

                this.serialPort.RtsEnable = true;
                this.serialPort.NewLine = "/r/n";	        //?
                this.serialPort.DataReceived += this.SerialPortDataReceived;

                // Real Devie begins here.
                if (this.IsRealDevice)
				{
					this.serialPort.Open();

					if (this.actionInterval > 0)
					{
						this.StartSenderTimer(this.actionInterval);
					}

                    // Set status of starting.
                    PostStartStatus();
				}
				else
				{
                    RecordManager.DoSystemEventRecord(this, "Notice, Virtual Device Started");
                    this.StartVirtualDevice();
				}

            }
            catch (IOException e)
            {
                string message = "IO: " + e.Message;
                RecordManager.DoSystemEventRecord(this, message);
                return false;
            }
            catch (Exception e)
            {
                string message = "Other: " + e.Message;
                RecordManager.DoSystemEventRecord(this, message);
                return false;
            }

			return true;
		}

        private void StartSenderTimer(int interval)
        {
            // timer 每4s一次
            int minInterval = 2;
            if (MainApplication.TimerCreator != null)
            {
                this.senderTimer = MainApplication.TimerCreator.CreateTimer(minInterval);

                this.senderTimer.Start(() => 
                {
                    this.Write();
                });
            }
        }

        //////////////////////////////////////////////////////////////////////
        // Virtual-Device.
        private void StartVirtualDevice()
        {
            if (this.actionInterval > 0)
            {
                this.StartSenderTimer(this.actionInterval);
            }
            else
            { }

            return;
        }
        //////////////////////////////////////////////////////////////////////

		private byte[] ReadData()
		{

			if (this.IsRealDevice)
			{
                // important, sleep 400ms to wait all the data come to system buffer, Kaikai
                Thread.Sleep(this.bufferSleep);

				int n = this.serialPort.BytesToRead;
				byte[] buffer = new byte[n];

				int r = this.serialPort.Read(buffer, 0, n);

				return buffer;
			}
			else // Virtual Device~!
			{
                if (this.actionInterval > 1)
                {
                    // 假设: 应答式的数据，都是完整的帧.
                    return this.GetExampleLine();
                }
                else
                {
                    return this.GetExampleLine();
                }
			}
		}

        private string[] Search(byte[] data, byte[] lastData)
		{
            string[] ret = this.Search(data);

            // 东北气象站坏了，给个假的值
            if (this.falsevalue == "true")
            {
                double windspeed = 0;
                if (double.TryParse(ret[7], out windspeed))
                {
                    if (windspeed <= 0)
                    {
                        Random ro = new Random();
                        windspeed = 0.5 + ro.NextDouble();
                        windspeed = Math.Round(windspeed, 1);
                        ret[7] = windspeed.ToString();
                    }
                }
            }
            
            double rd = 0;
            double r = 0;
            if (double.TryParse(ret[8], out r))
            {
                if (lastData != null)
                {
                    string[] ld = this.Search(lastData);
                    double rl = 0;
                    if (double.TryParse(ld[8], out rl))
                    {
                        rd = r - rl;
                    }
                }
                else
                {
                    rd = 0;
                }
            }
            else
            {
                // TODO: ? Average.?
                ret[8] = "0";
            }

            // 每次重置之后，rd值为负数，此时需要把rd重置
            if (rd < 0)
            {
                rd = 0;
            }

            // caculate rainspeed 
            if (ret.Length >= 9)
            {
                double speed = Math.Round(rd, 1);
                ret[9] = speed.ToString();
            }

            // caculate pressure
            double pressure = 0;
            if (double.TryParse(ret[5], out pressure))
            {
                pressure = Math.Round(pressure / 10, 1);
                ret[5] = pressure.ToString();
            }

            return ret;
		}

        private string[] Search(byte[] data)
        {
            // >"11/29/12","00:58", 10.0, 55,  1.3,1018.4,360,  0.0,   0.0,2,!195
            string line = Encoding.ASCII.GetString(data);

            int p = line.IndexOf('>');
            line = line.Substring(p + 1);
            string[] items = line.Split(',');
            for (int i = 0; i < items.Length; ++i)
            {
                items[i] = items[i].Trim();
                if (i == 6)
                {
                    int d = 0;
                    if (int.TryParse(items[i], out d))
                    {
                        items[i] = d.ToString();
                    }
                }
            }
            return items;
        }

		private byte[] GetLineBytes(byte[] data)
		{
            int len = data.Length;
            if (len < 2)
            {
                return data;
            }

            if (data[len - 2] == (byte)0x0d && data[len - 1] == (byte)0x0a)
            {
                return data;
            }
            
            return null;
		}

		private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs evt)  
		{
			Debug.Assert(this.DataReceived != null);
			try
			{
				handled = false;
				byte[] buffer = this.ReadData();

				byte[] line = this.GetLineBytes(buffer);
				if (line == null || line.Length == 0)
				{
                    return;
				}

                if (this.OnReceiveData(line))
                {
                    // 存入数据库
                    this.RecordData(line);
                }

			}
			catch (InvalidOperationException e)
			{
                RecordManager.DoSystemEventRecord(this, e.Message);
			}
			finally
			{
				handled = true;
			}
		}

        internal void RecordData(byte[] line)
        {
            DeviceData dd;
            if (!this.GetDeviceData(line, this.currentRecordTime, out dd))
            {
                dd = new DeviceData(this, null);
                dd.OriginData = DeviceData.ErrorFlag;
                this.SynchronizationContext.Post(this.DataReceived, dd);
                return;
            }

            // Post to Main thread to record.
            dd.OriginData = Encoding.ASCII.GetString(line);
            this.SynchronizationContext.Post(this.DataReceived, dd);
        }

        private void PostStartStatus()
        {
            DeviceData dd = new DeviceData(this, null);
            dd.OriginData = DeviceData.BeginFlag;
            this.SynchronizationContext.Post(this.DataReceived, dd);
        }

		protected bool GetDeviceData(byte[] line, DateTime time, out DeviceData dd)
		{
            dd = default(DeviceData);

            string[] data = null;
            try
            {
                data = this.Search(line, this.lastLine);
                this.lastLine = line;

                if (data == null || data.Length == 0)
                {
                    return false;
                }
                dd.Time = time;
                object[] fields = Device.GetFieldsData(data, time, this.fieldsConfig);
                dd = new DeviceData(this, fields);
                dd.InsertIntoCommand = this.insertIntoCommand;
            }

            catch (Exception e)
            {
                string strLine = Encoding.ASCII.GetString(line);
                string errorMsg = string.Format("GetDeviceData() Fail, Data={0}", strLine) + e.Message;
                RecordManager.DoSystemEventRecord(this, errorMsg);

                return false;
            }

			return true;
		}

		public override void Start(string address)
        {
            if (!this.Connect(address))
            {
                RecordManager.DoSystemEventRecord(this, "Connection Failure");
            }
        }

		public void Write()
		{
            if (this.serialPort == null || !this.IsOpen)
            {
                return;
            }

            try
            {
                // 归一化时间
                DateTime rightTime = default(DateTime);
                if (!this.recordTimePolicy.NowAtRightTime(out rightTime) ||
                    this.currentRecordTime == rightTime)
                {
                    return;
                }
                this.currentRecordTime = rightTime;

                // 记录当前值
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend1, 0, this.actionSend1.Length);
                }
                
                // 23:59:30时重置气象探测器
                if (this.currentRecordTime.ToString("HH:mm:ss").Equals("23:59:30"))
                {
                    // 等待2s，再发送重置命令
                    Thread.Sleep(2000);

                    if (this.IsRealDevice)
                    {
                        this.serialPort.Write(this.actionSend2, 0, this.actionSend2.Length);
                    }
                }
                
                #region Virtual-Device
                else
                {
                    this.OnSendDataToVirtualDevice(this.actionSend1);
                }
                #endregion
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, "Write COM Data Error: " + e.Message, RecordType.Error);
            }

            
		}

        public override void Stop()
        {
            if (this.senderTimer != null)
            {
                this.senderTimer.Close();
            }

            if (this.serialPort != null && this.IsOpen)
            {
                this.serialPort.Close();
            }
        }

        public override void Send(byte[] action, DateTime time)
        {
        }

        public override bool OnReceiveData(byte[] line)
        {
            string strLine = Encoding.ASCII.GetString(line);
            // 判断返回值是否为重置探测器，如是，则不存储数据库。
            if (strLine.Equals(">OK\r\n"))
            {
                RecordManager.DoSystemEventRecord(this, "Reset Weather Station Succeeful!", RecordType.Event);
                return false;
            }

            return true;
        }

#region virtual-device
        private void OnSendDataToVirtualDevice(byte[] action)
        {
            if (Bytes.Equals(action, this.actionSend1))
            {
                if (this.actionInterval > 0)
                {
					this.SerialPortDataReceived("virtual-device", null);
                }
                else
                {
                    this.timer = new Timer(new TimerCallback((object state) =>
                    {
						if (handled)
						{
							this.SerialPortDataReceived("virtual-device", null);
						}
					}), null, 2000, 5000);
                }
            }	
        }

        private byte[] GetExampleLine(int rand = 0)
        {
			return Encoding.ASCII.GetBytes(this.exampleLine);
        }
#endregion
	}
}

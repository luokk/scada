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
using System.Data.Sql;
using System.Data.SqlClient;

namespace Scada.Declare
{
    public class Bai9850Device : Device
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

        // retrieve gammalong command
		private byte[] actionSend1 = null;

        // retrieve emissionlong command
        private byte[] actionSend2 = null;

        private byte[] actionSend3 = null;

        private byte[] actionSend4 = null;

        private byte[] actionSend5 = null;

        private byte[] actionSend6 = null;

        private byte[] actionSend7 = null;

        private int actionInterval = 0;

		private string linePattern = string.Empty;

		private string insertIntoCommand = string.Empty;

		private FieldConfig[] fieldsConfig = null;

        private IMessageTimer senderTimer = null;

		private Timer timer = null;

		private bool handled = true;

        private string exampleLine;

		private List<byte> exampleBuffer = new List<byte>();

        // do not support virtual device
        private bool IsRealDevice = true;

		private string error = "No Error";

        // private static int MaxDelay = 10;

        private DateTime currentRecordTime = default(DateTime);

        private string alphaactivity;

        private string alpha;

        private string betaactivity;

        private string beta;

        private string i131activity;

        private string i131;

        private string doserate;

        // Serial port sleep 200 ms as default before read
        private int bufferSleep = 200;

		public Bai9850Device(DeviceEntry entry)
		{
            this.entry = entry;
			if (!this.Initialize(entry))
			{
				string initFailedEvent = string.Format("Device '{0}' initialized failed. Error is {1}.", entry[DeviceEntry.Identity], error);
				RecordManager.DoSystemEventRecord(this, initFailedEvent);
			}
		}

        ~Bai9850Device()
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

            // must add "\r"
            this.actionSend1 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend1"] + "\r");

            this.actionSend2 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend2"] + "\r");

            this.actionSend3 =Encoding .ASCII .GetBytes ((StringValue )entry ["ActionSend3"]+"\r");

            this.actionSend4 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend4"] + "\r");

            this.actionSend5 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend5"] + "\r");

            this.actionSend6 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend6"] + "\r");

            this.actionSend7 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend7"] + "\r");

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

			return true;
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
                this.serialPort.NewLine = "\r";	        //?
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
            // timer 每5s一次
            int minInterval = 5;
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

		private byte[] GetLineBytes(byte[] data)
		{
            int len = data.Length;

            if (data[len - 1] == (byte)0x0d)
            {
                data[len - 1] = 0;
                return data;
            }

            else { return null; }
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

            string[] data = new string[7];

            if (this.alphaactivity == null || this.alpha == null || this.betaactivity == null ||
                this.beta == null || this.i131activity == null || this.i131 == null || this.doserate == null)
            {
                RecordManager.DoSystemEventRecord(this, "Measurement is null");
                return false;
            }

            //fill the measurement to data
            data[0] = this.alphaactivity ;
            data[1] = this.alpha ;
            data[2] = this.betaactivity ;
            data[3] = this.beta ;
            data[4] = this.i131activity;
            data[5] = this.i131;
            data[6] = this.doserate;

            dd.Time = time;
            object[] fields = Device.GetFieldsData(data, time, this.fieldsConfig);
			dd = new DeviceData(this, fields);
			dd.InsertIntoCommand = this.insertIntoCommand;

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

                // 取alphaactivity
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend1, 0, this.actionSend1.Length);
                }

                Thread.Sleep(500);

                // 取betaactivity
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend2, 0, this.actionSend2.Length);
                }
             
                Thread .Sleep (500);
                
                // 取I131activity
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend3, 0, this.actionSend3.Length);
                }

                Thread .Sleep(500);

                //取alpha比活度
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend4, 0, this.actionSend4.Length);
                }

                Thread .Sleep (500);

                //取beta比活度
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend5, 0, this.actionSend5.Length);
                }
                Thread .Sleep (500);

                //取I131比活度
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend6, 0, this.actionSend6.Length);
                }
                Thread .Sleep(500);
                
                //取doserate
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend7, 0, this.actionSend7.Length);
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
            string strData = System.Text.Encoding.Default.GetString(line);

            if (strData.Contains("900000010105000008000000")) //alphaactivity返回值
            {
                string tmp6 = strData.Substring(25, 12);
                if (tmp6.Substring(tmp6.Length - 1, 1) == "*")
                {
                    string alphaactivitySci;
                    alphaactivitySci = tmp6.Remove(tmp6.Length - 1, 1);
                    this.alphaactivity = Convert.ToDecimal(Convert.ToDouble(alphaactivitySci)).ToString();
                }
                else
                {
                    this.alphaactivity = Convert.ToDecimal(Convert.ToDouble(tmp6 )).ToString(); ;//alphaactivity
                }
                              
                // 这里只取值，不存储
                return false;
            }

            else if (strData.Contains("900000010106000008000000")) //betaactivity返回值
            {
                string tmp5 = strData.Substring(25, 12);
                if (tmp5.Substring(tmp5.Length - 1, 1) == "*")
                {
                    string betaactivitySci;
                    betaactivitySci = tmp5.Remove(tmp5.Length - 1, 1);
                    this.betaactivity = Convert.ToDecimal(Convert.ToDouble(betaactivitySci)).ToString();
                }
                else
                {   
                    this.betaactivity = Convert.ToDecimal(Convert.ToDouble(tmp5)).ToString(); ;//alpha
                }

                // 这里只取值，不存储
                return false;
            }

            else if (strData.Contains("900000010111000008000000")) //i131activity返回值
            {
                string tmp4 = strData.Substring(25, 12);
                if (tmp4.Substring(tmp4.Length - 1, 1) == "*")
                {
                    string i131activitySci;
                    i131activitySci = tmp4.Remove(tmp4.Length - 1, 1);
                    this.i131activity = Convert.ToDecimal(Convert.ToDouble(i131activitySci)).ToString();
                }
                else
                {
                    this.i131activity = Convert.ToDecimal(Convert.ToDouble(tmp4)).ToString(); ;//i131activity
                }

                // 这里只取值，不存储
                return false;
            }

            else if (strData.Contains("900000010105000002000000")) //alpha返回值
            {
                string tmp3 = strData.Substring(25, 12);
                if (tmp3.Substring(tmp3.Length - 1, 1) == "*")
                {
                    string alphaSci;
                    alphaSci = tmp3.Remove(tmp3.Length - 1, 1);
                    this.alpha = Convert.ToDecimal(Convert.ToDouble(alphaSci)).ToString();
                }
                else
                {
                    this.alpha  = Convert.ToDecimal(Convert.ToDouble(tmp3)).ToString(); ;//alpha
                }

                // 这里只取值，不存储
                return false;
            }


            else if (strData.Contains("900000010106000002000000")) //beta返回值
            {
                string tmp2 = strData.Substring(25, 12);
                if (tmp2.Substring(tmp2.Length - 1, 1) == "*")
                {
                    string betaSci;
                    betaSci = tmp2.Remove(tmp2.Length - 1, 1);
                    this.beta = Convert.ToDecimal(Convert.ToDouble(betaSci)).ToString();
                }
                else
                {
                    this.beta = Convert.ToDecimal(Convert.ToDouble(tmp2)).ToString(); ;//beta
                }

                // 这里只取值，不存储
                return false;
            }

            else if (strData.Contains("900000010111000002000000")) //i131返回值
            {
                string tmp1 = strData.Substring(25, 12);
                if (tmp1.Substring(tmp1.Length - 1, 1) == "*")
                {
                    string i131Sci;
                    i131Sci = tmp1.Remove(tmp1.Length - 1, 1);
                    this.i131 = Convert.ToDecimal(Convert.ToDouble(i131Sci)).ToString();
                }
                else
                {
                    this.i131  = Convert.ToDecimal(Convert.ToDouble(tmp1)).ToString(); ;//i131
                }

                // 这里只取值，不存储
                return false;
            }

            else if (strData.Contains("900000010113000008000000")) //doserate返回值
            {
                string tmp0 = strData.Substring(25, 12);
                if (tmp0.Substring(tmp0.Length - 1, 1) == "*")
                {
                    string doserateSci;
                    doserateSci = tmp0.Remove(tmp0.Length - 1, 1);
                    this.doserate = Convert.ToDecimal(Convert.ToDouble(doserateSci)).ToString();
                }
                else
                {
                    this.doserate = Convert.ToDecimal(Convert.ToDouble(tmp0)).ToString(); ;//i131
                }

                // 这里进行存储
                return true;
            }

            else { return false; }
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

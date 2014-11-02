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
     * HPIC:    Send 'SFTW-131-001ER Ver' once, always recv data.
     * Weather: Send ':D' every 30s.
     * DWD: Send HEX(00 32 CD A0 30 30 30 01) every 30s
     * Shelter: Send Q1\r every 30s.
     * 
     * 
     * 
     * 
     */
    public class StandardDevice : Device
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

		private bool actionSendInHex = false;

		private string actionCondition = string.Empty;

		private byte[] actionSend = null;

		// private int actionDelay = 0;

        private int actionInterval = 0;

		private string linePattern = string.Empty;

		private string insertIntoCommand = string.Empty;

		private FieldConfig[] fieldsConfig = null;

		private DataParser dataParser = null;		

        private IMessageTimer senderTimer = null;

		private Timer timer = null;

		private bool handled = true;

        private string exampleLine;

		private List<byte> exampleBuffer = new List<byte>();


		private string error = "No Error";

        // private static int MaxDelay = 10;

        private DateTime currentActionTime = default(DateTime);

        private DateTime currentRecordTime = default(DateTime);

        private byte[] lastLine;

        private bool calcDataWithLastData = false;

        // Serial port sleep 200 ms as default before read
        private int bufferSleep = 200;


		public StandardDevice(DeviceEntry entry)
		{
            this.entry = entry;
			if (!this.Initialize(entry))
			{
				string initFailedEvent = string.Format("Device '{0}' initialized failed. Error is {1}.", entry[DeviceEntry.Identity], error);
				RecordManager.DoSystemEventRecord(this, initFailedEvent);
			}
		}

        ~StandardDevice()
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

			this.actionCondition = (StringValue)entry[DeviceEntry.ActionCondition];
			string actionSendInHex = (StringValue)entry[DeviceEntry.ActionSendInHex];
			if (actionSendInHex != "true")
			{
				string actionSend = (StringValue)entry[DeviceEntry.ActionSend];
                if (actionSend != null)
                {
                    actionSend = actionSend.Replace("\\r", "\r");
                    this.actionSend = Encoding.ASCII.GetBytes(actionSend);
                }
			}
			else
			{
				this.actionSendInHex = true;
				string hexes = (StringValue)entry[DeviceEntry.ActionSend];
                if (!string.IsNullOrEmpty(hexes))
                {
                    hexes = hexes.Trim();
                    this.actionSend = DeviceEntry.ParseHex(hexes);
                }
			}

			// this.actionDelay = (StringValue)entry[DeviceEntry.ActionDelay];

            const int DefaultRecordInterval = 30;
            this.actionInterval = this.GetValue(entry, DeviceEntry.ActionInterval, DefaultRecordInterval);
            this.RecordInterval = this.GetValue(entry, DeviceEntry.RecordInterval, DefaultRecordInterval);
            this.recordTimePolicy.Interval = this.RecordInterval;

            this.calcDataWithLastData = this.GetValue(entry, "CalcLast", 0) == 1;

			// Set DataParser & factors
            string dataParserClz = (StringValue)entry[DeviceEntry.DataParser];
            this.dataParser = this.GetDataParser(dataParserClz);
            this.SetDataParserFactors(this.dataParser, entry);

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
					else
                    {
                        this.Write(this.actionSend);
					}
                    // Set status of starting.
                    PostStartStatus();

                    /* TODO: Remove after test.
                    if (this.actionCondition == null || this.actionCondition.Length == 0)
                    {
                        this.Send(this.actionSend);
                    }
                    */
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
            }

			return true;
		}

        private void StartSenderTimer(int interval)
        {
            if (MainApplication.TimerCreator != null)
            {
                const int MinInterval = 2;
                this.senderTimer = MainApplication.TimerCreator.CreateTimer(MinInterval);
                // Trigger every 2s.
                this.senderTimer.Start(() => 
                {
                    this.Write(this.actionSend);
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
            else if (this.actionInterval == 0)
            {
                this.OnSendDataToVirtualDevice(this.actionSend);
            }
            /*
            else if (!string.IsNullOrEmpty(this.actionCondition))
            {
                this.Send(this.actionSend);
            }
            */
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
					if (this.actionSendInHex)
					{
						return this.GetExampleLine();
					}
					// 不完整帧模拟
					byte[] bytes = this.GetExampleLine();
					int len = bytes.Length;
					foreach (byte b in bytes)
					{
						exampleBuffer.Add(b);
					}

					int c = new Random().Next(len - 5, len + 5);
					int count = Math.Min(c, len);
					byte[] ret = new byte[count];

					for (int i = 0; i < count; ++i)
					{
						ret[i] = exampleBuffer[i];
					}
					exampleBuffer.RemoveRange(0, count);

					return ret;
				}
			}
		}

		private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs evt)  
		{
			Debug.Assert(this.DataReceived != null);
			try
			{
				handled = false;
				byte[] buffer = this.ReadData();

				byte[] line = this.dataParser.GetLineBytes(buffer);
				if (line == null || line.Length == 0)
				{
                    return;
				}

                /*
                if (this.sensitive)                
                {
                    DeviceData sdd;
                    if (this.GetSensitiveData(line, out sdd))
                    {
                        this.SynchronizationContext.Post(this.DataReceived, sdd);
                    }
                }
                 * */

                if (this.OnReceiveData(line))
                {
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
            // Defect: HPIC need check the right time here.
            // if ActionInterval == 0, the time trigger not depends send-time.
            DateTime rightTime = default(DateTime);
            if (!this.recordTimePolicy.NowAtRightTime(out rightTime) ||
                this.currentRecordTime == rightTime)
            {
                return;
            }

            this.currentRecordTime = rightTime;

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

            if (time == default(DateTime))
            {
                time = DateTime.Now;
            }
            string[] data = null;
            try
            {
                data = this.dataParser.Search(line, this.lastLine);
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

			// deviceData.FieldsConfig = this.fieldsConfig;
			return true;
		}

		private bool IsActionCondition(string line)
		{
            if (this.actionCondition == null || this.actionCondition == string.Empty)
			{
				return true;
			}
			if (line.IndexOf(this.actionCondition) >= 0)
			{
				return true;
			}
			return false;
		}

		public override void Start(string address)
        {
            if (!this.Connect(address))
            {
                RecordManager.DoSystemEventRecord(this, "Connection Failure");
            }
        }

		public void Write(byte[] action)
		{
            if (action == null || action.Length == 0)
            {
                return;
            }

			if (this.serialPort != null && this.IsOpen)
			{
				if (this.IsRealDevice)
				{
                    // RecordManager.DoSystemEventRecord(this, Encoding.ASCII.GetString(action));
                    // BUG for shelter!, Duplicated time key!!
                    // this.currentActionTime = time;
					this.serialPort.Write(action, 0, action.Length);
                }
                #region Virtual-Device
                else
				{
                    this.OnSendDataToVirtualDevice(action);
                }
                #endregion
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
            return true;
        }

#region virtual-device
        private void OnSendDataToVirtualDevice(byte[] action)
        {
            if (Bytes.Equals(action, this.actionSend))
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
			if (this.actionSendInHex)
			{
				return DeviceEntry.ParseHex(this.exampleLine);
			}
			else
			{
				return Encoding.ASCII.GetBytes(this.exampleLine);
			}
        }
#endregion
	}
}

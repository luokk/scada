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
    /*
     * Weather: Send ':D' every 30s to get the data.
     * Weather: Send ':S' every 24h to reset the weather device, to reset the rain gauge value
     */
    public class Bai9125Device : Device
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

        private string gammalong;

        private string gammalongcps;

        private string emissionlong;

        private string emissionlongcps;

        private string betacps;

        private string status;

        private string valve1;

        private string valve2;

        private string valve3;

        private string IPAdress;

        private string UID;

        private string pwd;

        private string database;

        private string tabel;

        private SqlConnection con;

        private string[] data;

        private SqlDataReader reader;

        // Serial port sleep 200 ms as default before read
        private int bufferSleep = 200;

        // init status for first start (第一次重启时需要重置设备，以后每天重置一次)
        private bool initStatus = false;

		public Bai9125Device(DeviceEntry entry)
		{
            this.entry = entry;
			if (!this.Initialize(entry))
			{
				string initFailedEvent = string.Format("Device '{0}' initialized failed. Error is {1}.", entry[DeviceEntry.Identity], error);
				RecordManager.DoSystemEventRecord(this, initFailedEvent);
			}
		}

        ~Bai9125Device()
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

            this.actionSend1 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend1"] + "\r");

            this.actionSend2 = Encoding.ASCII.GetBytes((StringValue)entry["ActionSend2"] + "\r");

            this.IPAdress = (StringValue)entry["IP"];

            this.UID = (StringValue)entry["uid"];

            this.pwd = (StringValue)entry["pwd"];

            this.database = (StringValue)entry["database"];

            this.tabel = (StringValue)entry["table"];
            

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
            // timer 每2s一次
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
           /*
            try
            {
                //读取水采样电脑的数据库
                string constr = "server=" + this.IPAdress + ";database=" + this.database + ";uid=" + this.UID + ";pwd=" + this.pwd;
                this.con = new SqlConnection(constr);
                con.Open();
                string sqlcommand = "select Value1,Value2,Value3 from BAI9125_rec";
                SqlCommand cmd = new SqlCommand(sqlcommand, con);
                this.reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    valve1 = reader[0].ToString();
                    valve2 = reader[1].ToString();
                    valve3 = reader[2].ToString();
                    data[6] = valve1;
                    data[7] = valve2;
                    data[8] = valve3;
                }
                this.reader.Close();
                this.con.Close();
            }
            catch (Exception ex)
            {
                data[6] = "0";
                data[7] = "0";
                data[8] = "0";
                this.reader.Close();
                this.con.Close();
            }
            */
            
            
            //读取水采样电脑数据库结束


            this.data = new string[9];
            //假的beta
            Random rad = new Random();
            double d = rad.NextDouble();//double本来就是产生0-1之间小数的
            string betafake = Convert.ToString(d.ToString("#0.0"));//这里输出是控制输出几位数，0.00表示小数点后两位！假的Beta值
            //fill the measurement to data
            data[0] = this.gammalong;
            data[1] = this.gammalongcps;
            data[2] = this.emissionlong;
            data[3] = this.emissionlongcps;
            data[4] = betafake;
            data[5] = "0";
            data[6] = "0";
            data[7] = "0";
            data[8] = "0";

           


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

                // 取Gammalong
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend1, 0, this.actionSend1.Length);
                }

                Thread.Sleep(500);

                // 取Gammaemission
                if (this.IsRealDevice)
                {
                    this.serialPort.Write(this.actionSend2, 0, this.actionSend2.Length);
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

            if (strData.Contains("900100010100000004000000"))
            {
                string strGammalongSci = strData.Substring(25, 11);
                string strGammalong = Convert.ToDecimal(Convert.ToDouble(strGammalongSci)).ToString();
                this.gammalong = strGammalong;

                this.gammalongcps = Math.Round(Convert.ToDecimal(Convert.ToDouble(strGammalong)) / 8300, 3).ToString();
                    
                // 这里只取值，不存储
                return false;
            }
            else if (strData.Contains("900100010100000000004000"))
            {
                string strEmissionlongSci = strData.Substring(25, 11);
                string strEmissionlong = Convert.ToDecimal(Convert.ToDouble(strEmissionlongSci)).ToString();
                this.emissionlong = strEmissionlong;

                this.emissionlongcps = Math.Round(Convert.ToDecimal(Convert.ToDouble(strEmissionlong)) / 8300, 3).ToString();

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

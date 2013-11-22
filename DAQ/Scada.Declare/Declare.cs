using Scada.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Scada.Declare
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="deviceName"></param>
	/// <param name="data"></param>
	/// <returns></returns>
    public delegate bool OnDataReceived(object sender, string deviceName, string data);

    public enum FieldType
    {
        Null,
        String,
        Int,
        Time,
        TimeNow,
        Bit,
    }

	public class FieldConfig
	{
		public FieldType type;

		public int index;

		public FieldConfig(FieldType type)
		{
			this.type = type;
			this.index = -1;
		}
	}


	public struct DeviceData
	{
		private object[] data;

		private Device device;

		private int delay;

		private Action action;

        private DateTime time;

        private string originData;

		private string insertIntoCommand;

		// private FieldConfig[] fieldsConfig;

		public DeviceData(Device device, object[] data)
		{
			this.device = device;
			this.data = data;
			this.delay = 0;
			this.action = null;
            this.time = default(DateTime);
            this.originData = string.Empty;
            this.insertIntoCommand = string.Empty;
            
			// this.fieldsConfig = null;
		}

		public object[] Data
		{
			get { return this.data; }
		}

		public Device Device
		{
			get { return this.device; }
		}

		public int Delay
		{
			get { return this.delay; }
			set { this.delay = value; }
		}

        public DateTime Time
        {
            get
            {
                return this.time;
            }
            set
            {
                this.time = value;
            }
        }

		public Action Action
		{
			get { return this.action; }
			set { this.action = value; }
		}

		public string InsertIntoCommand
		{
			get { return this.insertIntoCommand; }
			set { this.insertIntoCommand = value; }
		}

        public string OriginData
        {
            get { return this.originData; }
            set { this.originData = value; }
        }
		/*
		public FieldConfig[] FieldsConfig
		{
			get { return this.fieldsConfig; }
			set { this.fieldsConfig = value; }
		}
		*/
	}

    /// <summary>
    /// 
    /// </summary>
    public abstract class Device
    {
		private string name;

        private string path;

		private string version;

        private bool running = false;

        private SynchronizationContext synchronizationContext;

        private SendOrPostCallback dataReceived;

        private static int MaxDelay = 10;

        public const string ScadaDeclare = "Scada.Declare.";

        // Each device follow one time policy.
        public TimePolicy recordTimePolicy = new TimePolicy();


        public Device()
        {
            this.RecordInterval = TimePolicy.Every30Sec;
            this.recordTimePolicy.Interval = TimePolicy.Every30Sec;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

        public string Id
        {
            get;
            set;
        }

        // Device Config Path
        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Version
		{
            get { return this.version; }
			set { this.version = value; }
		}

        public bool Running
        {
            get { return this.running; }
        }

        // Default value is 30s. Maybe need change.
        public int RecordInterval
        {
            get;
            set;
        }

		

        public SendOrPostCallback DataReceived
        {
            get { return this.dataReceived; }
            set { this.dataReceived = value; }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return this.synchronizationContext; }
            set { this.synchronizationContext = value; }
        }

        public abstract void Start(string address);

		public abstract void Stop();

		public abstract void Send(byte[] action, DateTime time);


        public static object[] GetFieldsData(string[] data, DateTime now, FieldConfig[] fieldsConfig)
        {
            int count = fieldsConfig.Length;
            object[] ret = new object[count];
            for (int i = 0; i < count; ++i)
            {
                if (fieldsConfig[i].type == FieldType.TimeNow)
                {
                    ret[i] = now;
                }
                else if (fieldsConfig[i].type == FieldType.Null)
                {
                    ret[i] = "<Null>";
                }
                else if (fieldsConfig[i].index >= 0)
                {
                    int index = fieldsConfig[i].index;
                    if (index > data.Length)
                        return null;
                    string item = data[index];
                    if (fieldsConfig[i].type == FieldType.Bit)
                    {
                        bool r = (item == "1" || item.ToLower() == "true");
                        ret[i] = r;
                    }
                    else
                    {
                        ret[i] = item;
                    }
                }
                else if (fieldsConfig[i].index < 0)
                {
                    if (fieldsConfig[i].type == FieldType.Bit)
                    {
                        ret[i] = false;
                    }
                }
            }
            return ret;
        }


        protected List<FieldConfig> ParseDataFieldConfig(string fieldsConfigStr)
        {
            string[] fieldsConfig = fieldsConfigStr.Split(',');
            List<FieldConfig> fieldConfigList = new List<FieldConfig>();
            for (int i = 0; i < fieldsConfig.Length; ++i)
            {
                string config = fieldsConfig[i];
                config = config.Trim();
                if (config == "Now")
                {
                    FieldConfig fc = new FieldConfig(FieldType.TimeNow);
                    fc.type = FieldType.TimeNow;
                    fieldConfigList.Add(fc);
                }
                else if (config.StartsWith("#"))
                {
                    string cast = string.Empty;
                    int lb = config.IndexOf("(");
                    int rb = config.IndexOf(")");
                    if (lb > 0 && rb > lb)
                    {
                        cast = config.Substring(lb + 1, rb - lb - 1);
                        cast = cast.Trim().ToLower();
                    }
                    FieldType fieldType = FieldType.String;
                    if (cast == "bit")
                    {
                        fieldType = FieldType.Bit;
                    }
                    else if (cast == "int")
                    {
                        fieldType = FieldType.Int;
                    }
                    FieldConfig fc = new FieldConfig(fieldType);
                    int numl = lb > 0 ? lb - 1 : config.Length - 1;
                    fc.index = int.Parse(config.Substring(1, numl));
                    fieldConfigList.Add(fc);
                }
                else if (config == "int")
                {
                    FieldConfig fc = new FieldConfig(FieldType.Int);
                    fc.type = FieldType.Int;
                    fieldConfigList.Add(fc);
                }
                else if (config == "null")
                {
                    FieldConfig fc = new FieldConfig(FieldType.Null);
                    fc.type = FieldType.Null;
                    fieldConfigList.Add(fc);
                }

            }
            return fieldConfigList;
        }

        public static bool GetFactor(DeviceEntry entry, int i, out double v)
        {
            v = 0.0;
            string factor = string.Format("factor{0}", i);
            string s = (StringValue)entry[factor];
            if (s != null && s.Length > 0)
            {
                if (double.TryParse(s, out v))
                {
                    return true;
                }
            }
            return false;
        }

        // VB form data every 30 sec.
        // Verify the time (second) is the right time.
        public static bool At30Sec(DateTime now, out DateTime rightTime)
        {
            int second = (now.Second < 30) ? 0 : 30;
            rightTime = default(DateTime);
            if (now.Second >= 0 && now.Second <= MaxDelay)
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            else if (now.Second >= 30 && now.Second <= (30 + MaxDelay))
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            return false;
        }

        public DataParser GetDataParser(string dataParserClz)
        {
            if (!dataParserClz.Contains('.'))
            {
                dataParserClz = ScadaDeclare + dataParserClz;
            }
            Assembly assembly = Assembly.GetAssembly(typeof(StandardDevice));
            Type deviceClass = assembly.GetType(dataParserClz);
            if (deviceClass != null)
            {
                object dataParser = Activator.CreateInstance(deviceClass, new object[] { });
                return (DataParser)dataParser;
            }
            return null;
        }

        protected void SetDataParserFactors(DataParser dataParser, DeviceEntry entry)
        {
            int i = 0;
            double v = 0.0;
            while (Device.GetFactor(entry, ++i, out v))
            {
                dataParser.Factors.Add(v);
            }
        }

        protected int GetValue(DeviceEntry entry, string entryName, int defaultValue)
        {
            IValue v = entry[entryName];
            if (v != null)
            {
                return (int)(StringValue)v;
            }
            return defaultValue;
        }

        protected string GetValue(DeviceEntry entry, string entryName, string defaultValue)
        {
            IValue v = entry[entryName];
            if (v != null)
            {
                return (StringValue)v;
            }
            return defaultValue;
        }

        /*
        public static bool NowAt30Sec(out DateTime rightTime)
        {
            DateTime now = DateTime.Now;
            int second = (now.Second < 30) ? 0 : 30;
            rightTime = default(DateTime);
            if (now.Second >= 0 && now.Second <= MaxDelay)
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            else if (now.Second >= 30 && now.Second <= (30 + MaxDelay))
            {
                rightTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second);
                return true;
            }
            return false;
        }*/
	}
    // Enc of class Device

    
}

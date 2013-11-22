using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scada.MainVision
{
	public class ConfigItem
	{
		public ConfigItem(string key)
		{
			this.Key = key;
		}

		public string Key
		{
			get;
			private set;
		}

		public string ColumnName
		{
			get;
			set;
		}

        public string Unit
        {
            get;
            set;
        }

		public int FieldIndex
		{
			get;
			set;
		}

        public bool DisplayInChart
        {
            get;
            set;
        }

        public double Max
        {
            get;
            set;
        }

        public double Min
        {
            get; 
            set; 
        }

        public double Height
        {
            get;
            set;
        }

        public bool Alarm 
        { 
            get; 
            set; 
        }

        public double Yellow { get; set; }

        public double Red { get; set; }
    }

	public class ConfigEntry
	{
		private List<ConfigItem> items = new List<ConfigItem>();

		public void Add(ConfigItem item)
		{
			items.Add(item);
		}

        public List<ConfigItem> ConfigItems
		{
			get
			{
				return items;
			}
		}

        public ConfigItem GetConfigItem(string key)
        {
            foreach (var item in items)
            {
                if (key.Equals(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

		public string DisplayName
		{
			get;
			set;
		}

		public string DeviceKey
		{
			get;
			set;
		}

        public string TableName
        {
            get;
            set;
        }

        public string Icon
        {
            get;
            set;
        }

        public DataFilter DataFilter
        {
            get;
            set;
        }

        public int Interval
        {
            set;
            get;
        }

	}

	class Config
	{
		private static Config configInstance = new Config();

		private Dictionary<string, ConfigEntry> dict = new Dictionary<string, ConfigEntry>();

		private string currentParsedDevice;

		public static Config Instance()
		{
			return configInstance;
		}


		public ConfigEntry this[string deviceKey]
		{
			get
			{
				return dict[deviceKey];
			}
		}

		public string[] DeviceKeys
		{
			get
			{
				return dict.Keys.ToArray();
			}
		}

		internal void Load(string fileName)
		{
			using (StreamReader sr = new StreamReader(fileName))
			{
				string line = sr.ReadLine();
				while (line != null)
				{
					line = line.Trim();
					if (line.Length > 0 && !line.StartsWith("#"))
					{
						this.ParseLine(line);
					}
					// Next line.
					line = sr.ReadLine();
				}

                this.BuildIconMapping();
			}
		}

		private void ParseLine(string line)
		{
			// Handle Section
			if (line.StartsWith("[") && line.EndsWith("]"))
			{
				string deviceKey = line.Substring(1, line.Length - 2);
				deviceKey = deviceKey.Trim().ToLower();
				this.currentParsedDevice = deviceKey;
				dict.Add(deviceKey, new ConfigEntry());
				return;
			}

			if (line.StartsWith("{") && line.EndsWith("}"))
			{
				line = line.Trim('{', '}');

				ConfigEntry entry = dict[this.currentParsedDevice];
                entry.DeviceKey = this.currentParsedDevice;
                this.ParseItems(line, entry);
				return;
			}

			if (line.IndexOf('=') > 0)
			{
				string[] kv = line.Split('=');
				if (kv.Length > 0)
				{
					ConfigEntry entry = dict[this.currentParsedDevice];
					string key = kv[0].Trim();
					string value = kv[1].Trim();

					this.ProcessLine(key, value, entry);
				}
			}
		}

        private void ParseItems(string keyValueItems, ConfigEntry entry)
        {
            string[] keyValArray = keyValueItems.Split(';').Select(x => x.Trim()).ToArray();
            foreach (var keyValue in keyValArray)
            {
                string[] kv = keyValue.Split('=');
                if (kv.Length == 2)
                {
                    string key = kv[0].Trim().ToLower();
                    string val = kv[1];
                    if (key == "displayname")
                    {
                        entry.DisplayName = val;
                    }
                    else if (key == "tablename")
                    {
                        entry.TableName = val;
                    }
                    else if (key == "interval")
                    {
                        int interval = int.Parse(val);
                        entry.Interval = interval;
                    }
                    else if (key == "datafilter")
                    {
                        Assembly assembly = Assembly.Load("Scada.MainVision");
                        Type dataFilterType = assembly.GetType("Scada.MainVision." + val);

                        entry.DataFilter = (DataFilter)Activator.CreateInstance(dataFilterType, new object[] { });
                    }
                    else if (key == "datafilterparam")
                    {
                        if (entry.DataFilter != null)
                        {
                            entry.DataFilter.Parameter = val;
                        }
                        else
                        {
                            throw new Exception("DataFilter should be set first!");
                        }
                    }
                    else if (key == "icon")
                    {
                        entry.Icon = string.Format("Images/{0}.png", val);
                    }
                }

            }
        }

		private void ProcessLine(string key, string value, ConfigEntry entry)
		{
            int cp = value.IndexOf('#');
            if (cp > 0)
            {
                value = value.Substring(0, cp - 1);
            }
			string[] v = value.Split(';');
			int c = v.Length;

			string columnName = v[0].Trim();

            bool dynamicDataDisplay = false;
            bool alarm = false;
            double min = 0.0;
            double max = 100.0;
            double height = 100.0;
            double yellow = double.MaxValue;
            double red = double.MaxValue;
            if (v.Length > 1)
            {
                string e2 = v[1].Trim();
                if (e2.StartsWith("("))
                {
                    dynamicDataDisplay = true;
                    this.ParseDisplayParams(e2, out min, out max, out height);
                }

                if (v.Length > 2)
                {
                    string e3 = v[2].Trim();
                    if (e3.StartsWith("alarm"))
                    {
                        alarm = true;
                        e3 = e3.Substring(5);
                        this.ParseAlarmParams(e3, out yellow, out red);
                    }
                }
                else
                {
                    if (e2.StartsWith("alarm"))
                    {
                        alarm = true;
                        e2 = e2.Substring(5);
                        this.ParseAlarmParams(e2, out yellow, out red);
                    }
                }

                string dynDataDisplay = v[1].Trim();
                if (dynDataDisplay.StartsWith("("))
                {
                    dynamicDataDisplay = true;
                    this.ParseDisplayParams(dynDataDisplay, out min, out max, out height);
                }
            }

			var item = new ConfigItem(key);
			item.ColumnName = columnName;
            item.Unit = this.GetUnit(columnName);
            item.Max = max;
            item.Min = min;
            item.Height = height;

            item.DisplayInChart = dynamicDataDisplay;

            item.Alarm = alarm;
            if (alarm)
            {
                item.Yellow = yellow;
                item.Red = red;
            }
			entry.Add(item);
		}

        internal void ParseDisplayParams(string displayParams, out double min, out double max, out double height)
        {
            displayParams = displayParams.Trim('(', ')');
            string[] paramArray = displayParams.Split(',');

            min = double.Parse(paramArray[0]);
            max = double.Parse(paramArray[1]);

            if (paramArray.Length > 2)
            {
                height = double.Parse(paramArray[2]);
            }
            else
            {
                height = 100.0;
            }
        }

        internal void ParseAlarmParams(string alarmParams, out double yellow, out double red)
        {
            alarmParams = alarmParams.Trim('(', ')');
            string[] paramArray = alarmParams.Split(',');
            yellow = double.MaxValue;
            red = double.MaxValue;
            if (paramArray.Length > 1)
            {
                yellow = double.Parse(paramArray[0]);
                red = double.Parse(paramArray[1]);
            }
            else
            {
                red = double.Parse(paramArray[0]);
            }
        }

        private string GetUnit(string columnName)
        {
            int p1 = columnName.IndexOf("(");
            int p2 = columnName.IndexOf(")");
            if (p1 > 0 && p2 > p1)
            {
                return columnName.Substring(p1 + 1, p2 - p1 - 1);
            }
            return string.Empty;
        }

		internal string GetDisplayName(string deviceKey)
		{
			return this.dict[deviceKey].DisplayName;
		}

        private void BuildIconMapping()
        {
            this[DBDataProvider.DeviceKey_Hpic].Icon = "Images/1.png";
            this[DBDataProvider.DeviceKey_NaI].Icon = "Images/2.png";
            this[DBDataProvider.DeviceKey_Weather].Icon = "Images/3.png";
            this[DBDataProvider.DeviceKey_HvSampler].Icon = "Images/4.png";
            this[DBDataProvider.DeviceKey_ISampler].Icon = "Images/5.png";
            this[DBDataProvider.DeviceKey_Shelter].Icon = "Images/6.png";
            this[DBDataProvider.DeviceKey_Dwd].Icon = "Images/7.png";
        }
	}
}

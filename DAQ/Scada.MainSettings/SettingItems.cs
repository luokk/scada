using Scada.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Scada.MainSettings
{
    public class CommonSettings
    {
        private int frequence = 60;

        [Category("Common")]
        [DisplayName("采集频率")]
        [DefaultValue(60)]
        [TypeConverter(typeof(FrequenceConverter))]
        public int Frequence
        {
            get
            {
                return this.frequence;
            }
            set
            {
                this.frequence = value;
            }
        }
    }

    public class PropertyConverter : TypeConverter
    {
        private Dictionary<object, string> dict;

        public PropertyConverter(Dictionary<object, string> dict)
        {
            this.dict = dict;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(this.dict.Keys);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string v = (string)value;
                foreach (var kv in this.dict)
                {
                    if (v == kv.Value)
                    {
                        return kv.Key;
                    }
                }
                return null;
            }
            else
                return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            int v = (int)value;
            if (this.dict.ContainsKey(v))
            {
                return this.dict[v];
            }
            return string.Empty;
        }
        
    }

    class FrequenceConverter : PropertyConverter
    {
        public FrequenceConverter()
            :base(new Dictionary<object, string>
            {
                {30, "30 秒"}, {60, "1 分钟"}, {300, "5 分钟"}
            })
        {
        }
    }


    /// <summary>
    /// 1
    /// </summary>
    [DefaultProperty("AlertValue")]  
    public class HpicSettings : CommonSettings
    {
        private string serialPort = "COM1";

        [Category("Common")]
        [TypeConverter(typeof(SerialPortConverter))]
        [DisplayName("串口")]
        public virtual string SerialPort
        {
            get
            {
                return this.serialPort;
            }
            set
            {
                this.serialPort = value;
            }
        }

        [Category("高压电离室")]
        [DisplayName("剂量率因子")]
        [DefaultValue(1.0)]
        public double Factor
        {
            get;
            set;
        }

        [Category("报警")]
        [DisplayName("剂量率报警阀值")]
        [DefaultValue(100.0)]
        public double AlarmValue
        {
            get;
            set;
        }

    }

    /// <summary>
    /// 2
    /// </summary>
    public class WeatherSettings : CommonSettings
    {
        private string serialPort = "COM1";

        [Category("Common")]
        [TypeConverter(typeof(SerialPortConverter))]
        [DisplayName("串口")]
        public virtual string SerialPort
        {
            get
            {
                return this.serialPort;
            }
            set
            {
                this.serialPort = value;
            }
        }


    }

    /// <summary>
    /// 3
    /// </summary>
    public class MdsSettings : CommonSettings
    {
        [Category("超大流量采样器")]
        [DisplayName("剂量率因子")]
        [DefaultValue(1.0)]
        public double Factor
        {
            get;
            set;
        } 
    }

    /// <summary>
    /// 4
    /// </summary>
    public class AisSettings : CommonSettings
    {
        [Category("碘采样器")]
        [DisplayName("剂量率因子")]
        [DefaultValue(1.0)]
        public double Factor
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 5
    /// </summary>
    public class NaISettings : CommonSettings
    {
        // Hidden
        private string deviceSn = "sara";

        private string ipAddress = "http://192.168.0.1/spectra/";

        private int minuteAdjust = 0;

        [DisplayName("设备编号")]
        public string DeviceSn
        {
            get { return this.deviceSn; }
            set { this.deviceSn = value; }
        }

        [DisplayName("网络地址")]
        public string IPAddress
        {
            get { return this.ipAddress; }
            set { this.ipAddress = value; }
        }

        [DisplayName("时间偏移(分钟)")]
        public int MinuteAdjust
        {
            get { return this.minuteAdjust; }
            set { this.minuteAdjust = value; }
        }
    }

    /// <summary>
    /// 6
    /// </summary>
    public class DwdSettings : CommonSettings
    {
        private string serialPort = "COM1";

        [Category("Common")]
        [TypeConverter(typeof(SerialPortConverter))]
        [DisplayName("串口")]
        public virtual string SerialPort
        {
            get
            {
                return this.serialPort;
            }
            set
            {
                this.serialPort = value;
            }
        }
    }

    /// <summary>
    /// 7
    /// </summary>
    public class ShelterSettings : CommonSettings
    {
        private string serialPort = "COM1";

        [Category("Common")]
        [TypeConverter(typeof(SerialPortConverter))]
        [DisplayName("串口")]
        public virtual string SerialPort
        {
            get
            {
                return this.serialPort;
            }
            set
            {
                this.serialPort = value;
            }
        }
    }
}

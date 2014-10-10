using Scada.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    /* add for MKIII weather station by 2014.7.16*/

    public class WeatherDataParser_MKIII : DataParser
    {
        public WeatherDataParser_MKIII()
        {
            this.lineParser = new LineParser();
        }

        public override string[] Search(byte[] data, byte[] lastData)
        {
            string[] ret = this.Search(data);
           
            // 处理温度，华氏温度转成摄氏度
            double tempreture = 0;
            if (double.TryParse(ret[1], out tempreture))
            {
                tempreture = (tempreture - 32) / 1.8;
                tempreture = Math.Round(tempreture, 1);
                ret[1] = tempreture.ToString();
            }
            else 
            {
                ret[1] = "0";
            }

            // 处理大气压，汞柱气压转成标准大气压
            double pascal = 0;
            if (double.TryParse(ret[3], out pascal))
            {
                pascal = pascal * 101 * 10 / 29.92;
                pascal = Math.Round(pascal, 1);
                ret[3] = pascal.ToString();
            }
            else
            {
                ret[3] = "0";
            }
            
            // 处理风速，mph转m/s
            double windspeed = 0;
            if (double.TryParse(ret[5], out windspeed))
            {
                windspeed = windspeed * 0.28 / 0.6214;
                windspeed = Math.Round(windspeed, 1);
                ret[5] = windspeed.ToString();
            }
            else
            {
                ret[5] = "0";
            }

            // 处理降雨量，1英寸转1mm
            double rain = 0;
            if (double.TryParse(ret[7], out rain))
            {
                rain = rain * 25.4;
                rain = Math.Round(rain, 1);
                ret[7] = rain.ToString();
            }
            else
            {
                ret[7] = "0";
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

        public override byte[] GetLineBytes(byte[] data)
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
            else if (this.lineParser != null)
            {
                return this.lineParser.ContinueWith(data);
            }

            return data;
        }
    }


	public class HPICDataParser : DataParser
	{
		public HPICDataParser()
		{
			this.lineParser = new LineParser();
		}

        public override string[] Search(byte[] data, byte[] lastData)
		{
			// .0000   .0000   .0000   .0000   .5564   383.0   6.136   28.40   .0000 
			string line = Encoding.ASCII.GetString(data);
			int p = line.IndexOf('>');
			line = line.Substring(p + 1);
			string[] items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            double d = 0.0;
            if (double.TryParse(items[4], out d))
            {
                double f = 1.0;
                if (this.Factors.Count > 0)
                {
                    f = this.Factors[0];
                }

                // uGy/h (*1000)==> nGy/h
                items[4] = (f * d * 1000).ToString();
            }
			return items;
		}

		public override byte[] GetLineBytes(byte[] data)
		{
			if (this.lineParser != null)
			{
				return this.lineParser.ContinueWith(data);
			}
			return data;
		}
	}


    public class ShelterDataParser : DataParser
    {
        public ShelterDataParser()
        {
            // this.lineParser = new LineParser();
        }

        public override string[] Search(byte[] data, byte[] lastData)
        {
            // [10:28:09] 2013-5-19 10:28:09;; 1827 2470 2698 1953 4095 4095 4095 4095 
            string line = Encoding.ASCII.GetString(data);
            int p = line.IndexOf('#');
            line = line.Substring(p + 1);
            string[] items = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Temperature, Humidity, IfMainPowerOff, BatteryHours, IfSmoke, IfWater, IfDoorOpen, Alarm
            string[] ret = new string[8];

            string item = string.Empty;

            item = items[0];    // 温度, 第1个是温度
            if (!string.IsNullOrEmpty(item))
            {
                double v = 0.0;
                if (double.TryParse(item, out v))
                {
                    double f = 0.034188;
                    //if (this.Factors.Count >= 1)
                    //{
                    //    f = this.Factors[0];
                    //}
                    int i = (int)(v * f - 40);
                    ret[0] = i.ToString();
                }
                else
                {
                    return null;
                }
            }

            item = items[1];    // 湿度, 第2个是湿度
            if (!string.IsNullOrEmpty(item))
            {
                double v = 0.0;
                if (double.TryParse(item, out v))
                {
                    double f = 0.0244;
                    //if (this.Factors.Count >= 2)
                    //{
                    //    f = this.Factors[1];
                    //}

                    int i = (int)(v * f);
                    ret[1] = i.ToString();
                }
                else
                {
                    return null;
                }
            }

            item = items[11];    // 备电状态
            if (!string.IsNullOrEmpty(item))
            {
                bool isMainPowerOff = item.Trim() == "1";
                ret[2] = isMainPowerOff ? "1" : "0";
            }

            item = items[2];    // 备电时间Hour, 第三个是电压
            if (!string.IsNullOrEmpty(item))
            {
                double v = 0.0;
                if (double.TryParse(item, out v))
                {
                    double f = 0.00488 * 600 * 0.8 / 80;
                    if (this.Factors.Count >= 3)
                    {
                        f = this.Factors[2];
                    }

                    double hour = f * v;// *0.00488 * 0.8 / 80;
                    ret[3] = ((int)hour).ToString();
                }
                else
                {
                    return null;
                }
            }

            item = items[10];    // 烟
            if (!string.IsNullOrEmpty(item))
            {
                bool ifSmoke = (item.Trim() == "1");
                ret[4] = ifSmoke ? "1" : "0";
            }

            item = items[9];    // 水
            if (!string.IsNullOrEmpty(item))
            {
                bool ifWater = (item.Trim() == "1");
                ret[5] = ifWater ? "1" : "0";
            }

            item = items[8];    // 门
            if (!string.IsNullOrEmpty(item))
            {
                bool ifOpen = (item.Trim() == "0");
                ret[6] = ifOpen ? "1" : "0";
            }

            ret[7] = "";

            return ret;
        }

        public override byte[] GetLineBytes(byte[] data)
        {
            // Data in One Frame! And I don't known whether the line-parser is valid. 
            //if (this.lineParser != null)
            //{
            //return this.lineParser.ContinueWith(data);
            //}
            return data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
	public class DWD485ASCIIFormater : DataParser
	{
		public DWD485ASCIIFormater()
		{
			this.lineParser = new DWD485ASCIILineParser();
		}

        public override string[] Search(byte[] bytes, byte[] lastData)
		{
			// 00 
			// 32 CD A0 
			// 3B 
			// 32 
			// 30 30 31 30 33 [Barrel]
            // 30 30 30 30 30 [Time] 02 57? 01 [1A] Seems No 1A 
			
			// Skip 0 123 4
			
			bool open = (bytes[5] == 0x31);
            string ifOpen = string.Format("{0}", open ? 1 : 0);

			// Skip, File number.
			int s1 = bytes[6];
			int s2 = bytes[7];
			int s3 = bytes[8];

            // If Rain.
			bool rain = bytes[9] == 0x31;
            string ifRain = string.Format("{0}", rain ? 1 : 0);

            // 桶状态(0x31  0x32  0x33)
            int iBarrelState = bytes[10] - 0x30;    //[10]
            string barrelState = string.Format("{0}", iBarrelState);
            // 降雨总时间  [11 - (5)]
			string rainTime = Encoding.ASCII.GetString(bytes, 11, 5);	// in minutes;

            // IfRain, Barrel, Alarm, IsLidOpen, CurrentRainTime
            // 0, 1, 2, 3
            return new string[] { ifRain, barrelState, ifOpen, rainTime };
		}

		public override byte[] GetLineBytes(byte[] data)
		{
            // !
            if (data.Length == 19)
            {
                // 1 Completed Data Frame.
                return data;
            }
            else
            {
                // Not Completed.
                return this.lineParser.ContinueWith(data);
            }
		}

    }


	class DWD485ASCIILineParser : LineParser
	{
		private byte[] lineBreak = { (byte)0x01 };

		List<byte> list = new List<byte>();

		public DWD485ASCIILineParser()
		{
		}

		public override byte[] LineBreak
		{
			get 
            { 
                return this.lineBreak; 
            }
			
            set
            {
                this.lineBreak = value;
            }
		}

		private int IndexLineBreak()
		{
			int index = -1;
			int count = list.Count;
			for (int i = 0; i < count; ++i)
			{
				if (list[i] == this.LineBreak[0])
				{
					bool find = true;
					for (int j = 1; j < this.LineBreak.Length && (i + j < count); ++j)
					{
						if (list[i + j] != this.LineBreak[j])
						{
							find = false;
							break;
						}
					}
					if (find)
					{
						return i;
					}
				}
			}
			return index;
		}

		public override byte[] ContinueWith(byte[] data)
		{
			byte[] line = DataParser.EmptyByteArray;
			for (int i = 0; i < data.Length; ++i)
			{
				list.Add(data[i]);
			}

			int p = this.IndexLineBreak();

            int len = this.LineBreak.Length;
			if (p > 0)
			{
                byte[] ret = new byte[p + len];
				list.CopyTo(0, ret, 0, p + len);

				list.RemoveRange(0, p + len);
				return ret;
			}
			else if (p == 0)
			{
				list.RemoveRange(0, this.LineBreak.Length);
				return line;
			}

			return line;
		}
	}
	

	
}

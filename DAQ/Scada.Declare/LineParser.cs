using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
	public class LineParser
	{
		private byte[] lineBreak = { (byte)'\r', (byte)'\n' };

		List<byte> list = new List<byte>();

		public LineParser()
		{
		}

		public virtual byte[] LineBreak
		{
			get { return this.lineBreak; }
			set { this.lineBreak = value; }
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

		public virtual byte[] ContinueWith(byte[] data)
		{
			byte[] line = DataParser.EmptyByteArray;
			for (int i = 0; i < data.Length; ++i)
			{
				list.Add(data[i]);
			}

			int p = this.IndexLineBreak();

			if (p > 0)
			{
				byte[] ret = new byte[p];
				list.CopyTo(0, ret, 0, p);
				list.RemoveRange(0, p);
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

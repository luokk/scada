using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Common
{
	public class Bytes
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
		public static bool Equals(byte[] a, byte[] b)
		{
            if (a == b)
            {
                return true;
            }
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; ++i)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}

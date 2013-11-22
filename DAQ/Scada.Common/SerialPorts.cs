using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Scada.Common
{
	public class SerialPorts
	{
		public static Parity ParseParity(string parity)
		{
			if (parity != null && parity != string.Empty)
			{
				if (parity == "None")
				{
					return Parity.None;
				}
				else if (parity == "Odd")
				{
					return Parity.Odd;
				}
				else if (parity == "Even")
				{
					return Parity.Even;
				}
				else if (parity == "Mark")
				{
					return Parity.Mark;
				}
				else if (parity == "Space")
				{
					return Parity.Space;
				}
			}
			return Parity.None;
		}
	}
}

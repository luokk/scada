using Scada.Declare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
	public interface IRecord
	{
		bool DoRecord(DeviceData data);
	}
}

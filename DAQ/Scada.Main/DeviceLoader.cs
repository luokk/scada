using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Declare;
using System.Threading;

namespace Scada.Main
{
    /// <summary>
    /// 
    /// </summary>
    public class DeviceLoader
    {
		private string deviceName;

        private string version;

		public DeviceLoader(string deviceName)
        {
			this.deviceName = deviceName;
        }

        public DeviceLoader(string deviceName, string version)
        {
            this.deviceName = deviceName;
            this.version = version;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Device Load()
        {
			Device device = null;
			
			return device;
        }

		public void Unload()
		{
			// 1.

			// 2.
		}

    }
}

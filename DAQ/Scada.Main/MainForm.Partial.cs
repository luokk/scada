/**
 * 
 * 
 * 
 */

namespace Scada.Main
{
    using Scada.Declare;
    using System;

    public partial class MainForm
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private bool OnDataArrival(DeviceData deviceData)
		{
            // Record.
			RecordManager.DoDataRecord(deviceData);

            // For Rescue
            Device device = deviceData.Device;
            if (device != null)
            {
                string deviceKey = device.Id.ToLower();
                Program.DeviceManager.UpdateLastModifyTime(deviceKey, DateTime.Now.Ticks);
            }

			return true;
		}

		////////////////////////////////////////////////////////
		public void OnDataReceived(object state)
		{
			if (state is DeviceData)
			{
				this.OnDataArrival((DeviceData)state);


			}
		}


    }
}

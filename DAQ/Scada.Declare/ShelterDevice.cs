using Scada.Config;
using Scada.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    public class ShelterDevice : StandardDevice
    {

        private bool lastDoorState = false;

        private bool lastPowerState = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public ShelterDevice(DeviceEntry entry)
            :base(entry)
        {
        }

        public override bool OnReceiveData(byte[] data)
        {
            DeviceData dd;
            if (this.GetDeviceData(data, DateTime.Now, out dd))
            {
                if (lastDoorState != (bool)dd.Data[7])
                {
                    // Save
                    lastDoorState = (bool)dd.Data[7];

                    // send door status to datacenter
                    Command.Send(Ports.DataClient, string.Format("DOOR={0}", lastDoorState ? "1" : "0"));

                    this.SynchronizationContext.Post(this.DataReceived, dd);
                    return false;
                }


                if (lastPowerState != (bool)dd.Data[3])
                {
                    // Save
                    lastPowerState = (bool)dd.Data[3];
                    this.SynchronizationContext.Post(this.DataReceived, dd);
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}

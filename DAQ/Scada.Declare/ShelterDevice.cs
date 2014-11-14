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

        private bool lastSmokeState = false;

        private bool lastWaterState = false;

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
                // 处理实时报警记录
                if (lastDoorState != (bool)dd.Data[7] || lastPowerState != (bool)dd.Data[3]
                    || lastSmokeState != (bool)dd.Data[5] || lastWaterState != (bool)dd.Data[6])
                {
                    // 存储门禁记录
                    if (lastDoorState != (bool)dd.Data[7])
                    {
                        lastDoorState = (bool)dd.Data[7];

                        // send door status to datacenter
                        Command.Send(Ports.DataClient, string.Format("DOOR={0}", lastDoorState ? "1" : "0"));
                    }

                    // 存储主电源记录
                    if (lastPowerState != (bool)dd.Data[3])
                    {
                        lastPowerState = (bool)dd.Data[3];
                    }

                    // 存储烟感记录
                    if (lastSmokeState != (bool)dd.Data[5])
                    {
                        lastSmokeState = (bool)dd.Data[5];
                    }

                    // 存储浸水记录
                    if (lastWaterState != (bool)dd.Data[6])
                    {
                        lastWaterState = (bool)dd.Data[6];
                    }

                    // 实时存储数据库
                    this.SynchronizationContext.Post(this.DataReceived, dd);
                    return false;
                }

                // 处理归一化时间记录
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}

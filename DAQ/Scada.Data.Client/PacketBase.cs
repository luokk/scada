using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client
{
    public class PacketBase
    {
        /// <summary>
        /// Guid or ...
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }


        public string DeviceKey
        {
            get;
            set;
        }
    }
}

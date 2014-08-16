using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client
{
    public class Notify
    {
        /// <summary>
        /// Guid or ...
        /// </summary>
        public Dictionary<string, string> Payload
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

        public void SetValue(string key, string value)
        {
            if (this.Payload == null)
            {
                this.Payload = new Dictionary<string, string>();
            }
            this.Payload.Add(key, value);
        }

        public string GetValue(string key)
        {
            if (this.Payload == null)
            {
                return null;
            }
            if (this.Payload.ContainsKey(key))
            {
                return this.Payload[key];
            }
            else
            {
                return null;
            }
        }

    }
}

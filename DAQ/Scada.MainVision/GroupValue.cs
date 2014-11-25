using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.MainVision
{
    class GroupValue
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        private int count = 0;

        public void AddValue(Dictionary<string, object> item, params string[] keys)
        {
            foreach (var key in keys)
            {
                object v = item[key];
                if (v is string)
                {
                    double s = 0.0;
                    if (value.ContainsKey(key))
                    {
                        s = (double)value[key];
                        string sv = (string)v;
                        if (sv.Length > 0)
                        {
                            s += double.Parse(sv);
                        }
                    }
                    value[key] = s;
                }
                else if (v is bool)
                {
                    int s = 0;
                    if (value.ContainsKey(key))
                    {
                        s = (int)value[key];
                        s += ((bool)v) ? 1 : 0;
                    }
                    value[key] = s;
                }

            }
            this.count++;
        }

        internal Dictionary<string, object> GetValue(params string[] keys)
        {
            if (this.value == null || this.value.Count == 0)
                return null;
            Dictionary<string, object> ret = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                object v = this.value[key];
                if (v is double)
                {
                    if (value.ContainsKey(key))
                    {
                        ret[key] = (double)v / this.count;
                    }

                }
                else if (v is int)
                {
                    if (value.ContainsKey(key))
                    {
                        ret[key] = (int)v / (count / 2);
                    }
                    
                }
            }
            return ret;
        }

        internal void Clear()
        {
            value.Clear();
            this.count = 0;
        }
    }
}

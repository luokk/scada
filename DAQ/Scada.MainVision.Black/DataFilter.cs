using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.MainVision
{
    public abstract class DataFilter
    {
        public string Parameter
        {
            get;
            set;
        }

        public abstract void Fill(Dictionary<string, object> data, params object[] parameters);

    }
}

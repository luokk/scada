using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.About
{
    class Feature
    {
        public string Description
        {
            get;
            set;
        }

        public bool IsFeature { get; set; }

        public string Progress { get; set; }

        public string PlanDate { get; set; }

        public string ReleasedDate { get; set; }
    }
}

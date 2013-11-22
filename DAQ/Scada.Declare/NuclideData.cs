using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
    struct NuclideData
    {
        public string Name
        {
            get;
            set;
        }

        public string Activity
        {
            get;
            set;
        }

        public string Indication
        {
            get;
            set;
        }


        public string DoseRate
        {
            get;
            set;
        }

        public string Channel
        {
            get;
            set;
        }

        public string Energy
        {
            get;
            set;
        }
    }

    class NuclideDataSet
    {
        public List<NuclideData> sets = new List<NuclideData>();



        public void AddNuclideData(NuclideData nd)
        {
            sets.Add(nd);
        }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string Coefficients { get; set; }

        public string ChannelData { get; set; }

        public string DoseRate { get; set; }

        public string Temperature { get; set; }

        public string HighVoltage { get; set; }

        public bool CalibrationNuclideFound { get; set; }

        public string ReferencePeakEnergyFromPosition { get; set; }
    }

}

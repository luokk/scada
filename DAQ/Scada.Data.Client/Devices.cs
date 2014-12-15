using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client
{
    class Devices
    {
        public const string Prefix = "scada.";

        public const string Hpic = Prefix + "hpic";

        public const string Weather = Prefix + "weather";

        public const string Labr = Prefix + "labrdevice";

        public const string HPGe = Prefix + "hpge";

        public const string CinderellaData = Prefix + "cinderella.data";

        public const string CinderellaStatus = Prefix + "cinderella.status";

        public const string Shelter = Prefix + "shelter";

        public const string LabrFilter = Prefix + "labrfilter";

        public const string LabrNuclideFilter = Prefix + "labrnuclidefilter";
    }
}

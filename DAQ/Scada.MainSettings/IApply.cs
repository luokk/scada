using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.MainSettings
{
    interface IApply
    {
        void Apply();

        void Cancel();
    }
}

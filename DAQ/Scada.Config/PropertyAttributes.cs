using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Scada.Config
{
    public class SerialPortConverter : StringConverter
    {
        private static string[] serialPorts = SerialPort.GetPortNames();

        public SerialPortConverter()
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(serialPorts.ToArray());
        }
    }
}

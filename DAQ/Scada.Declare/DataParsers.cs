using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
	public abstract class DataParser
	{
		public static readonly byte[] EmptyByteArray = new byte[0];

		protected LineParser lineParser;

        private List<double> factors = new List<double>();

		public LineParser GetLineParser()
		{
			return lineParser;
		}

        public List<double> Factors
        {
            get
            {
                return this.factors;
            }
        }
        

		public abstract byte[] GetLineBytes(byte[] data);

		public abstract string[] Search(byte[] data, byte[] lastData);

	}
}

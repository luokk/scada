using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Main
{
    public struct Error
    {
		public const string Framework = "@framework";

		public const string Devices = "@devices";

		public const string Database = "@database";

		// public const string Framework = "";

        private string domain;

        private int code;

        static Error()
        {
            // TODO: Load error descriptions from files.
        }

        public Error(string domain, int code)
        {
            this.domain = domain;
            this.code = code;
        }

        static string Lookup(string domain, int code)
        {
            return string.Empty;
        }

        public string Domain
        {
            get { return this.domain; }
        }

        public int Code
        {
            get
            {
                return this.code;
            }
        }

        public string Message
        {
            get
            {
                return Error.Lookup(this.domain, this.code);
            }
        }
    }

	public static class Errors
	{
		public static Error NoError = default(Error);

		// Framework
		public static Error UnknownError = new Error(Error.Framework, 1);



		// Devices

	}
}

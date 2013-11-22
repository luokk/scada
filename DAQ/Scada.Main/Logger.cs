using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Declare;

namespace Scada.Main
{

	/// <summary>
	/// 
	/// </summary>
	class LogFile
	{
		void Write(LogEvent logEvent, string msg)
		{

		}

	}

	/// <summary>
	/// 
	/// </summary>
	class Logger
	{
		private static Logger logger = new Logger();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static Logger Instance()
		{
			return logger;
		}

		// TODO:
		public LogFile GetLogFile()
		{
			LogFile lf = new LogFile();
			return lf;
		}

		public static LogFile GetLogFile(string name, LogEvent logType)
		{
			// TODO:?
			LogFile lf = Instance().GetLogFile();
			return lf;
		}


	}
}

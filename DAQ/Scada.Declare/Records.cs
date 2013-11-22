using MySql.Data.MySqlClient;
using Scada.Declare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Declare
{
	/// <summary>
	/// for certain Device, and Framework
	/// </summary>
	public class FileRecord : IRecord
	{
		private string module = null;

		public FileRecord(string module)
		{
			this.module = module;
		}

		public bool DoRecord(DeviceData data)
		{
			/// throw new NotImplementedException();
			/// 

			return true;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class AnalysisRecord : IRecord
	{
		public AnalysisRecord()
		{
		}

		public bool DoRecord(DeviceData data)
		{
			/// throw new NotImplementedException();
			/// 
			return true;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class MySQLRecord : IRecord
	{
		private DBConnection conn = null;

		public MySQLRecord()
		{
			// TODO: Initilaize the DB connection
			this.conn = new DBConnection();
            this.conn.Connect();
		}

		public bool DoRecord(DeviceData data)
		{
			if (data.Data != null)
			{
				bool ret = this.conn.AddRecordData(data.InsertIntoCommand, data.Time, data.Data);
				return ret;
			}
			return false;
		}
	}
}

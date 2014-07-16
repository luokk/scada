using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Diagnostics;
using MySql.Data.Types;
using Scada.Config;
using System.Timers;

namespace Scada.Declare
{
	class DBConnection
	{
        /// <summary>
        /// No Need Port 3306.
        /// 
        /// </summary>

        private MySqlConnection conn = null;

		private MySqlCommand cmd = null;

        private const string Localhost = "127.0.0.1";

        private const string Root = "root";

		private const string DAQDatabase = "scada";

        private string database = null;

        public string Database
        {
            get { return this.database; }
            set { this.database = value; }
        }

        public void Connect()
        {
            try
            {
                string connectionString = new DBConnectionString().ToString();
                this.conn = new MySqlConnection(connectionString);
                this.conn.Open();
                this.cmd = this.conn.CreateCommand();
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(Device.Main, e.Message, RecordType.Error);
                
                // disable RetryConnection      by Kaikai
                //this.RetryConnection(e);
            }
        }

        private void RetryConnection(Exception e)
        {
            try
            {
                if (this.conn != null)
                {
                    this.conn.Close();
                    this.conn = null;
                }
            }
            catch (Exception)
            {
                this.conn = null;
            }
            finally
            {
                this.Reconnect();
            }

        }

        public void Disconnect()
        {
            try
            {
                if (this.conn != null)
                {
                    this.conn.Clone();
                    this.conn = null;
                }
            }
            catch (Exception)
            {
                this.conn = null;
            }
            finally
            {
                this.conn = null;
            }
 
        }

        private void Reconnect()
        {
            RecordManager.DoSystemEventRecord(Device.Main, "Reconnecting to MySQL DB", RecordType.Notice);
            this.Connect();
        }

		public bool AddRecordData(string commandText, DeviceData data)
		{
            try
            {
                if (this.cmd != null)
                {
                    this.cmd.CommandText = commandText;
                    var items = data.Data;
                    for (int i = 0; i < items.Length; ++i)
                    {
                        string at = string.Format("@{0}", i + 1);
                        this.cmd.Parameters.AddWithValue(at, items[i]);
                    }
                    
                    int num = this.cmd.ExecuteNonQuery();
                    if (num != 1)
                    {
                        this.cmd.Parameters.Clear();
                        return false;
                    }
                    // If exception, the params would NOT clear.
                    // cmd.Parameters.Clear();

                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(data.Device, string.Format("{0} => {1}", commandText, e.Message), RecordType.Error);
                //this.RetryConnection(e);
                cmd.Parameters.Clear();
                return false;
            }
            finally
            {
                if (this.cmd != null)
                {
                    this.cmd.Parameters.Clear();
                }
            }
			return true;
		}


	}
}

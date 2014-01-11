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

        private Timer retryTimer = null;

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
                this.RetryConnection(e);
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

            this.retryTimer = new Timer(30 * 1000);
            this.retryTimer.Elapsed += this.RetryTimerTick;
            this.retryTimer.Start();
        }

        void RetryTimerTick(object sender, ElapsedEventArgs e)
        {
            if (this.retryTimer != null)
            {
                this.retryTimer.Stop();
                this.retryTimer = null;

                this.Reconnect();
            }
        }

        private void Reconnect()
        {
            RecordManager.DoSystemEventRecord(Device.Main, "Reconnect to MySQL DB", RecordType.Notice);
            this.Connect();
        }

		public bool AddRecordData(string commandText, DateTime time, params object[] items)
		{
            try
            {
                if (this.cmd != null)
                {
                    cmd.CommandText = commandText;

                    for (int i = 0; i < items.Length; ++i)
                    {
                        string at = string.Format("@{0}", i + 1);
                        cmd.Parameters.AddWithValue(at, items[i]);
                    }
                    cmd.ExecuteNonQuery();
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
                this.RetryConnection(e);
                return false;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Parameters.Clear();
                }
            }
			return true;
		}


	}
}

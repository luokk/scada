using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Diagnostics;
using MySql.Data.Types;
using Scada.Config;

namespace Scada.Declare
{
    class DBConnectionForSid
    {
        private MySqlConnection conn = null;

        private MySqlCommand cmd = null;

        private const string Localhost = "127.0.0.1";

        private const string Root = "root";

        private const string DAQDatabase = "scada";

        public int GetCurrentSid(string tableName)
        {
            try
            {
                string connectionString = new DBConnectionString().ToString();
                this.conn = new MySqlConnection(connectionString);
                this.conn.Open();
                this.cmd = this.conn.CreateCommand();

                string query = string.Format("select max(Sid) from {0} ", tableName);
                this.cmd.CommandText = query;

                using (var reader = this.cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int sid = reader.GetInt32(0);
                        return sid;
                    }
                }

                conn.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return 1;
        }

    }
}

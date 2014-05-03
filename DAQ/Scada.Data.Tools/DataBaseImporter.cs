using MySql.Data.MySqlClient;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Tools
{
    class DataBaseImporter
    {
        private string csvFile;

        public DataBaseImporter(string csvFile)
        {
            this.csvFile = csvFile;
        }

        internal void Import()
        {
            var connectionString = new DBConnectionString().ToString();
            using (var connToMySql = new MySqlConnection(connectionString))
            {
                connToMySql.Open();

                MySqlCommand cmd = connToMySql.CreateCommand();

                string sql = string.Format("LOAD DATA INFILE '{0}' IGNORE INTO TABLE scada.hpic_rec FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' lines terminated by '\n' (time,Doserate,Highvoltage,Battery,Temperature);", this.csvFile);
                cmd.CommandText = sql;

                cmd.ExecuteNonQuery();
            }
        }
    }
}

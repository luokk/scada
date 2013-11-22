using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Scada.MainVision
{
    /// <summary>
    /// NaI Device data filter
    /// </summary>
    class NaIDataFilter : DataFilter
    {
        private bool init = false;

        private const string ConnectionString = "datasource=127.0.0.1;username=root;database=scada";

        private MySqlConnection conn = new MySqlConnection(ConnectionString);

        private MySqlCommand cmd = null;

        private void Initialize()
        {
            this.conn.Open();
            this.cmd = this.conn.CreateCommand();
        }

        public override void Fill(Dictionary<string, object> data, params object[] parameters)
        {
            if (!this.init)
            {
                this.Initialize();
                this.init = true;
            }
            string time = (string)data["time"];
            string cmdText = this.GetCommandText(this.Parameter, time);

            this.cmd.CommandText = cmdText;
            using (MySqlDataReader reader = this.cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nuclname = reader.GetString("name");
                    string doserate = reader.GetString("Doserate");
                    string indication = reader.GetString("Indication");

                    string nuclideKey = nuclname.ToLower();
                    string indicationKey = string.Format("Ind({0})", nuclideKey);

                    if (!data.ContainsKey(nuclideKey))
                    {
                        data.Add(nuclideKey, doserate);
                        data.Add(indicationKey, indication);
                    }
                    else
                    {
                        // MessageBox.Show(nuclideKey);
                    }
                }
            }
        }

        private string GetCommandText(string tableName, string time)
        {
            string format = "select * from {0} where time='{1}'";

            return string.Format(format, tableName, time);
        }

    }
}

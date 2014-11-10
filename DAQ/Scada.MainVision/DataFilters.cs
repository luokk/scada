using MySql.Data.MySqlClient;
using Scada.Config;
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

        private MySqlConnection conn = null;

        private MySqlCommand cmd = null;

        private void Initialize()
        {
            string connectionString = new DBConnectionString().ToString();
            this.conn = new MySqlConnection(connectionString);
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
                    string activities = reader.GetString("Activity");
                    string nuclideKey = nuclname.ToLower();
                    string indicationKey = string.Format("Ind({0})", nuclideKey);
                    
                    string ac = activities.Split(' ')[0];
                    double activity = 0.0;
                    double.TryParse(ac, out activity);
                    if (!data.ContainsKey(nuclideKey))
                    {
                        // data.Add(nuclideKey, doserate);
                        // data.Add(indicationKey, indication);
                        data.Add(nuclideKey, string.Format("{0}, {1}({2})", doserate, activity, indication));
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

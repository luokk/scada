using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Config
{
    public class DBConnectionString
    {
        public string Username
        {
            set;
            get;
        }

        public string Password
        {
            set;
            get;
        }

        public string Database
        {
            set;
            get;
        }

        public string Address
        {
            get;
            set;
        }


        public DBConnectionString(string username, string password, string database)
        {
            this.Username = username;
            this.Password = password;
            this.Database = database;

            this.Address = "127.0.0.1"; // As default
        }

        public DBConnectionString()
            :this("root", "root", "scada")
        {
        }

        public override string ToString()
        {
            return string.Format("datasource={0};username={1};password={2};database={3}", this.Address, this.Username, this.Password, this.Database);
        }

    }
}

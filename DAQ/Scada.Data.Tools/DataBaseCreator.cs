using MySql.Data.MySqlClient;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Data.Tools
{
    internal class DataBaseCreator
    {
        private string dataBaseFile;

        private MySqlConnection conn = null;

        private string connectionString = string.Empty;

        public DataBaseCreator(string dataBaseFile)
        {
            this.dataBaseFile = dataBaseFile;
        }


        internal void Execute()
        {
            if (dataBaseFile != null && dataBaseFile.Length > 0)
            {
                if (File.Exists(dataBaseFile))
                {
                    using (FileStream fs = new FileStream(dataBaseFile, FileMode.Open))
                    {
                        long size = fs.Length;
                        byte[] buffer = new byte[size];
                        int r = fs.Read(buffer, 0, (int)size);
                        string content = Encoding.UTF8.GetString(buffer);
                        this.Execute(content);
                    }
                }
            }
        }

        internal void Execute(string content)
        {
            SQLStatementParser parser = new SQLStatementParser();
            CreateDAQDB();
            using(StringReader sr = new StringReader(content))
            {
                this.conn = new MySqlConnection(this.connectionString);
                this.conn.Open();

                MySqlCommand cmd = this.conn.CreateCommand();
                if (cmd != null)
                {
                    Console.WriteLine("DB Connected");
                }

                string line = sr.ReadLine();
                while (line != null)
                {
                    string statement = parser.Add(line);
                    if (statement.Length > 0)
                    {
                        if (statement.StartsWith("/*") && statement.EndsWith("*/"))
                        {
                            
                        }
                        else
                        {
                            string log = string.Format("Execute SQL: {0}", statement);
                            Console.WriteLine(log);

                            this.ExecuteSQL(cmd, statement);
                        }
                    }
                    line = sr.ReadLine();
                }

            }
        }

        internal void ExecuteSQL(MySqlCommand cmd, string statement)
        {

            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
            // cmd.ExecuteScalar();
        }

        internal void CreateDAQDB()
        {
            var s = new DBConnectionString();
            s.Database = "mysql";
            this.connectionString = new DBConnectionString().ToString();
            using (var connToMySql = new MySqlConnection(this.connectionString))
            {
                connToMySql.Open();

                MySqlCommand cmd = connToMySql.CreateCommand();
                cmd.CommandText = "CREATE DATABASE if NOT EXISTS scada";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}

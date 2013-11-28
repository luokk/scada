
namespace Scada.Data.Client.Tcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MySql.Data.MySqlClient;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Threading;
    using Scada.Config;
    
    /// <summary>
    /// Each Device has a Listener.
    /// </summary>
    internal class DBDataSource
    {
        private const int MaxCountFetchRecent = 10;

        private const string Id = "Id";

        private const string Time = "time";

        private MySqlConnection conn = null;

        private MySqlCommand cmd = null;


        private List<string> tables = new List<string>();

        private List<string> deviceKeyList = new List<string>();

        private int minuteAdjustForNaI = 0;
      

        // <DeviceKey, dict[data]>
        private Dictionary<string, object> latestData = new Dictionary<string, object>();


        /// <summary>
        /// 
        /// </summary>
        private DBDataSource()
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
                string msg = e.Message;
            }
        }

        private static DBDataSource instance = null;

        public static DBDataSource Instance
        {
            get
            {
                if (DBDataSource.instance == null)
                {
                    DBDataSource.instance = new DBDataSource();
                }
                return DBDataSource.instance;
            }
        }


        DataPacket GetDataPacket(string deviceKey, DateTime time)
        {
            return default(DataPacket);
        }

        private string GetSelectStatement(string tableName, DateTime time)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} where time='{1}'";
            return string.Format(format, tableName, time.ToString());
        }

        private string GetSelectStatement(string tableName, DateTime fromTime, DateTime toTime)
        {
            // Get the recent <count> entries.
            string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Id DESC";
            string sql = string.Format(format, tableName, toTime, fromTime);
            return sql;
        }

        public Dictionary<string, object> GetData(string deviceKey, DateTime time, string code = null)
        {
            if (this.cmd == null)
            {
                return new Dictionary<string,object>(0);
            }
            // Return values
            const int MaxItemCount = 20;
            var ret = new Dictionary<string, object>(MaxItemCount);

            string tableName = Settings.Instance.GetTableName(deviceKey);

            this.cmd.CommandText = this.GetSelectStatement(tableName, time);
            try
            {
                using (MySqlDataReader reader = this.cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Must Has an Id.
                        string id = reader.GetString(Id);
                        id = id.Trim();

                        if (string.IsNullOrEmpty(id))
                        {
                            return null;
                        }

                        ret.Add(Id, id);

                        List<Settings.DeviceCode> codes = Settings.Instance.GetCodes(deviceKey);

                        string dataTime = reader.GetString("Time");
                        ret.Add("time", dataTime);
                        foreach (var c in codes)
                        {
                            if (code != null && code != c.Code)
                            {
                                continue;
                            }
                            string field = c.Field.ToLower();
                            try
                            {
                                string v = reader.GetString(field);
                                ret.Add(c.Code, v);
                            }
                            catch (SqlNullValueException)
                            {
                                // TODO: Has Null Value
                                ret.Add(c.Code, string.Empty);
                            }
                            catch (Exception)
                            {
                                // No this field.
                            }
                        }



                    }
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(500);
            }

            return ret;

        }

        private string GetDatePath(DateTime date)
        {
            return string.Format("{0}-{1:D2}", date.Year, date.Month);
        }

        public string GetNaIDeviceData(DateTime time)
        {
            string fileName = this.GetFileName(time);
            string datePath = this.GetDatePath(time);
            string filePath = LogPath.GetDeviceLogFilePath("scada.naidevice", time) + "\\" + fileName;
            string content = string.Empty;
            if (File.Exists(filePath))
            {
                StreamReader fs = new StreamReader(filePath);
                content = fs.ReadToEnd();
            }
            
            return content;
        }

        private string GetFileName(DateTime time)
        {
            int minuteAdjust = Settings.Instance.MinuteAdjust;
            string deviceSn = Settings.Instance.NaIDeviceSn;
            string fileName;
            DateTime t = time;
            t = t.AddHours(-8).AddMinutes(minuteAdjust);
            fileName = string.Format("{0}_{1}-{2:D2}-{3:D2}T{4:D2}_{5:D2}_00Z-5min.n42",
                deviceSn, t.Year, t.Month, t.Day, t.Hour, t.Minute / 5 * 5);
            return fileName;
        }

        public static int DateTimeCompare(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            object t1 = a[Time];
            object t2 = b[Time];
            DateTime dt1 = DateTime.MinValue;
            DateTime dt2 = DateTime.MinValue;
            if (t1 != null)
            {
                dt1 = DateTime.Parse((string)t1);
            }
            if (t2 != null)
            {
                dt2 = DateTime.Parse((string)t2);
            }

            if (dt1 > dt2)
            {
                return -1;
            }
            return 1;
        }
    }
}

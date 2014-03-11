
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
    using System.Data.SqlClient;

    public enum ReadResult
    {
        SqlCommandError,
        RecordsWithoutId,
        NoThisField,
        ReadDBException,
        ReadOK,
        NoDataFound,
        ReadIOException,
        ReadSqlException,
        ReadInvalidOpException,

    }
    
    /// <summary>
    /// Each Device has a Listener.
    /// </summary>
    internal class DBDataSource
    {
        private const int MaxCountFetchRecent = 10;

        private const string Id = "Id";

        private const string Time = "time";

        private MySqlConnection mainThreadConn = null;

        private MySqlConnection historyDataThreadConn = null;

        private MySqlCommand mainSqlCmd = null;


        private List<string> tables = new List<string>();

        private List<string> deviceKeyList = new List<string>();

        //private int minuteAdjustForNaI = 0;

        // <DeviceKey, dict[data]>
        private Dictionary<string, object> latestData = new Dictionary<string, object>();

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

        public void Initialize()
        {
            try
            {
                string connectionString = new DBConnectionString().ToString();
                this.mainThreadConn = new MySqlConnection(connectionString);
                this.mainThreadConn.Open();
                this.mainSqlCmd = this.mainThreadConn.CreateCommand();
            }
            catch (Exception e)
            {
                string msg = e.Message;
            }
        }

        public MySqlCommand CreateHistoryDataCommand()
        {
            string connectionString = new DBConnectionString().ToString();
            this.historyDataThreadConn = new MySqlConnection(connectionString);
            this.historyDataThreadConn.Open();
            return this.historyDataThreadConn.CreateCommand();
        }

        DataPacket GetDataPacket(string deviceKey, DateTime time)
        {
            return default(DataPacket);
        }

        private static string GetSelectStatement(string tableName, DateTime time)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} where time='{1}'";
            return string.Format(format, tableName, time.ToString());
        }

        private static string GetSelectStatement(string tableName, DateTime fromTime, DateTime toTime, bool sort = false)
        {
            // Get the recent <count> entries.
            string format = "select * from {0}  where time>='{1}' and time<='{2}'";
            if (sort)
            {
                format += " order by time DESC";
            }
            string sql = string.Format(format, tableName, fromTime, toTime);
            return sql;
        }

        public ReadResult GetData(string deviceKey, DateTime time, string code, Dictionary<string, object> data)
        {
            if (this.mainSqlCmd == null)
            {
                return ReadResult.SqlCommandError;
            }

            return GetData(this.mainSqlCmd, deviceKey, time, code, data);
        }

        public static ReadResult GetData(MySqlCommand command, string deviceKey, DateTime time, string code, Dictionary<string, object> data)
        {
            string tableName = Settings.Instance.GetTableName(deviceKey);

            command.CommandText = GetSelectStatement(tableName, time);
            try
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Must Has an Id.
                        string id = reader.GetString(Id);
                        id = id.Trim();

                        if (string.IsNullOrEmpty(id))
                        {
                            return ReadResult.RecordsWithoutId;
                        }

                        data.Add(Id, id);

                        List<Settings.DeviceCode> codes = Settings.Instance.GetCodes(deviceKey);

                        string dataTime = reader.GetString("Time");
                        data.Add("time", dataTime);
                        foreach (var c in codes)
                        {
                            if (!string.IsNullOrEmpty(code) && code != c.Code)
                            {
                                continue;
                            }
                            string field = c.Field.ToLower();
                            try
                            {
                                string v = reader.GetString(field);
                                data.Add(c.Code, v);
                            }
                            catch (SqlNullValueException)
                            {
                                // TODO: Has Null Value
                                data.Add(c.Code, string.Empty);
                            }
                            catch (Exception)
                            {
                                return ReadResult.NoThisField;
                            }
                        }
                        return ReadResult.ReadOK;
                    }
                    else
                    {
                        return ReadResult.NoDataFound;
                    }
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(500);
                return ReadResult.ReadDBException;
            }
        }

        public static ReadResult GetData(MySqlCommand command, string deviceKey, DateTime startTime, DateTime stopTime, string code, List<Dictionary<string, object>> data)
        {
            string tableName = Settings.Instance.GetTableName(deviceKey);

            command.CommandText = GetSelectStatement(tableName, startTime, stopTime);

            try
            {
                data.Clear();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        // Must Has an Id.
                        string id = reader.GetString(Id);
                        id = id.Trim();

                        if (string.IsNullOrEmpty(id))
                        {
                            return ReadResult.RecordsWithoutId;
                        }

                        item.Add(Id, id);

                        List<Settings.DeviceCode> codes = Settings.Instance.GetCodes(deviceKey);

                        string dataTime = reader.GetString("Time");
                        item.Add("time", dataTime);
                        foreach (var c in codes)
                        {
                            if (!string.IsNullOrEmpty(code) && code != c.Code)
                            {
                                continue;
                            }
                            string field = c.Field.ToLower();
                            try
                            {
                                string v = reader.GetString(field);
                                item.Add(c.Code, v);
                            }
                            catch (SqlNullValueException)
                            {
                                // TODO: Has Null Value
                                item.Add(c.Code, string.Empty);
                            }
                            catch (Exception)
                            {
                                return ReadResult.NoThisField;
                            }
                        }

                        data.Add(item);
                    }// While

                    if (data.Count == 0)
                    {
                        return ReadResult.NoDataFound;
                    }
                    return ReadResult.ReadOK;
                }
            }
            catch (IOException e)
            {
                return ReadResult.ReadIOException;
            }
            catch (SqlException e)
            {
                return ReadResult.ReadSqlException;
            }
            catch (InvalidOperationException e)
            {
                return ReadResult.ReadInvalidOpException;
            }
            catch (Exception e)
            {
                return ReadResult.ReadDBException;
            }
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
            else
            {
                Log.GetLogFile("scada.naidevice").Log(string.Format("{0} Not_Found", filePath));
            }
            
            return content;
        }

        private string GetFileName(DateTime time)
        {
            int minuteAdjust = Settings.Instance.MinuteAdjust;
            string deviceSn = Settings.Instance.NaIDeviceSn;
            string fileName;
            DateTime t = time;
            fileName = string.Format("{0}_{1}-{2:D2}-{3:D2}T{4:D2}_{5:D2}_00-5min.n42",
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

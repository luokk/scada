
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

        private const string Time = "time";

        private MySqlConnection mainThreadConn = null;

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

        public MySqlConnection CreateMySqlConnection()
        {
            string connectionString = new DBConnectionString().ToString();
            return new MySqlConnection(connectionString);
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

        private static string GetSelectStatement(string tableName, DateTime fromTime, DateTime toTime, string sid, bool sort = false)
        {
            // Get the recent <count> entries.
            string format = "select * from {0}  where time>='{1}' and time<='{2}'";
            if (!string.IsNullOrEmpty(sid))
            {
                format += " and sid='" + sid + "'";
            }

            if (sort)
            {
                format += " order by time DESC";
            }
            string sql = string.Format(format, tableName, fromTime, toTime);
            return sql;
        }

        public static ReadResult GetData(MySqlCommand command, string deviceKey, DateTime startTime, DateTime stopTime, string sid, string code, List<Dictionary<string, object>> data, out string errorMessage)
        {
            errorMessage = string.Empty;
            string tableName = Settings.Instance.GetTableName(deviceKey);

            if (stopTime != default(DateTime))
            {
                command.CommandText = GetSelectStatement(tableName, startTime, stopTime, sid);
            }
            else
            {
                command.CommandText = GetSelectStatement(tableName, startTime);
            }

            try
            {
                data.Clear();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>(12);

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
                            catch (Exception e)
                            {
                                errorMessage = e.Message;
                                return ReadResult.NoThisField;
                            }
                        }

                        data.Add(item);
                    }// While

                    if (data.Count == 0)
                    {
                        errorMessage = "data.Count == 0";
                        return ReadResult.NoDataFound;
                    }
                    return ReadResult.ReadOK;
                }
            }
            catch (IOException e)
            {
                errorMessage = e.Message;
                return ReadResult.ReadIOException;
            }
            catch (SqlException e)
            {
                errorMessage = e.Message;
                return ReadResult.ReadSqlException;
            }
            catch (InvalidOperationException e)
            {
                errorMessage = e.Message;
                return ReadResult.ReadInvalidOpException;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
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
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader fs = new StreamReader(filePath))
                    {
                        content = fs.ReadToEnd();
                        return content;
                    }
                }
                else
                {
                    Log.GetLogFile("scada.naidevice").Log(string.Format("{0} Not_Found", filePath));
                }
            }
            catch (Exception)
            {
                return "";
            }
            
            return content;
        }

        private string GetFileName(DateTime time)
        {
            // int minuteAdjust = Settings.Instance.MinuteAdjust;
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

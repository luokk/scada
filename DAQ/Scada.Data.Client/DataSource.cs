
namespace Scada.Data.Client
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
        FieldNotFound,
        UnknownReadDataError,
        ReadDataOK,
        DataNotFound,
        ReadDataIOError,
        ReadDataSqlError,
        ReadDataInvalidOperation,
    }

    public enum RangeType
    {
        OpenClose,
        CloseOpen,
        OpenOpen,
        CloseClose
    }

    /// <summary>
    /// Each Device has a Listener.
    /// </summary>
    internal class DataSource
    {
        private const int MaxCountFetchRecent = 10;

        private const string Time = "time";

        private List<string> tables = new List<string>();

        private List<string> deviceKeyList = new List<string>();

        //private int minuteAdjustForNaI = 0;

        // <DeviceKey, dict[data]>
        private Dictionary<string, object> latestData = new Dictionary<string, object>();

        private static DataSource instance = null;

        public static DataSource Instance
        {
            get
            {
                if (DataSource.instance == null)
                {
                    DataSource.instance = new DataSource();
                }
                return DataSource.instance;
            }
        }

        internal MySqlConnection GetDBConnection()
        {
            string connectionString = new DBConnectionString().ToString();
            var conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        Packet GetDataPacket(string deviceKey, DateTime time)
        {
            return default(Packet);
        }

        private static string GetSelectStatement(string tableName, DateTime time)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} where time='{1}'";
            return string.Format(format, tableName, time.ToString());
        }

        private static string GetSelectStatement(string tableName, DateTime fromTime, DateTime toTime, RangeType rangeType)
        {
            string format = "select * from {0} where time>'{1}' and time<='{2}'";
            if (rangeType == RangeType.CloseOpen)
            {
                // Default
                format = "select * from {0} where time>'{1}' and time<='{2}'";
            }
            if (rangeType == RangeType.OpenClose)
            {
                format = "select * from {0} where time>='{1}' and time<'{2}'";
            }
            string sql = string.Format(format, tableName, fromTime, toTime);
            return sql;
        }

        // Not care PolId in HTTP uploading.
        // SELECT * from <DEVICE TABLE> where time in (t1, t2]
        public static ReadResult GetData(MySqlCommand command, string deviceKey, DateTime time1, DateTime time2, RangeType rangeType, List<Dictionary<string, object>> data, out string errorMsg)
        {
            errorMsg = string.Empty;
            string tableName = Settings.Instance.GetTableName(deviceKey);

            if (time2 == default(DateTime))
            {
                command.CommandText = GetSelectStatement(tableName, time1);
            }
            else
            {
                command.CommandText = GetSelectStatement(tableName, time1, time2, rangeType);
            }

            try
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    data.Clear();
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>(20);

                        List<Settings.DeviceCode> codes = Settings.Instance.GetCodes(deviceKey);

                        string dataTime = reader.GetString("Time");
                        item.Add("time", dataTime);
                        foreach (var c in codes)
                        {
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
                                errorMsg = e.Message;
                                return ReadResult.FieldNotFound;
                            }
                        }

                        data.Add(item);
                    }

                    if (data.Count == 0)
                    {
                        errorMsg = string.Format("Data Not Found ({0}: {1} ~ {2})", deviceKey, time1, time2);
                        return ReadResult.DataNotFound;
                    }
                    return ReadResult.ReadDataOK;
                }
            }
            catch (IOException e)
            {
                errorMsg = e.Message;
                return ReadResult.ReadDataIOError;
            }
            catch (SqlException e)
            {
                errorMsg = e.Message;
                return ReadResult.ReadDataSqlError;
            }
            catch (InvalidOperationException e)
            {
                errorMsg = e.Message;
                return ReadResult.ReadDataInvalidOperation;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return ReadResult.UnknownReadDataError;
            }
        }

        public static string GetDatePath(DateTime date)
        {
            return string.Format("{0}-{1:D2}", date.Year, date.Month);
        }

        // NO USE for NaI device.
        public string GetNaIDeviceData(DateTime time)
        {
            string fileName = this.GetLabrFileName(time);
            string datePath = GetDatePath(time);
            string filePath = LogPath.GetDeviceLogFilePath("scada.naidevice", time) + "\\" + fileName;
            string content = string.Empty;
            if (File.Exists(filePath))
            {
                StreamReader fs = new StreamReader(filePath);
                content = fs.ReadToEnd();
            }
            else
            {
                // TODO: fix here, For second agent process, I disbale this log for temp.
                // Log.GetLogFile("scada.naidevice").Log(string.Format("{0} Not_Found", filePath));
            }

            return content;
        }

        public string GetLabrDeviceFile(DateTime time)
        {
            string fileName = this.GetLabrFileName(time);
            string datePath = GetDatePath(time);
            string filePath = LogPath.GetDeviceLogFilePath(Devices.Labr, time) + "\\" + time.Day.ToString() + "\\" + fileName;
            string content = string.Empty;
            if (File.Exists(filePath))
            {
                return filePath;
            }
            return null;
        }

        private string GetLabrFileName(DateTime time)
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

        internal static string GetCurrentSid()
        {
            string path = LogPath.GetDeviceLogFilePath("scada.hpge");
            string sidFile = Path.Combine(path, "SID");
            string sid = string.Empty;
            if (!File.Exists(sidFile))
            {
                return string.Empty;
            }
            using (FileStream fs = File.OpenRead(sidFile))
            {
                long len = fs.Length;
                byte[] bs = new byte[len];
                fs.Read(bs, 0, (int)len);
                sid = Encoding.ASCII.GetString(bs);
            }
            return sid;
        }

        internal string GetNewHpGeFile(string sid = null)
        {
            string path = LogPath.GetDeviceLogFilePath("scada.hpge");
            if (string.IsNullOrEmpty(sid))
            {
                sid = GetCurrentSid();
            }

            string currentFilePath = Path.Combine(path, sid);

            if (Directory.Exists(currentFilePath))
            {
                string[] files = Directory.GetFiles(currentFilePath);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("!"))
                        return file;
                }
                return string.Empty;
            }
            else { return string.Empty; }
        }
    }
}

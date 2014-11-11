
namespace Scada.MainVision
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Scada.Controls;
    using Scada.Controls.Data;
    using MySql.Data.MySqlClient;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Reflection;
    using Scada.Config;
    using System.Windows.Forms;

    /// <summary>
    /// Each Device has a Listener.
    /// </summary>
    public class DBDataProvider
    {
        private string ConnectionString;

        private const int MaxCountFetchRecent = 10;

        private const string Id = "Id";

        private const string Time = "time";

        public bool Quit { get; set; }

        public string CurrentDeviceKey { set; get; }

        public static DBDataProvider Instance
        {
            get;
            set;
        }


        private List<string> allDeviceKeys = new List<string>();

        private List<string> deviceKeyList = new List<string>();


        private Dictionary<string, DBDataCommonListerner> dataListeners;

        // ?? What's filter.
        private Dictionary<string, object> filters = new Dictionary<string, object>(10);


        // ?
        // private Dictionary<string, object> dataCache = new Dictionary<string, object>();


        // <DeviceKey, dict[data]>
        private Dictionary<string, object> latestData = new Dictionary<string, object>();

        /// <summary>
        /// 
        /// </summary>
        static DBDataProvider()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public DBDataProvider()
        {
            if (Settings.Instance.IsCAS)
            {
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Hpic);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Labr);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Shelter);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Weather);
            }
            else
            {
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Hpic);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Dwd);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_MDS);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_AIS);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_NaI);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Shelter);
                this.allDeviceKeys.Add(DataProvider.DeviceKey_Weather);
            }

            this.FetchCount = 26;

            this.dataListeners = new Dictionary<string, DBDataCommonListerner>(30);

            // 192.168.1.24
            // this.timelineSource = new List<Dictionary<string, object>>();
        }

        public MySqlConnection GetMySqlConnection()
        {
            string installPath = Assembly.GetExecutingAssembly().Location;
            string fileName = string.Format("{0}\\..\\local.ip", installPath);

            var s = new DBConnectionString();
            s.Address = "127.0.0.1";
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string ip = sr.ReadLine();
                    if (ip != null && ip.Length > 0)
                    {
                        s.Address = ip;
                    }
                }
            }
            this.ConnectionString = s.ToString();
            var conn = new MySqlConnection(this.ConnectionString);

            if (conn != null)
            {
                try
                {
                    conn.Open();
                    return conn;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }
            }
            return null;
        }

        public DataListener GetDataListener(string deviceKey)
        {
            deviceKey = deviceKey.ToLower();
            this.deviceKeyList.Add(deviceKey);
            if (this.dataListeners.ContainsKey(deviceKey))
            {
                return this.dataListeners[deviceKey];
            }
            else
            {
                DBDataCommonListerner listener = new DBDataCommonListerner(deviceKey);
                this.dataListeners.Add(deviceKey, listener);
                return listener;
            }

        }

        public void RemoveDataListener(string deviceKey)
        {
            deviceKey = deviceKey.ToLower();
            if (this.deviceKeyList.Contains(deviceKey))
            {
                this.deviceKeyList.Remove(deviceKey);
            }
        }

        // For DevicePage.
        // Get Latest data ( 1 Entry ),
        public void RefreshTimeNow(MySqlCommand cmd)
        {
            this.latestData.Clear();
            foreach (var item in this.allDeviceKeys)
            {
                string deviceKey = item.ToLower();
                // Would use listener to notify, panel would get the lastest data.
                var data = this.RefreshTimeNow(deviceKey, cmd);
                if (data != null)
                {
                    this.latestData.Add(deviceKey, data);

                    if (this.dataListeners.ContainsKey(deviceKey))
                    {
                        DBDataCommonListerner listener = this.dataListeners[deviceKey];
                        if (listener != null)
                        {
                            listener.OnDataArrival(DataArrivalConfig.TimeNew, data);
                        }
                    }
                }
            }
        }

        public int FetchCount
        {
            get;
            set;
        }

        // Get time-range data,
        // Notify with all the result.
        public List<Dictionary<string, object>> RefreshTimeRange(string deviceKey, DateTime beginTime, DateTime endTime, DateTime fromTime, DateTime toTime, MySqlCommand cmd)
        {
            try
            {
                var result = this.Refresh(deviceKey, false, -1, beginTime, endTime, fromTime, toTime, cmd);
                result.Reverse();
                return result;
            }
            catch (Exception)
            {

            }

            return new List<Dictionary<string, object>>();
        }

        public List<Dictionary<string, object>> RefreshTimeRange2(string deviceKey, DateTime fromTime, DateTime toTime, MySqlCommand cmd)
        {
            try
            {
                var result = this.Refresh(deviceKey, false, -1, fromTime, toTime, fromTime, toTime, cmd);
                result.Reverse();
                return result;
            }
            catch (Exception)
            {

            }

            return new List<Dictionary<string, object>>();
        }

        public Dictionary<string, object> RefreshTimeNow(string deviceKey, MySqlCommand cmd)
        {
            // Return values
            const int MaxItemCount = 20;
            var ret = new Dictionary<string, object>(MaxItemCount);

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];
            cmd.CommandText = this.GetSelectStatement(entry.TableName, 1);

            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return ret;
                    foreach (var i in entry.ConfigItems)
                    {
                        string key = i.Key.ToLower();
                        if (deviceKey == DataProvider.DeviceKey_NaI && key.IndexOf('-') > 0)
                        {
                            continue;
                        }
            
                        try
                        {
                            int index = reader.GetOrdinal(key);
                            if (!reader.IsDBNull(index))
                            {
                                string v = reader.GetString(index);
                                if (key == "doserate")
                                {
                                    double dv;
                                    if (double.TryParse(v, out dv))
                                    {
                                        ret.Add(key, dv.ToString("0.0"));
                                    }
                                    else
                                    {
                                        ret.Add(key, v);
                                    }
                                }
                                else if (key == "nuclidefound")
                                {
                                    ret.Add(key, v == "1" ? "发现" : "未发现");
                                }
                                else if (key.IndexOf("alarm") >= 0 || key == "direction")
                                {
                                    ret.Add(key, v);
                                }
                                else
                                {
                                    double d;
                                    if (double.TryParse(v, out d))
                                    {
                                        ret.Add(key, d.ToString("0.0"));
                                    }
                                    else
                                    {
                                        ret.Add(key, v);
                                    }
                                }
                            }
                        }
                        catch (SqlNullValueException)
                        {
                            // TODO: Has Null Value
                            ret.Add(key, null);
                        }
                        catch (Exception)
                        {
                            // No this field.
                        }
                    }

                    if (entry.DataFilter != null)
                    {
                        entry.DataFilter.Fill(ret);
                    }
                }
            }
            catch (Exception)
            {
            }

            
            if (deviceKey == "scada.weather")
            {
                var d = new List<Dictionary<string, object>>();
                d.Add(ret);
                ReviseIfRainForWeather(cmd, d);
                return d[0];
            }
            return ret;
        }


        /// <summary>
        /// Implements 
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <param name="current"></param>
        /// <param name="count"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private List<Dictionary<string, object>> Refresh(string deviceKey, bool current, int count, DateTime beginTime, DateTime endTime, DateTime fromTime, DateTime toTime, MySqlCommand cmd)
        {
            // Return values
            var ret = new List<Dictionary<string, object>>();

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];
            if (current)
            {
                // Dead Branch
                cmd.CommandText = this.GetSelectStatement(entry.TableName, count);
            }
            else
            {
                cmd.CommandText = this.GetSelectStatement(entry.TableName, beginTime, endTime, fromTime, toTime);
            }

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                int index = 0;

                List<string> keys = new List<string>();
                foreach (var i in entry.ConfigItems)
                {
                    string key = i.Key.ToLower();
                    if (deviceKey == DataProvider.DeviceKey_NaI && key.IndexOf('-') > 0)
                    {
                        continue;
                    }

                    keys.Add(key);
                }

                if (deviceKey == DataProvider.DeviceKey_Hpic || deviceKey == DataProvider.DeviceKey_Weather)
                {
                    keys.Add("ifrain");
                }

                string[] keyArray = keys.ToArray();

                Dictionary<string, object> data = null;// new Dictionary<string, object>(10);
                while (reader.Read())
                {
                    data = new Dictionary<string, object>(keyArray.Length);
                    foreach (var key in keyArray)
                    {
                        try
                        {
                            // -----------------------------
                            int i = reader.GetOrdinal(key);
                            if (!reader.IsDBNull(i))
                            {
                                string v = reader.GetString(i);
                                if (key == "ifrain")
                                {
                                    if (v == "0")
                                    {
                                        data.Add(key, false);
                                    }
                                    else
                                    {
                                        data.Add(key, true);
                                    }
                                }
                                else if (key == "doserate")
                                {
                                    double dv;
                                    if (double.TryParse(v, out dv))
                                    {
                                        data.Add(key, dv.ToString("0.0"));
                                    }
                                    else
                                    {
                                        data.Add(key, v);
                                    }
                                }
                                else if (key == "nuclidefound")
                                {
                                    data.Add(key, v == "1" ? "发现" : "未发现");
                                }
                                else if (key.IndexOf("alarm") >= 0)
                                {
                                    data.Add(key, v);
                                }
                                else
                                {
                                    double d;
                                    if (double.TryParse(v, out d))
                                    {
                                        data.Add(key, d.ToString("0.0"));
                                    }
                                    else
                                    {
                                        data.Add(key, d.ToString(v));
                                    }
                                }
                            }
                            else
                            {
                                if (key == "ifrain")
                                {
                                    data.Add(key, false);
                                }
                            }
                        }
                        catch (SqlNullValueException)
                        {
                            // TODO: Has Null Value
                            if (!data.ContainsKey(key))
                            {
                                data.Add(key, string.Empty);
                            }
                        }
                        catch (Exception e)
                        {
                            // break;
                            // No this field.
                        }
                    }

                    /*
                    if (entry.DataFilter != null)
                    {
                        entry.DataFilter.Fill(data);
                    }
                    */
                    ret.Add(data);

                    index++;
                }
            }

            return ret;
        }

        private void AddRainStatus(List<Dictionary<string, object>> ret, string time, string ifRain)
        {
            foreach (var i in ret)
            {
                if ((string)i["time"] == time)
                {
                    i.Add("ifrain", ifRain == "1");
                    return;
                }
            }

        }

        private void ReviseIfRainForWeather(MySqlCommand cmd, List<Dictionary<string, object>> ret)
        {
            foreach (var item in ret)
            {
                string time = (string)item["time"];
                cmd.CommandText = this.GetSelectStatement("RDSampler_rec", time);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    try
                    {
                        if (reader.Read())
                        {
                            item["ifrain"] = reader.GetString("IfRain");
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);

                    }
                }
            }
        }

        private string GetSelectStatement(string tableName, string time)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} where time='{1}'";
            return string.Format(format, tableName, time);
        }

        private string GetSelectStatement(string tableName, int count)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} order by Time DESC limit {1}";
            return string.Format(format, tableName, count);
        }

        private string GetSelectStatement(string tableName, DateTime beginTime, DateTime endTime, DateTime fromTime, DateTime toTime)
        {
            fromTime = beginTime;
            toTime = endTime;
            int days = (endTime - beginTime).Days;
            if (days <= 2)
            {
                if (tableName.ToLower().IndexOf("hpic") >= 0)
                {
                    string format = "select a.time, a.doserate, a.Highvoltage, a.Temperature, a.Battery, b.ifRain from {0} a left join RDSampler_rec b on a.time=b.time where a.time<'{1}' and a.time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                if (tableName.ToLower().IndexOf("weather") >= 0)
                {
                    string format = "select a.time, a.Windspeed, a.Direction, a.Temperature, a.Humidity, a.Pressure, a.Raingauge, a.Rainspeed, a.Dewpoint, b.ifRain from {0} a left join RDSampler_rec b on a.time=b.time where a.time<'{1}' and a.time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
            }
            else
            {
                int interval = 300;
                if (days > 7)
                    interval = 3600;
                string timeIndex = string.Format("FROM_UNIXTIME( CEILING(UNIX_TIMESTAMP(a.time) / {0}) * {1}) as time", interval, interval);
                // Too many days!
                if (tableName.ToLower().IndexOf("hpic") >= 0)
                {
                    string format = "select {0}, avg(a.doserate) as doserate, avg(a.Highvoltage) as Highvoltage, avg(a.Temperature) as Temperature, avg(a.Battery) as Battery, avg(b.ifRain) as ifRain from {1} a left join RDSampler_rec b on a.time=b.time where a.time<'{2}' and a.time>'{3}' group by time order by Time DESC";
                    string sql = string.Format(format, timeIndex, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("weather") >= 0)
                {
                    //string format = "select a.time, a.Windspeed, a.Direction, a.Temperature, a.Humidity, a.Pressure, a.Raingauge, a.Rainspeed, a.Dewpoint, b.ifRain from {0} a left join RDSampler_rec b on a.time=b.time where a.time<'{1}' and a.time>'{2}' order by Time DESC";
                    string format = "select {0}, avg(a.Windspeed) as Windspeed, avg(a.Direction) as Direction, avg(a.Temperature) as Temperature, avg(a.Humidity) as Humidity, avg(a.Pressure) as Pressure, avg(a.Raingauge) as Raingauge, avg(a.Rainspeed) as Rainspeed, avg(a.Dewpoint) as Dewpoint, avg(b.ifRain) as ifRain from {1} a left join RDSampler_rec b on a.time=b.time where a.time<'{2}' and a.time>'{3}' group by time order by Time DESC";
                    string sql = string.Format(format, timeIndex, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("nai") >= 0)
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("mds") >= 0)
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("ais") >= 0)
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("environment") >= 0)
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else if (tableName.ToLower().IndexOf("rdsampler") >= 0)
                {
                    // Get the recent <count> entries.
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
                else
                {
                    string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
                    string sql = string.Format(format, tableName, toTime, fromTime);
                    return sql;
                }
            }
        }

        private DataListener GetDataListenerByTableName(string tableName)
        {
            if (!this.dataListeners.ContainsKey(tableName))
            {
                return null;
            }
            return this.dataListeners[tableName];
        }

        public Dictionary<string, object> GetLatestEntry(string deviceKey)
        {
            if (this.latestData.ContainsKey(deviceKey))
            {
                return (Dictionary<string, object>)this.latestData[deviceKey];
            }
            return null;
        }

        public static int DateTimeCompare(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            object t1 = a[Time];
            object t2 = b[Time];
            DateTime dt1 = DateTime.MinValue;
            DateTime dt2 = DateTime.MinValue;
            if (t1 != null)
            {
                if (!DateTime.TryParse((string)t1, out dt1))
                    return 1;
            }
            if (t2 != null)
            {
                if (!DateTime.TryParse((string)t2, out dt2))
                    return 1;
            }

            if (dt1 > dt2)
            {
                return -1;
            }
            return 1;
        }

        public string GetNaIDeviceChannelData(DateTime time, MySqlCommand cmd, out double a, out double b, out double c)
        {
            string sql = string.Format("select ChannelData, Coefficients from nai_rec where time='{0}'", time);
            cmd.CommandText = sql;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    string ret = reader.GetString(0);
                    string co = reader.GetString(1);
                    string[] d = co.Split(' ');
                    c = double.Parse(d[0]);
                    b = double.Parse(d[1]);
                    a = double.Parse(d[2]);
                    return ret;
                }
            }
            a = 0.0;
            b = 1.0;
            c = 0.0;
            return string.Empty;
        }

        internal List<Dictionary<string, object>> GetSidData(string p, DateTime dt1, DateTime dt2)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            using (var conn = this.GetMySqlConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    string tableName = (p == DataProvider.DeviceKey_MDS) ? "mds_rec" : "ais_rec";
                    
                    string sql = string.Format("select * from {0} where time>'{1}' and time<'{2}' and status=0", tableName, dt1, dt2);
                    cmd.CommandText = sql;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            var data = new Dictionary<string, object>();
                            data.Add("sid", reader.GetString("Sid"));
                            data.Add("begintime", reader.GetString("begintime"));
                            data.Add("endtime", reader.GetString("endtime"));
                            data.Add("volume", reader.GetString("volume"));
                            data.Add("hours", reader.GetString("hours"));
                            ret.Add(data);
                        }

                        
                    }
                }
            }
            return ret;
        }



        internal List<Dictionary<string, object>> GetDoorStatus(DateTime dt1, DateTime dt2)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            using (var conn = this.GetMySqlConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    int ifOpen = 1;

                    string beginTime = dt1.ToString();
                    string endTime = string.Empty;
                    while (true)
                    {
                        string sql = string.Format("select * from environment_rec where time>'{0}' and time<'{1}' and ifDoorOpen={2} limit 1", beginTime, dt2, ifOpen);
                        cmd.CommandText = sql;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var data = new Dictionary<string, object>();
                            if (!reader.Read())
                            {
                                break;
                            }


                            if (ifOpen == 0)
                            {
                                ifOpen = 1;
                                endTime = reader.GetString("Time");
                                ret.Add(new Dictionary<string, object>() 
                                {
                                    {"begintime", beginTime}, {"endtime", endTime}
                                }
                                );
                                beginTime = endTime;
                            }
                            else
                            {
                                ifOpen = 0;
                                beginTime = reader.GetString("Time");
                            }
                        }
                    }

                }
            }
            return ret;
        }
    }
}


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

    /// <summary>
    /// Each Device has a Listener.
    /// </summary>
    internal class DBDataProvider : DataProvider
    {
        private string ConnectionString;

        private const int MaxCountFetchRecent = 10;

        private const string Id = "Id";

        private const string Time = "time";

        private MySqlConnection conn = null;

        private MySqlCommand cmd = null;

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
        private Dictionary<string, object> dataCache = new Dictionary<string, object>();


        // <DeviceKey, dict[data]>
        private Dictionary<string, object> latestData = new Dictionary<string, object>();


        private List<Dictionary<string, object>> timelineSource;
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
            this.allDeviceKeys.Add(DeviceKey_Hpic);
            this.allDeviceKeys.Add(DeviceKey_Dwd);
            this.allDeviceKeys.Add(DeviceKey_HvSampler);
            this.allDeviceKeys.Add(DeviceKey_ISampler);
            this.allDeviceKeys.Add(DeviceKey_NaI);
            this.allDeviceKeys.Add(DeviceKey_Shelter);
            this.allDeviceKeys.Add(DeviceKey_Weather);

            this.FetchCount = 26;

            this.dataListeners = new Dictionary<string, DBDataCommonListerner>(30);

            // 192.168.1.24
            this.timelineSource = new List<Dictionary<string, object>>();
        }

        public MySqlCommand GetMySqlCommand()
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
            this.conn = new MySqlConnection(this.ConnectionString);

            if (this.conn != null)
            {
                try
                {
                    this.conn.Open();
                    MySqlCommand cmd = this.conn.CreateCommand();
                    return cmd;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }
            }
            return null;
        }

        public override DataListener GetDataListener(string deviceKey)
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

        public override void RemoveDataListener(string deviceKey)
        {
            deviceKey = deviceKey.ToLower();
            if (this.deviceKeyList.Contains(deviceKey))
            {
                this.deviceKeyList.Remove(deviceKey);
            }
        }

        public override void RemoveFilters()
        {
            this.filters.Clear();
        }

        public override void SetFilter(string key, object value)
        {
            if (!this.filters.ContainsKey(key))
            {
                this.filters.Add(key, value);
            }
        }

        // For Panels.
        // Get Latest data,
        // No Notify.
        public override void RefreshTimeNow()
        {
            this.latestData.Clear();
            foreach (var item in this.allDeviceKeys)
            {
                string deviceKey = item.ToLower();
                // Would use listener to notify, panel would get the lastest data.
                var data = this.RefreshTimeNow(deviceKey);
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

        // Get Recent data
        // Notify the new 
        public override void RefreshTimeline(string deviceKey)
        {
            DBDataCommonListerner listener = this.dataListeners[deviceKey];
            if (listener == null)
            {
                return;
            }

            var result = this.Refresh(deviceKey, true, this.FetchCount, DateTime.MinValue, DateTime.MinValue);
            if (result == null)
            {
                return;
            }

            result.Sort(DBDataProvider.DateTimeCompare);

            listener.OnDataArrivalBegin(DataArrivalConfig.TimeRecent);
            foreach (var data in result)
            {
                listener.OnDataArrival(DataArrivalConfig.TimeRecent, data);
            }
            listener.OnDataArrivalEnd(DataArrivalConfig.TimeRecent);
        }

        // Get time-range data,
        // Notify with all the result.
        public override List<Dictionary<string, object>> RefreshTimeRange(string deviceKey, DateTime fromTime, DateTime toTime)
        {
            try
            {
                var result = this.Refresh(deviceKey, false, -1, fromTime, toTime);
                result.Reverse();
                return result;
            }
            catch (Exception)
            {

            }

            return new List<Dictionary<string, object>>();
        }

        private Dictionary<string, object> RefreshTimeNow(string deviceKey)
        {
            if (this.cmd == null)
            {
                this.cmd = this.GetMySqlCommand();
            }
            // Return values
            const int MaxItemCount = 20;
            var ret = new Dictionary<string, object>(MaxItemCount);

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];
            this.cmd.CommandText = this.GetSelectStatement(entry.TableName, 1);

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

                        foreach (var i in entry.ConfigItems)
                        {
                            string key = i.Key.ToLower();
                            try
                            {
                                string v = reader.GetString(key);
                                ret.Add(key, v);
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
            }
            catch (Exception)
            {
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
        private List<Dictionary<string, object>> Refresh(string deviceKey, bool current, int count, DateTime fromTime, DateTime toTime)
        {
            if (this.cmd == null)
            {
                this.cmd = this.GetMySqlCommand();
            }

            // Return values
            var ret = new List<Dictionary<string, object>>();

            Config cfg = Config.Instance();
            ConfigEntry entry = cfg[deviceKey];
            if (current)
            {
                this.cmd.CommandText = this.GetSelectStatement(entry.TableName, count);
            }
            else
            {
                this.cmd.CommandText = this.GetSelectStatement(entry.TableName, fromTime, toTime);
            }
            using (MySqlDataReader reader = this.cmd.ExecuteReader())
            {
                int index = 0;
                while (reader.Read())
                {
                    Dictionary<string, object> data = new Dictionary<string, object>(10);
                    foreach (var i in entry.ConfigItems)
                    {
                        string key = i.Key.ToLower();
                        try
                        {
                            string v = reader.GetString(key);
                            data.Add(key, v);
                        }
                        catch (SqlNullValueException)
                        {
                            // TODO: Has Null Value
                            if (!data.ContainsKey(key))
                            {
                                data.Add(key, string.Empty);
                            }
                        }
                        catch (Exception)
                        {
                            // No this field.
                        }
                    }

                    if (entry.DataFilter != null)
                    {
                        entry.DataFilter.Fill(data);
                    }
                    ret.Add(data);

                    index++;
                }
            }

            return ret;
        }

        private string GetSelectStatement(string tableName, int count)
        {
            // Get the recent <count> entries.
            string format = "select * from {0} order by Time DESC limit {1}";
            return string.Format(format, tableName, count);
        }

        private string GetSelectStatement(string tableName, DateTime fromTime, DateTime toTime)
        {
            // Get the recent <count> entries.
            string format = "select * from {0}  where time<'{1}' and time>'{2}' order by Time DESC";
            string sql = string.Format(format, tableName, toTime, fromTime);
            return sql;
        }

        private DataListener GetDataListenerByTableName(string tableName)
        {
            if (!this.dataListeners.ContainsKey(tableName))
            {
                return null;
            }
            return this.dataListeners[tableName];
        }

        public override Dictionary<string, object> GetLatestEntry(string deviceKey)
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

        public string GetNaIDeviceChannelData(DateTime time)
        {
            if (this.cmd == null)
            {
                this.cmd = this.GetMySqlCommand();
            }

            string sql = string.Format("select ChannelData from nai_rec where time='{0}'", time);
            this.cmd.CommandText = sql;

            using (MySqlDataReader reader = this.cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    string ret = reader.GetString(0);
                    return ret;
                }
            }
            return string.Empty;
        }
    }
}

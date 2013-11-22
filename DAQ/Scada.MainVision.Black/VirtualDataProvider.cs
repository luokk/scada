
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Controls.Data;
using System.IO;

namespace Scada.MainVision
{
	internal class VirtualDataProvider : DataProvider
	{
		private DBDataCommonListerner dataListener;

        private List<string> deviceKeyList = new List<string>();

        private Dictionary<string, DBDataCommonListerner> dataListeners = new Dictionary<string, DBDataCommonListerner>();

        private string dataProviderFile = "HPIC.log";

        private List<Dictionary<string, object>> dataPool = new List<Dictionary<string, object>>();

        private List<string> lists = new List<string>();

        int index = 0;

		public VirtualDataProvider()
		{
            deviceKeyList.Add("scada.hpic");

            using (FileStream fileStream = File.OpenRead(dataProviderFile))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string line = sr.ReadLine();
                    int index = 0;
                    while (line != null)
                    {
                        line = line.Trim();
                        if (line.Length > 0)
                        {
                            lists.Add(line);
                            index++;
                        }
                        // Next line;
                        line = sr.ReadLine();
                    }
                }
            }

            // this.dataListeners.Add("scada.hpic", new DBDataCommonListerner("scada.hpic"));
		}

        /*
		public override void Refresh()
		{
            bool show = false;
            foreach (var item in this.deviceKeyList)
            {
                if (show)
                {
                    continue;
                }
                show = true;
                string deviceKey = item.ToLower();
                if (!this.dataListeners.ContainsKey(deviceKey))
                {
                    continue;
                }
                this.Refresh(deviceKey);
            }
		}
        */

        // Get DataArrivalConfig.TimeRecent Data.
        public override void RefreshTimeline(string deviceKey)
        {
            DBDataCommonListerner listener = this.dataListeners[deviceKey];
            if (listener != null)
            {

                // int count = MaxCountFetchRecent;
                Config cfg = Config.Instance();
                ConfigEntry entry = cfg[deviceKey];

                listener.OnDataArrivalBegin(DataArrivalConfig.TimeRecent);

                string line = lists[this.index];

                if (line.Length > 0)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>(10);
                    data.Clear();
                    ParseLine(line, entry, data);
                    listener.OnDataArrival(DataArrivalConfig.TimeRecent, data);
                    this.index++;
                }
                listener.OnDataArrivalEnd(DataArrivalConfig.TimeRecent);
            }
        }


        private void ParseLine(string line, ConfigEntry entry, Dictionary<string, object> data)
        {
            string[] a = line.Split(' ');
            foreach (var i in entry.ConfigItems)
            {
                if (i.FieldIndex > 1)
                {
                    string v = a[i.FieldIndex + 1];
                    data.Add(i.Key, v);
                }
                else if (i.FieldIndex == 1)
                {
                    string v = a[1] + " " + a[2];
                    data.Add(i.Key, v);
                }

            }

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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override void RemoveDataListener(string tableName)
        {
            // Do nothing.
        }

        public override void RemoveFilters()
        {
            throw new NotImplementedException();
        }

        public override void SetFilter(string key, object value)
        {
            throw new NotImplementedException();
        }

        public override void RefreshTimeNow()
        {
            throw new NotImplementedException();
        }

        public override List<Dictionary<string, object>> RefreshTimeRange(string deviceKey, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, object> GetLatestData(string deviceKey)
        {
            throw new NotImplementedException();
        }
    }
}

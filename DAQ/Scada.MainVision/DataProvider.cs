
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Controls.Data;

namespace Scada.MainVision
{
	public abstract class DataProvider
	{

        public const string DeviceKey_Hpic = "scada.hpic";

        public const string DeviceKey_Weather = "scada.weather";

        public const string DeviceKey_HvSampler = "scada.hvsampler";

        public const string DeviceKey_ISampler = "scada.isampler";

        public const string DeviceKey_Shelter = "scada.shelter";

        public const string DeviceKey_Dwd = "scada.dwd";

        public const string DeviceKey_NaI = "scada.naidevice";

        /// <summary>
        /// 
        /// </summary>
        public abstract void RefreshTimeNow();

        public abstract List<Dictionary<string, object>> RefreshTimeRange(string deviceKey, DateTime from, DateTime to);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceKey"></param>
        public abstract void RefreshTimeline(string deviceKey);

        public string CurrentDeviceKey { set; get; }

        public abstract Dictionary<string, object> GetLatestEntry(string deviceKey);

        public abstract void RemoveFilters();

        public abstract void SetFilter(string key, object value);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public abstract DataListener GetDataListener(string tableName);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract void RemoveDataListener(string tableName);

        public bool Quit { get; set; }
    }
}

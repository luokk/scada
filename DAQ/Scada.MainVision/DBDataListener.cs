using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Controls.Data
{
    public enum DataArrivalConfig
    {
        TimeNew,
        TimeRecent,
        TimeRange,
        TimeCurrent
    }

	public delegate void OnDataArrivalBegin(DataArrivalConfig config);

	public delegate void OnDataArrival(DataArrivalConfig config, Dictionary<string, object> data);

    public delegate void OnDataArrivalEnd(DataArrivalConfig config);

    public class ColumnInfo
    {
        public ColumnInfo()
        {
            this.Width = 160;
        }

        public string Header
        {
            get;
            set;
        }

        public string BindingName
        {
            get;
            set;
        }

        public double Width
        {
            get;
            set;
        }

        public bool DisplayInChart
        {
            get;
            set;
        }
    }

	public abstract class DataListener
	{
		public DataListener()
		{

		}

        public string DeviceKey
        {
            get;
            set;
        }

		public OnDataArrival OnDataArrival
		{
			get;
			set;
		}

		public OnDataArrivalBegin OnDataArrivalBegin
		{ 
			get;
			set;
		}

		public OnDataArrivalEnd OnDataArrivalEnd
		{
			get;
			set;
		}

        public abstract List<ColumnInfo> GetColumnsInfo();

    }
}

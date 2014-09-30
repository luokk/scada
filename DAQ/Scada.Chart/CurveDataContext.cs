using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Scada.Chart
{
    public enum UpdateResult
    {
        None,
        Overflow
    }

    public delegate void UpdateView();

    public delegate void AddCurvePoint(DateTime time, double value);

    public delegate UpdateResult UpdateCurve(Point point);

    public delegate void ClearCurve();

    // Class stands for DataSourcr
    public class CurveDataContext
    {
        public List<Point> points = new List<Point>();

        public event UpdateView UpdateView;

        public event UpdateCurve UpdateCurve;

        public event ClearCurve ClearCurve;

        public event AddCurvePoint AddCurvePoint;

        public CurveDataContext(string curveName)
        {
            this.CurveName = curveName;
        }

        public string CurveName
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public void UpdateCurves()
        {
            this.UpdateView();
        }

        public void SetDataSource(List<Dictionary<string, object>> data, string timeKey = "time")
        {
            
        }

        public void AppendData(Dictionary<string, object> item, string timeKey = "time")
        {

        }

        public UpdateResult AddPoint(DateTime time, double value, double graduation)
        {
            int index = this.GetIndexByTime(time);
            double x = index * graduation;
            double y = value;
            var p = new Point(x, y);
            this.points.Add(p);
            UpdateResult result = this.UpdateCurve(p);
            return result;
        }

        private int i = 0;

        private int GetIndexByTime(DateTime time)
        {
            return this.i++;
        }

        public void Clear()
        {
            this.points.Clear();
            this.ClearCurve();
        }
    }
}

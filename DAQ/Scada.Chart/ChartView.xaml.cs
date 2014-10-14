using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scada.Chart
{
    /// <summary>
    /// Interaction logic for ChartView.xaml
    /// </summary>
    public partial class ChartView : UserControl
    {
        public const double ViewGap = 10.0;

        // public const double Graduation = 8.0;

        public const double Offset = 8.0;

        public const double CommonGraduation = 4.0;

        struct GraduationLine
        {
            public Line Line
            {
                get;
                set;
            }

            public double Pos
            {
                get;
                set;
            }
        }

        struct GraduationTime
        {
            public TextBlock Text
            {
                get;
                set;
            }

            public double Pos
            {
                get;
                set;
            }
        }

        // private CurveView curveView;

        private double currentGraduation = 0.0;

        private int currentGraduationCount = 0;

        private bool initialized = false;

        private DateTime currentBaseTime = default(DateTime);

        private Dictionary<int, GraduationLine> Graduations
        {
            get;
            set;
        }

        public ChartView()
        {
            InitializeComponent();
            this.Graduations = new Dictionary<int, GraduationLine>();
            this.CurveView.ChartView = this;
        }

        public static readonly DependencyProperty TimeScaleProperty = DependencyProperty.Register("TimeScale", typeof(long), typeof(ChartView));
        private CurveDataContext curveDataContext;

        private DateTime GetBaseTime(DateTime startTime)
        {
            // 目前只支持30秒 和 5分钟两种间隔
            // Debug.Assert(this.Interval == 30 || this.Interval == 60 * 5 || this.Interval == 0);

            DateTime baseTime = default(DateTime);
            if (this.Interval == 30)
            {
                int second = startTime.Second / 30 * 30;
                baseTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, second);
            }
            else if (this.Interval == 60 * 5)
            {
                int min = startTime.Minute / 5 * 5;
                baseTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, min, 0);
            }
            else if (this.Interval == 3600)
            {
                baseTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
            }
            return baseTime;
        }

        // TOO LONG
        private void UpdateTimeAxisGraduations(DateTime beginTime, DateTime endTime, int days, bool completedDays, out double graduation, out int graduationCount)
        {
            this.Graduations.Clear();
            this.TimeAxis.Children.Clear();
            graduation = 0.0;
            graduationCount = 0;

            if (!completedDays && days <= 1)
            {
                int hours = GetHours(beginTime, endTime);
                
                if (hours > 24) 
                    hours = 24;
                const double TimeLabelOffset = 9.0;
                graduation = 9.0 * 24 / hours;
                graduationCount = 30;

                for (int i = 0; i <= hours * 4; i++)
                {
                    // One interval per 5px
                    double x = i * graduation;
                    Line scaleLine = new Line();

                    this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                    bool isWholePoint = (i % 4 == 0);
                    scaleLine.X1 = scaleLine.X2 = x;
                    scaleLine.Y1 = 0;
                    scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                    scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                    this.TimeAxis.Children.Add(scaleLine);

                    TextBlock timeLabel = null;
                    timeLabel = new TextBlock();
                    timeLabel.Foreground = Brushes.Black;
                    timeLabel.FontWeight = FontWeights.Light;
                    timeLabel.FontSize = 9;

                    double pos = i * graduation;

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - TimeLabelOffset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.TimeAxis.Children.Add(timeLabel);

                    if (isWholePoint)
                    {
                        string displayTime = this.GetFormatTime2(this.currentBaseTime, i * graduationCount, this.Interval);
                        if (timeLabel != null)
                        {
                            timeLabel.Text = displayTime;
                        }
                    }
                }


                return;
            }

            if (days <= 1)
            {
                const double TimeLabelOffset = 9.0;
                graduation = 9.0; // 15 min
                graduationCount = 30;

                for (int i = 0; i <= 24 * 4; i++)
                {
                    // One interval per 5px
                    double x = i * graduation;
                    Line scaleLine = new Line();

                    this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                    bool isWholePoint = (i % 4 == 0);
                    scaleLine.X1 = scaleLine.X2 = x;
                    scaleLine.Y1 = 0;
                    scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                    scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                    this.TimeAxis.Children.Add(scaleLine);

                    TextBlock timeLabel = null;
                    timeLabel = new TextBlock();
                    timeLabel.Foreground = Brushes.Black;
                    timeLabel.FontWeight = FontWeights.Light;
                    timeLabel.FontSize = 9;

                    double pos = i * graduation;

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - TimeLabelOffset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.TimeAxis.Children.Add(timeLabel);

                    if (isWholePoint)
                    {
                        string displayTime = this.GetFormatTime(this.currentBaseTime, i * graduationCount, this.Interval);
                        if (timeLabel != null)
                        {
                            timeLabel.Text = displayTime;
                        }
                    }
                }

            }
            else if (days == 2)
            {
                graduation = 4.0;
                graduationCount = 24;

                for (int i = 0; i <= 240; i++)
                {
                    // One interval per 5px
                    double x = i * graduation;
                    Line scaleLine = new Line();

                    this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                    bool isWholePoint = (i % 10 == 0);
                    scaleLine.X1 = scaleLine.X2 = x;
                    scaleLine.Y1 = 0;
                    scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                    scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                    this.TimeAxis.Children.Add(scaleLine);

                    TextBlock timeLabel = null;
                    timeLabel = new TextBlock();
                    timeLabel.Foreground = Brushes.Black;
                    timeLabel.FontWeight = FontWeights.Light;
                    timeLabel.FontSize = 9;

                    double pos = i * graduation;

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - Offset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.TimeAxis.Children.Add(timeLabel);

                    if (isWholePoint)
                    {
                        string displayTime = this.GetFormatTime(this.currentBaseTime, i * graduationCount, this.Interval);
                        if (timeLabel != null)
                        {
                            timeLabel.Text = displayTime;
                        }
                    }
                }
            }
            else if (days >= 3)
            {
                graduation = 9.0 / days;
                graduationCount = 30;

                int parts = days * 24 * 4;
                for (int i = 0; i <= parts; i++)
                {
                    // One interval per 5px
                    double x = i * graduation;
                    Line scaleLine = new Line();

                    this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                    bool isWholePoint = (i % 16 == 0);
                    scaleLine.X1 = scaleLine.X2 = x;
                    scaleLine.Y1 = 0;
                    scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                    scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                    this.TimeAxis.Children.Add(scaleLine);

                    TextBlock timeLabel = null;
                    timeLabel = new TextBlock();
                    timeLabel.Foreground = Brushes.Black;
                    timeLabel.FontWeight = FontWeights.Light;
                    timeLabel.FontSize = 9;

                    double pos = i * graduation;

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - Offset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.TimeAxis.Children.Add(timeLabel);

                    if (isWholePoint)
                    {
                        string displayTime = this.GetFormatTime(this.currentBaseTime, i * graduationCount, this.Interval);
                        if (timeLabel != null)
                        {
                            timeLabel.Text = displayTime;
                        }
                    }
                }
            }

            this.currentGraduation = graduation;
            this.currentGraduationCount = graduationCount;
            return;
            
        }

        public static int GetDays(DateTime beginTime, DateTime endTime)
        {
            long seconds = (endTime.Ticks - beginTime.Ticks) / 10000000;
            return (int)(seconds / 3600 / 24);
        }

        public static int GetHours(DateTime beginTime, DateTime endTime)
        {
            long seconds = (endTime.Ticks - beginTime.Ticks) / 10000000;
            return (int)(seconds / 3600);
        }

        public void UpdateTimeAxis(DateTime beginTime, DateTime endTime, bool completedDays, out double graduation, out int graduationCount)
        {
            int days = GetDays(beginTime, endTime);
            DateTime baseTime = this.GetBaseTime(beginTime);
            this.currentBaseTime = baseTime;
            this.UpdateTimeAxisGraduations(beginTime, endTime, days, completedDays, out graduation, out graduationCount);
        }

        public void SetValueRange(double min, double max)
        {
            this.CurveView.Min = min;
            this.CurveView.Max = max;
        }

        public void SetCurveDisplayName(string displayName)
        {
            this.CurveView.DisplayName = displayName;
        }
           

        private void MainViewMouseMove(object sender, MouseEventArgs e)
        {
            this.TrackTimeLine(e);   
        }

        private void TrackTimeLine(MouseEventArgs e)
        {
            if (this.disableTracking)
            {
                return;
            }

            bool timed = false;
            string timeLabel = string.Empty;
            CurveView curveView = (CurveView)this.CurveView;

            Point point = e.GetPosition((UIElement)curveView.CanvasView);
            double x = point.X;

            if (!timed && x >= 0)
            {
                double index = x * this.currentGraduationCount / this.currentGraduation;
                timeLabel = this.GetFormatDateTime(this.currentBaseTime, (int)index, this.Interval);
            }

            curveView.TrackTimeLine(point, timeLabel);
        }

        private string GetFormatTime(DateTime baseTime, int index, int interval)
        {
            if (interval == 60 * 5)
            {
                DateTime dt = baseTime.AddSeconds(index * interval / 10);
                if (dt.Minute == 0 && dt.Hour == 0)
                {
                    return string.Format("{0:d2}-{1:d2}\n{2:d2}:{3:d2}", dt.Month, dt.Day, dt.Hour, dt.Minute);
                }
                else
                {
                    return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
                }
            }
            else if (interval == 30)
            {
                DateTime dt = baseTime.AddSeconds(index * interval);
                if (dt.Minute == 0 && dt.Hour == 0)
                {
                    return string.Format("{0:d2}-{1:d2}\n{2:d2}:{3:d2}", dt.Month, dt.Day, dt.Hour, dt.Minute);
                }
                else
                {
                    return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
                }
            }
            return "";
        }

        private string GetFormatTime2(DateTime baseTime, int index, int interval)
        {
            DateTime dt = baseTime.AddSeconds(index * interval);
            if (interval == 60 * 5)
            {
                return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
            }
            else if (interval == 30)
            {
                return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
            }
            return "";
        }


        private string GetFormatDateTime(DateTime baseTime, int index, int interval)
        {
            DateTime dt = baseTime.AddSeconds(index * interval);
            string time = string.Empty;
            if (interval == 30)
            {
                time = string.Format("{0}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else if (interval == 60 * 5)
            {
                time = string.Format("{0}-{1:d2}-{2:d2} {3:d2}:{4:d2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
            }
            return time;
        }


        public int Interval
        {
            get;
            set;
        }


        // Save CHART bitmap file.
        public void SaveChart(string filePath = null)
        {
            DateTime now = DateTime.Now;
            string fileName = string.Format("{0}-{1}-{2}-{3}.bmp", now.Year, now.Month, now.Day, now.Ticks);
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = string.Format("./captures/{0}", fileName);
            }

            FileStream ms = new FileStream(filePath, FileMode.CreateNew);
            double width = this.MainView.ActualWidth;
            double height = this.MainView.ActualHeight;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Pbgra32);
            bmp.Render(this.MainView);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(ms);
            ms.Close();
        }

        private void CurveViewLoaded(object sender, RoutedEventArgs e)
        {
            this.curveDataContext = this.CurveView.AddCurveDataContext(this);
        }

        public void SetDataSource(List<Dictionary<string, object>> data, string valueKey, string timeKey = "time")
        {
            this.curveDataContext.SetDataSource(data, valueKey, timeKey);
            this.UpdateCurve();
        }

        public void SetDataSource2(List<Dictionary<string, object>> data, string valueKey, string timeKey = "time")
        {
            this.curveDataContext.SetDataSource2(data, valueKey, timeKey);
            this.UpdateCurve();
        }

        public void AddPoint(DateTime time, object value)
        {
            this.curveDataContext.AddPoint(time, value);
        }

        internal void UpdateCurve()
        {
            this.CurveView.UpdateCurve();
        }

        public void SetRealtime()
        {
            this.CurveView.RealTime = true;
        }

        public void HideTimeAxis()
        {
            this.TimeAxis.Visibility = Visibility.Collapsed;
            this.TimeAxisRow.Height = new GridLength(0);
        }

        public void HideResetButton()
        {
            this.CurveView.HideResetButton();
        }

        private bool disableTracking = false;

        internal bool disableGridLine = false;

        public void DisableTrackingLine()
        {
            this.disableTracking = true;
        }

        public void DisableGridLine()
        {
            this.disableGridLine = true;
        }

        public void SetCurveColor(Color color)
        {
            this.CurveView.CurveColor = color;
        }


        public void SetUpdateRangeHandler(Action<double, double> action)
        {
            this.updateRangeAction = action;    
        }

        public void SetResetHandler(Action action)
        {
            this.resetAction = action;
        }

        public Action<double,double> updateRangeAction { get; set; }

        public Action resetAction { get; set; }


        public void UpdateRange(double begin, double end)
        {
            this.curveDataContext.UpdateRange(begin, end);
        }

        public void Reset()
        {
            this.curveDataContext.Reset();
        }
    }
}

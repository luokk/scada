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

        private CurveView curveView;

        private double currentGraduation = 0.0;

        private int currentGraduationCount = 0;

        private bool initialized = false;

        private DateTime currentBaseTime = default(DateTime);

        private Dictionary<int, GraduationLine> Graduations
        {
            get;
            set;
        }

        private Dictionary<int, GraduationTime> GraduationTimes
        {
            get;
            set;
        }

        public ChartView()
        {
            InitializeComponent();
            this.Graduations = new Dictionary<int, GraduationLine>();
            this.GraduationTimes = new Dictionary<int, GraduationTime>();
        }

        public static readonly DependencyProperty TimeScaleProperty = DependencyProperty.Register("TimeScale", typeof(long), typeof(ChartView));
        private CurveDataContext curveDataContext;

        private DateTime GetBaseTime(DateTime startTime)
        {
            return startTime;
            /*
            // 目前只支持30秒 和 5分钟两种间隔
            Debug.Assert(this.Interval == 30 || this.Interval == 60 * 5 || this.Interval == 0);

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
            return baseTime;
             * */
        }

        private void UpdateTimeAxisGraduation(DateTime beginTime, DateTime endTime, int days, out double graduation, out int graduationCount)
        {
            this.Graduations.Clear();

            graduation = 0.0;
            graduationCount = 0;
            if (days <= 1)
            {
                const double TimeLabelOffset = 9.0;
                graduation = 4.0;
                graduationCount = 12;

                this.Interval = 30;
                for (int i = 0; i < 240; i++)
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
                    if (this.GraduationTimes.ContainsKey(i))
                    {
                        timeLabel = this.GraduationTimes[i].Text;
                    }
                    else
                    {
                        timeLabel = new TextBlock();
                        timeLabel.Foreground = Brushes.Black;
                        timeLabel.FontWeight = FontWeights.Light;
                        timeLabel.FontSize = 9;

                        double pos = i * graduation;
                        GraduationTimes.Add(i, new GraduationTime()
                        {
                            Text = timeLabel,
                            Pos = pos
                        });

                        timeLabel.SetValue(Canvas.LeftProperty, (double)pos - TimeLabelOffset);
                        timeLabel.SetValue(Canvas.TopProperty, (double)10);

                        this.TimeAxis.Children.Add(timeLabel);
                    }

                    if (isWholePoint)
                    {
                        string displayTime = this.GetFormatTime(this.currentBaseTime, i * graduationCount, this.Interval);
                        if (timeLabel != null)
                        {
                            timeLabel.Text = displayTime;
                        }
                    }
                }

                this.TimeAxis.UpdateLayout();
            }
            else if (days == 2)
            {
                graduation = 4.0;
                graduationCount = 24;

                this.Interval = 30;
                for (int i = 0; i < 240; i++)
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
                    if (this.GraduationTimes.ContainsKey(i))
                    {
                        timeLabel = this.GraduationTimes[i].Text;
                    }
                    else
                    {
                        timeLabel = new TextBlock();
                        timeLabel.Foreground = Brushes.Black;
                        timeLabel.FontWeight = FontWeights.Light;
                        timeLabel.FontSize = 9;

                        double pos = i * graduation;
                        GraduationTimes.Add(i, new GraduationTime()
                        {
                            Text = timeLabel,
                            Pos = pos
                        });

                        timeLabel.SetValue(Canvas.LeftProperty, (double)pos - Offset);
                        timeLabel.SetValue(Canvas.TopProperty, (double)10);

                        this.TimeAxis.Children.Add(timeLabel);
                    }

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
                /*
            else if (days == 2)
            {

            }
            else if (days == 3)
            {

            }
            else if (days > 3)
            {

            }
                 */

            
        }

        public static int GetDays(DateTime beginTime, DateTime endTime)
        {
            long seconds = (endTime.Ticks - beginTime.Ticks) / 10000000;
            return (int)(seconds / 3600 / 24);
        }

        public void UpdateTimeAxis(DateTime beginTime, DateTime endTime, out double graduation, out int graduationCount)
        {
            int days = GetDays(beginTime, endTime);
            DateTime baseTime = this.GetBaseTime(beginTime);
            this.currentBaseTime = baseTime;
            this.UpdateTimeAxisGraduation(beginTime, endTime, days, out graduation, out graduationCount);
        }

        // TODO: Remove
        public CurveView SetCurveView(string curveViewName, string displayName, double height = 200.0)
        {
            //this.CurveView.CurveViewName = curveViewName;
            //this.CurveView.Height = height + ChartView.ViewGap;

            return this.CurveView;
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
            bool timed = false;
            string timeLabel = string.Empty;
            CurveView curveView = (CurveView)this.CurveView;

            Point point = e.GetPosition((UIElement)curveView.View);
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
            DateTime dt = baseTime.AddSeconds(index * interval);
            if (interval == 60 * 5)
            {
                return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
            }
            else if (interval == 30)
            {
                if (dt.Minute == 0 && dt.Second == 0)
                {
                    return string.Format("{0:d2}:{1:d2}", dt.Hour, dt.Minute);
                }
                else
                {
                    return string.Format("{0:d2}:{1:d2}", dt.Minute, dt.Second);
                }
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
            else
            {
                filePath = string.Format("{0}/{1}", filePath, fileName);
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
            this.CurveView.ChartView = this;
            this.curveDataContext = this.CurveView.AddCurveDataContext(this);
        }

        public void SetDataSource(List<Dictionary<string, object>> data, string valueKey, string timeKey = "time")
        {
            this.curveDataContext.SetDataSource(data, valueKey, timeKey);
        }

        public void AddPoint(DateTime time, object value)
        {
            this.curveDataContext.AddPoint(time, value);
        }


    }
}

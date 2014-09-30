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

        public const double Graduation = 8.0;

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

        public bool RealTimeMode
        {
            get;
            set;
        }

        private double scale = 1.0;

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

        public static readonly DependencyProperty TimeScaleProperty =
            DependencyProperty.Register("TimeScale", typeof(long), typeof(ChartView));

        private void TimeAxisLoaded(object sender, RoutedEventArgs e)
        {
            if (this.initialized)
            {
                return;
            }
            this.initialized = true;

            this.InitTimeAxis(DateTime.Now);
        }

        private DateTime GetBaseTime(DateTime startTime)
        {
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
        }

        private void InitTimeAxis(DateTime startTime)
        {
            // Base Time;   
            // this.currentBaseTime = this.GetBaseTime(startTime);

            for (int i = 0; i < 100; i++)
            {
                // One interval per 5px
                double x = i * Graduation;
                Line scaleLine = new Line();

                this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                bool isWholePoint = (i % 5 == 0);
                scaleLine.X1 = scaleLine.X2 = x;
                scaleLine.Y1 = 0;
                scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                this.TimeAxis.Children.Add(scaleLine);
            }

            this.UpdateTimeAxis(startTime);
        }

        public void UpdateTimeAxis(int offset)
        {
            var newDateTime = this.currentBaseTime.AddSeconds(offset * this.Interval);
            this.UpdateTimeAxis(newDateTime);
        }

        public void UpdateTimeAxis(DateTime startTime)
        {
            DateTime baseTime = this.GetBaseTime(startTime);
            if (this.currentBaseTime == baseTime)
            {
                return;
            }
            this.currentBaseTime = baseTime;

            for (int i = 0; i < 20; i++)
            {
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

                    double pos = i * 5 * Graduation;
                    GraduationTimes.Add(i, new GraduationTime()
                    {
                        Text = timeLabel, Pos = pos
                    });

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - Offset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.TimeAxis.Children.Add(timeLabel);
                }

                string displayTime = this.GetFormatTime(this.currentBaseTime, i, this.Interval);
                if (timeLabel != null)
                {
                    timeLabel.Text = displayTime;
                }
                
            }

        }

        public CurveView AddCurveView(string curveViewName, string displayName, double height = 200.0)
        {
            CurveView curveView = new CurveView(this);
            curveView.CurveViewName = curveViewName;
            curveView.TimeScale = this.TimeScale;
            curveView.Height = height + ChartView.ViewGap;
            this.ChartContainer.Children.Add(curveView);
            this.AddCurveViewCheckItem(curveViewName, displayName);
            return curveView;
        }

        private void AddCurveViewCheckItem(string curveViewName, string displayName)
        {
            CheckBox cb = new CheckBox();
            cb.IsChecked = true;
            cb.Content = displayName;
            cb.Margin = new Thickness(5, 0, 5, 0);
            
            cb.Checked += (object sender, RoutedEventArgs e) => 
            {
                this.OnItemChecked(curveViewName, true);
            };
            cb.Unchecked += (object sender, RoutedEventArgs e) =>
            {
                this.OnItemChecked(curveViewName, false);
            };
            if (this.SelectedItems1.Children.Count > 8)
            {
                this.SelectedItems2.Visibility = Visibility.Visible;
                this.SelectedItems2.Children.Add(cb);
            }
            else
            {
                this.SelectedItems1.Children.Add(cb);
            }
        }

        private void OnItemChecked(string curveViewName, bool itemChecked)
        {
            foreach (var cv in this.ChartContainer.Children)
            {
                CurveView curveView = (CurveView)cv;
                if (curveView.CurveViewName == curveViewName)
                {
                    curveView.Visibility = itemChecked ? Visibility.Visible : Visibility.Collapsed;
                    break;
                }
            }
        }

        private void MainViewMouseMove(object sender, MouseEventArgs e)
        {
            Mouse.SetCursor(Cursors.Arrow);
            this.TrackTimeLine(e);   
        }


        private void TrackTimeLine(MouseEventArgs e)
        {
            bool timed = false;
            string timeLabel = string.Empty;
            foreach (var view in this.ChartContainer.Children)
            {
                CurveView curveView = (CurveView)view;

                Point point = e.GetPosition((UIElement)curveView.View);
                double x = point.X;
                double centerX = curveView.CenterX;
                if (!timed && x >= 0)
                {
                    double v = (x - centerX) / scale + centerX;

                    double index = v / Graduation / 5;
                    timeLabel = this.GetFormatDateTime(this.currentBaseTime, (int)index, this.Interval);
                }

                curveView.TrackTimeLine(point, timeLabel);
            }
        }

        private void MoveCurveLines(MouseEventArgs e, bool moving)
        {
            bool timed = false;
            string timeLabel = string.Empty;
            foreach (var view in this.ChartContainer.Children)
            {
                CurveView curveView = (CurveView)view;

                if (moving)
                {

                    Point point = e.GetPosition((UIElement)curveView.View);
                    double x = point.X;
                    double centerX = curveView.CenterX;
                    if (!timed && x >= 0)
                    {
                        double v = (x - centerX) / scale + centerX;

                        double index = v / Graduation / 5;
                        timeLabel = this.GetFormatDateTime(this.currentBaseTime, (int)index, this.Interval);
                    }

                    curveView.MoveCurveLine(point, timeLabel);
                }
                else
                {
                    curveView.MoveCurveLine(false);
                }
            }
        }



        public long TimeScale
        {
            get
            {
                return (long)this.GetValue(TimeScaleProperty);
            }

            set
            {
                this.SetValue(TimeScaleProperty, (long)value);
            }
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
                    return string.Format("{0:d2}:{1:d2}\n[{2:d2}时]", dt.Minute, dt.Second, dt.Hour);
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
    }
}

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
    public partial class EnergyChartView : UserControl
    {
        public const double ViewGap = 10.0;

        public const double Grad = 2.0;

        const int IntervalCount = 20;

        public const double Offset = 8.0;

        private const int AllCount = 2048;

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

        public EnergyChartView()
        {
            InitializeComponent();
            this.Graduations = new Dictionary<int, GraduationLine>();
            this.GraduationTimes = new Dictionary<int, GraduationTime>();
            this.CurveView.Height = 400;
        }

        public static readonly DependencyProperty PointAxisScaleProperty =
            DependencyProperty.Register("PointAxisAxisScale", typeof(long), typeof(EnergyChartView));

        private void PointAxisLoaded(object sender, RoutedEventArgs e)
        {
            if (this.initialized)
            {
                return;
            }
            this.initialized = true;
            this.InitPointAxis();
            //this.AddCurveView("Energy", "能谱图");
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

        private void InitPointAxis()
        {
            /*
            for (int i = 0; i < 2048 / 2; i++)
            {
                // One interval per 5px
                double x = i * Grad;
                Line scaleLine = new Line();

                this.Graduations.Add(i, new GraduationLine() { Line = scaleLine, Pos = x });

                bool isWholePoint = (i % 10 == 0);
                scaleLine.X1 = scaleLine.X2 = x;
                scaleLine.Y1 = 0;
                scaleLine.Y2 = isWholePoint ? Charts.MainScaleLength : Charts.ScaleLength;
                scaleLine.Stroke = isWholePoint ? Brushes.Gray : Brushes.LightGray;
                this.PointAxis.Children.Add(scaleLine);
            }

            this.UpdatePointAxis();
            */ 
        }

        public void UpdatePointAxis()
        {
            /*
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
                    //timeLabel.Foreground = Brushes.White;
                    timeLabel.FontWeight = FontWeights.Light;
                    timeLabel.FontSize = 9;

                    double pos = i * IntervalCount * Grad;
                    GraduationTimes.Add(i, new GraduationTime()
                    {
                        Text = timeLabel, Pos = pos
                    });

                    timeLabel.SetValue(Canvas.LeftProperty, (double)pos - Offset);
                    timeLabel.SetValue(Canvas.TopProperty, (double)10);

                    this.PointAxis.Children.Add(timeLabel);
                }

                string displayTime = this.GetFormatTime(this.currentBaseTime, i, IntervalCount * this.Interval);
                if (timeLabel != null)
                {
                    timeLabel.Text = displayTime;
                }
                
            }*/
        }

        // !
        public EnergyCurveView AddCurveView(string curveViewName, string displayName)
        {
            //EnergyCurveView curveView = new EnergyCurveView(this);
            this.CurveView.CurveViewName = curveViewName;
            this.CurveView.PointAxisScale = this.PointAxisScale;
            this.CurveView.Height = 400;
            
            //this.ChartContainer.Children.Add(curveView);
            //this.ChartContainer.Height= 500;
            return this.CurveView;
        }

        public void ClearPoints()
        {
            EnergyCurveView curveView = (EnergyCurveView)this.CurveView;
            curveView.ClearPoints();
            //this.index = 0;
        }

        public void SetDataPoints(int[] data)
        {
            EnergyCurveView curveView = (EnergyCurveView)this.CurveView;


            curveView.SetPoints(data);
        }

        private void MainViewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.pressed)
            {
                // Mouse.SetCursor(Cursors.ScrollWE);
                // this.MoveCurveLines(e, true);
            }
            else
            {
                Mouse.SetCursor(Cursors.Arrow);
                this.TrackTimeLine(e);
                this.MoveCurveLines(e, false);
            }
        }


        private void TrackTimeLine(MouseEventArgs e)
        {
            bool timed = false;
            string timeLabel = string.Empty;
            EnergyCurveView curveView = (EnergyCurveView)this.CurveView;

            Point point = e.GetPosition((UIElement)curveView.View);
            double x = point.X;
            double centerX = curveView.CenterX;
            if (!timed && x >= 0)
            {
                double v = (x - centerX) / scale + centerX;
                double index = v / Grad / IntervalCount;

                timeLabel = this.GetFormatDateTime(this.currentBaseTime, index, IntervalCount * this.Interval);
            }

            curveView.TrackTimeLine(point, timeLabel);

            /*
            foreach (var view in this.ChartContainer.Children)
            {
                EnergyCurveView curveView = (EnergyCurveView)view;

                Point point = e.GetPosition((UIElement)curveView.View);
                double x = point.X;
                double centerX = curveView.CenterX;
                if (!timed && x >= 0)
                {
                    double v = (x - centerX) / scale + centerX;
                    double index = v / Grad / IntervalCount;

                    timeLabel = this.GetFormatDateTime(this.currentBaseTime, index, IntervalCount * this.Interval);
                }

                curveView.TrackTimeLine(point, timeLabel);
            }*/
        }

        private void MoveCurveLines(MouseEventArgs e, bool moving)
        {
            /*
            bool timed = false;
            string timeLabel = string.Empty;
            foreach (var view in this.ChartContainer.Children)
            {
                EnergyCurveView curveView = (EnergyCurveView)view;

                if (moving)
                {

                    Point point = e.GetPosition((UIElement)curveView.View);
                    double x = point.X;
                    double centerX = curveView.CenterX;
                    if (!timed && x >= 0)
                    {
                        double v = (x - centerX) / scale + centerX;

                        double index = v / Grad / 5;
                        timeLabel = this.GetFormatDateTime(this.currentBaseTime, (int)index, this.Interval);
                    }

                    curveView.MoveCurveLine(point, timeLabel);
                }
                else
                {
                    curveView.MoveCurveLine(false);
                }
            }
            */
        }



        public long PointAxisScale
        {
            get
            {
                return (long)this.GetValue(PointAxisScaleProperty);
            }

            set
            {
                this.SetValue(PointAxisScaleProperty, (long)value);
            }
        }

        private void ZoomHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            int a = e.Delta;
            if (a > 0)
            {
                this.scale += 0.1;
                if (this.scale > 3.0)
                {
                    this.scale = 3.0;
                }
            }
            else if (a < 0)
            {
                this.scale -= 0.1;
                if (this.scale < 1.0)
                {
                    this.scale = 1.0;
                }
            }

            this.ZoomChartView(this.scale);
        }

        private void ZoomChartView(double scale)
        {
            double centerX = 0.0;
            EnergyCurveView curveView = (EnergyCurveView)this.CurveView;
            curveView.UpdateCurveScale(scale);
            centerX = curveView.CenterX;

            this.UpdateTimeAxisScale(scale, centerX);
        }

        private void UpdateTimeAxisScale(double scale, double centerX)
        {
            if (scale < 1.0 || scale > 3.0)
            {
                return;
            }

            // Update Time graduation lines.
            foreach (var g in this.Graduations)
            {
                Line l = g.Value.Line;
                l.X1 = l.X2 = (g.Value.Pos - centerX) * scale + centerX;
            }

            // Update Time Label
            foreach (var t in this.GraduationTimes)
            {
                TextBlock b = t.Value.Text;
                // double pos = (g.Value.Pos - centerY) * scale + centerY;
                double pos = (t.Value.Pos - centerX) * scale + centerX;
                b.SetValue(Canvas.LeftProperty, (double)pos - Offset);
            }
        }

        private string GetFormatTime(DateTime baseTime, int index, int interval)
        {
            DateTime dt = baseTime.AddSeconds(index * interval);
            if (interval >= 60 * 5)
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


        private string GetFormatDateTime(DateTime baseTime, double index, int interval)
        {
            DateTime dt = baseTime.AddSeconds(index * interval);
            string time = string.Empty;
            if (interval == 30)
            {
                time = string.Format("{0}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else if (interval >= 60 * 5)
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



        public void SaveChart()
        {
            DateTime now = DateTime.Now;
            string fileName = string.Format("{0}-{1}-{2}-{3}.bmp", now.Year, now.Month, now.Day, now.Ticks);
            string filePath = string.Format("./captures/{0}", fileName);
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


        private bool pressed = false;

        private void CanvasViewMouseLeftButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                pressed = true;
            }
            else
            {
                pressed = false;
            }
        }


    }
}

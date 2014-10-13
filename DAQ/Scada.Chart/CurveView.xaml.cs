using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scada.Chart
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class CurveView : UserControl
    {
        // Graduation Line
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

        // Graduation Label Text
        struct GraduationText
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

        public const double GridViewHeight = 1000.0;

        public const double GridViewWidth = 1000.0;

        private bool init = false;

        public bool RealTime { get; set; }

        private Line timeLine = new Line();

        private Path curve = null;

        private GeometryGroup lines = new GeometryGroup();

        private CurveDataContext dataContext;

        private double centerX = 0.0;

        // private double centerY = 0.0;

        private Border valueBorder;

        private Rectangle rangeSelectionRect;

        private TextBlock valueLabel;

        public double Min { set; get; }

        public double Max { set; get; }

        private double paddingTop;

        private double paddingMargin;


        public ChartView ChartView { get; set; }

        const int MaxVisibleCount = 14;

        private Dictionary<int, GraduationText> GraduationTexts
        {
            get;
            set;
        }

        public CurveView()
        {
            InitializeComponent();
            this.RealTime = false;
            this.paddingTop = 2.0;
            this.paddingMargin = 2.0;
        }

        private void CurveViewLoaded(object sender, RoutedEventArgs e)
        {
            if (!init)
            {
                this.Initialize();
                init = true;
            }
        }

        private void Initialize()
        {
            this.CanvasView.Height = this.Height - ChartView.ViewGap;
            this.Graduation.Height = this.Height - ChartView.ViewGap;
            // Grid Line |||
            double canvasHeight = this.CanvasView.Height;
            Color gridLineColor = Color.FromRgb(192, 192, 192);
            SolidColorBrush gridLineBrush = new SolidColorBrush(gridLineColor);


            // Grid Line ---
            for (int i = 0; i < 20; i++)
            {
                Line l = new Line();
                l.Y1 = l.Y2 = canvasHeight - i * 40;
                l.X1 = 0;
                l.X2 = 1900;
                if (i == 0)
                    l.StrokeThickness = 0.1;
                else
                    l.StrokeThickness = 0.5;

                l.Stroke = gridLineBrush;
                this.CanvasView.Children.Add(l);
            }

            // Scale line
            double height = this.CanvasView.Height;

            double scaleWidth = 30;
            this.Graduation.ClipToBounds = true;
            int textCount = 0;

            double d = height / (this.Max - this.Min);
            // How many graduation?
            int dc = (int)height / 10;
            // What's the value each graduation 
            double ev = (this.Max - this.Min) / dc;

            if (!this.ChartView.disableGridLine)
            {
                for (int i = 0; i < 60; i++)
                {
                    double y = height - i * 10;

                    if (y < 0)
                    {
                        break;
                    }

                    Line l = new Line();

                    l.Y1 = l.Y2 = y;
                    l.X1 = (i % 5 != 0) ? scaleWidth - Charts.ScaleLength : scaleWidth - Charts.MainScaleLength;
                    l.X2 = scaleWidth;

                    l.Stroke = new SolidColorBrush(Colors.Gray);
                    this.Graduation.Children.Add(l);

                    double value = this.Min + i * ev;

                    if (i % 5 == 0)
                    {
                        TextBlock t = new TextBlock();
                        t.Foreground = Brushes.Black;
                        t.FontSize = 9;
                        double pos = (double)y - 10;

                        if (this.Max > 10)
                        {
                            t.Text = string.Format("{0}", (int)value);
                        }
                        else if (this.Max > 1)
                        {
                            double dv = ConvertDouble(value, 1);
                            t.Text = string.Format("{0:f1}", (double)dv);
                        }
                        else
                        {
                            double dv = ConvertDouble(value, 2);
                            t.Text = string.Format("{0:f2}", (double)dv);
                        }

                        t.SetValue(Canvas.RightProperty, (double)10.0);
                        t.SetValue(Canvas.TopProperty, (double)pos - 10.0);
                        this.Graduation.Children.Add(t);

                        textCount++;
                    }
                }
            }

            timeLine.Y1 = 0;
            timeLine.Y2 = GridViewHeight / 2;
            timeLine.Stroke = new SolidColorBrush(Colors.Gray);
            this.CanvasView.Children.Add(timeLine);
            this.CanvasView.ClipToBounds = true;

            this.AddCurveLine();

            this.SetDisplayName(this.DisplayName);
        }

        private void AddCurveLine()
        {
            this.curve = new Path();
            if (this.RealTime)
            {
                this.curve.Data = this.lines;
            }

            this.curve.StrokeThickness = 1;
            Color curveColor = Color.FromRgb(00, 0x7A, 0xCC);

            this.curve.Stroke = new SolidColorBrush(curveColor);
            this.CanvasView.Children.Add(this.curve);
        }

        public Color CurveColor
        {
            get;
            set;
        }

        public CurveDataContext AddCurveDataContext(ChartView chartView)
        {
            this.dataContext = new CurveDataContext(chartView);
            this.dataContext.AppendCurvePoint += this.AppendCurvePointHandler;
            this.dataContext.ClearCurvePoints += this.ClearCurvePointsHandler;

            return this.dataContext;
        }

        public string CurveViewName
        {
            get;
            set;
        }

        private string displayName;

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
                this.UpdateDisplayName(value);
            }
        }

        private Point lastPoint = default(Point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        private void AppendCurvePointHandler(Point point)
        {
            if (this.lastPoint == default(Point))
            {
                this.lastPoint = point;
                return;
            }

            if (point == default(Point))
            {
                this.lastPoint = point;
                return;
            }
            Point p1, p2;
            this.Convert(this.lastPoint, out p1);
            this.Convert(point, out p2);

            LineGeometry line = new LineGeometry(p1, p2);
            this.lines.Children.Add(line);
            this.lastPoint = point;
            return;
        }

        private void ClearCurvePointsHandler()
        {
            this.lastPoint = default(Point);
            if (this.curve != null)
            {
                this.lines.Children.Clear();
                // Do not need Remove the object. 
                // this.CanvasView.Children.Remove(this.curve);
            }
        }

        public void TrackTimeLine(Point point, string timeLabel)
        {
            timeLine.X1 = timeLine.X2 = point.X;
            // this.centerX = point.X;
            this.ShowValueTip(point, timeLabel);
        }


        private void ShowValueTip(Point point, string timeLabel)
        {
            double x = point.X;

            double xo = x;

            double y = double.NaN;
            if (this.GetY(xo, out y))
            {
                double v = ConvertDouble(y, 4);

                this.valueBorder.Visibility = Visibility.Visible;
                
                if (string.IsNullOrEmpty(timeLabel))
                {
                    this.valueLabel.Text = v.ToString();
                }
                else
                {
                    this.valueLabel.Text = string.Format("[{0}]  {1}", timeLabel, v);
                }
            }
            else
            {
                this.valueBorder.Visibility = Visibility.Collapsed;
            }
        }

        static double ConvertDouble(double d, int n)
        {
            if (d == 0.0)
                return 0;
            if (d > 1 || d < -1)
                n = n - (int)Math.Log10(Math.Abs(d)) - 1;
            else
                n = n + (int)Math.Log10(1.0 / Math.Abs(d));
            if (n < 0)
            {
                d = (int)(d / Math.Pow(10, 0 - n)) * Math.Pow(10, 0 - n);
                n = 0;
            }
            return Math.Round(d, n);
        }

        private double Convert(double v)
        {
            double range = this.Max - this.Min;
            double pa = 0.0;
            double pb = 0.0;
            if (v <= this.Max && v >= this.Min)
            {
                pa = this.Max - v;
                pb = v - this.Min;
            }
            else
            {
                return 0.0;
            }

            double pos = this.Height / (pa / pb + 1);
            double y = this.Height - pos;
            return y;
        }

        private bool GetY(double x, out double y)
        {
            Point a = default(Point);
            Point b = default(Point);
            bool found = false;
            foreach (var p in this.dataContext.points)
            {
                if (p.X > x)
                {
                    b = p;
                    found = true;
                    break;
                }
                a = p;
            }

            if (found)
            {
                if (x - a.X < b.X - x)
                {
                    y = a.Y;
                }
                else
                {
                    y = b.Y;
                }
                return true;
            }
            else
            {
                y = double.NaN;
                return false;
            }
        }

        private double GetValue(double y)
        {
            double v = this.Max - (this.Max - this.Min) * y / this.Height;
            return v;
        }

        private void Convert(Point p, out Point po)
        {
            po = new Point(p.X, this.Convert(p.Y));
        }

        private void SetDisplayName(string displayName)
        {
            const double Top = 12.0;
            SolidColorBrush labelBrush = new SolidColorBrush(Color.FromRgb(219, 219, 219));
            
            TextBlock displayLabel = new TextBlock();
            this.displayNameLabel = displayLabel;
            displayLabel.Text = displayName;
            // displayLabel.Background = labelBrush;
            
            displayLabel.Foreground = Brushes.Black;
            displayLabel.SetValue(Canvas.RightProperty, 12.0);
            displayLabel.SetValue(Canvas.TopProperty, Top);
            this.CanvasView.Children.Add(displayLabel);
            
            // Value text Label.
            this.valueBorder = new Border();
            // valueBorder.Background = labelBrush;
            valueBorder.CornerRadius = new CornerRadius(1.0);
            valueBorder.BorderBrush = labelBrush;
            valueBorder.Padding = new Thickness(4.0, 0.0, 4.0, 3.0);
            valueBorder.SetValue(Canvas.RightProperty, 120.0);
            
            valueBorder.SetValue(Canvas.TopProperty, Top);
            this.valueLabel = new TextBlock();
            this.valueLabel.Foreground = Brushes.Black;

            valueBorder.Child = valueLabel;
            this.CanvasView.Children.Add(valueBorder);
        }

        private void UpdateDisplayName(string displayName)
        {
            if (this.displayNameLabel != null)
                this.displayNameLabel.Text = displayName;
        }

        private bool mouseLeftButtonDown = false;

        private Point selBeginPoint;

        private Point selEndPoint;

        private void CanvasView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.mouseLeftButtonDown = true;
            this.selBeginPoint = e.GetPosition((UIElement)this.CanvasView);

            if (this.rangeSelectionRect == null)
            {
                this.rangeSelectionRect = new Rectangle();
                this.rangeSelectionRect.Fill = Brushes.LightBlue;
                this.rangeSelectionRect.Opacity = 0.5;
                this.CanvasView.Children.Add(this.rangeSelectionRect);
            }

            this.rangeSelectionRect.Height = 0;
            this.rangeSelectionRect.Width = 0;

            this.rangeSelectionRect.Visibility = Visibility.Visible;

            var targetElement = e.Source as IInputElement;
            targetElement.CaptureMouse();
        }

        private void CanvasView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            if (this.mouseLeftButtonDown)
            {
                this.selEndPoint = e.GetPosition((UIElement)this.CanvasView);
                this.DoRenderUpdate();
            }
            this.mouseLeftButtonDown = false;
            if (this.rangeSelectionRect != null)
            {
                this.rangeSelectionRect.Visibility = Visibility.Hidden;
            }
        }

        private void CanvasViewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseLeftButtonDown)
            {
                this.selEndPoint = e.GetPosition((UIElement)this.CanvasView);
                this.DoRangeUpdate(this.selBeginPoint, this.selEndPoint);
            }
        }

        private void DoRangeUpdate(Point selBeginPoint, Point selEndPoint)
        {
            double left = Math.Min(selBeginPoint.X, selEndPoint.X);
            double top = Math.Min(selBeginPoint.Y, selEndPoint.Y);

            this.rangeSelectionRect.SetValue(Canvas.LeftProperty, left);
            this.rangeSelectionRect.SetValue(Canvas.TopProperty, top);

            this.rangeSelectionRect.Width = Math.Abs(selBeginPoint.X - selEndPoint.X);
            this.rangeSelectionRect.Height = Math.Abs(selBeginPoint.Y - selEndPoint.Y);

        }

        private void DoRenderUpdate()
        {
            double beginPointX = Math.Min(this.selBeginPoint.X, this.selEndPoint.X);
            double endPointX = Math.Max(this.selBeginPoint.X, this.selEndPoint.X);
            if (endPointX - beginPointX > 10)
            {
                this.dataContext.UpdateRange(beginPointX, endPointX);
            }
        }

        internal void UpdateCurve()
        {
            if (this.CurveColor == default(Color))
            {
                this.CurveColor = Color.FromRgb(00, 0x7A, 0xCC);
            }
            this.curve.Stroke = new SolidColorBrush(this.CurveColor);
            this.curve.Data = this.lines;
        }

        protected void OnResetClick(object sender, RoutedEventArgs e)
        {
            this.dataContext.Reset();
        }

        internal void HideResetButton()
        {
            this.ResetButton.Visibility = System.Windows.Visibility.Hidden;
        }

        public TextBlock displayNameLabel { get; set; }
    }
}

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
    public partial class EnergyCurveView : UserControl
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

        private Line timeLine = new Line();

        private Polyline curve = null;

        //private double i = 0;

        private double currentScale = 1.0;

        //private CurveDataContext dataContext;

        private double centerX = 0.0;

        private double centerY = 0.0;

        private Border valueBorder;

        private TextBlock valueLabel;

        const int MaxVisibleCount = 14;

        private double finalOffsetPos = 0.0;

        private double ZeroOffset = 30.0;

        public int[] data { get; set; }

        public double CenterX
        {
            get
            {
                return this.centerX;
            }
        }

        public EnergyCurveView()
        {
            InitializeComponent();
        }

        public EnergyCurveView(EnergyChartView chartView)
        {
            InitializeComponent();
        }

        private void CurveViewLoaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void CanvasView_Loaded_1(object sender, RoutedEventArgs e)
        {
        }

        private double CanvasHeight
        {
            get;
            set;
        }

        private void Initialize()
        {
            if (this.init)
                return;
            
            this.init = true;

            this.CanvasView.Height  = this.CanvasHeight = this.Height - 10;

            Color gridLineColor = Color.FromRgb(192, 192, 192);
            SolidColorBrush gridLineBrush = new SolidColorBrush(gridLineColor);

            for (int i = 0; i < 32; i++)
            {
                Line l = new Line();
                l.X1 = l.X2 = i * Grad * 200;
                l.Y1 = 0;
                l.Y2 = GridViewHeight;
                l.StrokeThickness = 0.5;

                l.Stroke = gridLineBrush;
                this.CanvasView.Children.Add(l);
            }

            this.InitPointAxis();

            // Grid Line ---
            for (int i = 0; i < 20; i++)
            {
                Line l = new Line();
                l.Y1 = l.Y2 = this.CanvasHeight - ZeroOffset - i * Grad * 200;
                l.X1 = 0;
                l.X2 = 1900;
                l.StrokeThickness = 0.5;
                
                l.Stroke = gridLineBrush;
                this.CanvasView.Children.Add(l);
            }


            timeLine.Y1 = 0;
            timeLine.Y2 = GridViewHeight / 2;
            timeLine.Stroke = new SolidColorBrush(Colors.Gray);
            this.CanvasView.Children.Add(timeLine);
            this.CanvasView.ClipToBounds = true;

            this.AddCurveLine();

            this.SetDisplayName(this.DisplayName);

        }

        private void InitPointAxis()
        {
            this.PointAxis.Children.Clear();
            for (int i = 0; i <= 16; i++)
            {
                double x = i * Grad * 200;
                Line scaleLine = new Line();
                
                scaleLine.X1 = scaleLine.X2 = x;
                scaleLine.Y1 = 0;
                scaleLine.Y2 = Charts.ScaleLength;
                scaleLine.Stroke =  Brushes.Gray;
                this.PointAxis.Children.Add(scaleLine);

                TextBlock timeLabel = new TextBlock();
                timeLabel.Foreground = Brushes.Black;
                timeLabel.FontWeight = FontWeights.Light;
                timeLabel.FontSize = 9;
                timeLabel.Text = (i * 200).ToString();
                timeLabel.SetValue(Canvas.LeftProperty, (double)x - 10);
                timeLabel.SetValue(Canvas.TopProperty, (double)10);
                this.PointAxis.Children.Add(timeLabel);
            }

        }

        private double UpdatePointAxis(int from, int to)
        {
            this.PointAxis.Children.Clear();
            double grad = Grad * 16 / (to - from);
            for (int i = 0; i <= (to - from); i++)
            {
                double x = i * grad * 200;
                Line scaleLine = new Line();

                scaleLine.X1 = scaleLine.X2 = x;
                scaleLine.Y1 = 0;
                scaleLine.Y2 = Charts.ScaleLength;
                scaleLine.Stroke = Brushes.Gray;
                this.PointAxis.Children.Add(scaleLine);

                TextBlock timeLabel = new TextBlock();
                timeLabel.Foreground = Brushes.Black;
                timeLabel.FontWeight = FontWeights.Light;
                timeLabel.FontSize = 9;
                timeLabel.Text = ((from + i) * 200).ToString();
                timeLabel.SetValue(Canvas.LeftProperty, (double)x - 10);
                timeLabel.SetValue(Canvas.TopProperty, (double)10);
                this.PointAxis.Children.Add(timeLabel);
            }
            return grad;
        }


        private void AddCurveLine()
        {
            this.curve = new Polyline();
            Color curveColor = Color.FromRgb(00, 0x7A, 0xCC);
            this.curve.Stroke = new SolidColorBrush(curveColor);
            
            this.CanvasView.Children.Add(this.curve);
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string CurveViewName
        {
            get;
            set;
        }

        const double Grad = 0.3;

        private void UpdateValueAxis(double max, double min)
        {
            double scaleWidth = 30;
            this.Graduation.ClipToBounds = true;
            int textCount = 0;

            double d = this.CanvasHeight / (max - min);
            // How many graduation?
            int dc = (int)CanvasHeight / 10;
            // What's the value aach graduation 
            double ev = (max - min) / dc;

            for (int i = 0; i < 60; i++)
            {
                double y = this.CanvasHeight - (ZeroOffset) - i * 12.0;

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

                double value = min + i * ev;

                if (i % 5 == 0)
                {
                    TextBlock t = new TextBlock();
                    t.Foreground = Brushes.Black;
                    t.FontSize = 9;
                    double pos = (double)y - 6.0;

                    if (max > 10)
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
                    t.SetValue(Canvas.TopProperty, (double)pos);
                    this.Graduation.Children.Add(t);

                    textCount++;
                }
            }
        }

        public void SetPoints(int[] data)
        {
            this.Initialize();
            this.data = data;
            this.Max = data.Max() * 1.4;
            this.Min = 0;
            this.InitPointAxis();
            this.UpdateValueAxis(this.Max, this.Min);

            int count = data.Length;
            this.curve.Points.Clear();
            for (int i = 0; i < count; ++i)
            {
                Point point = new Point(i * Grad, data[i]);
                Point p;
                this.Convert(point, out p);
                curve.Points.Add(p);
            }
        }

        public void UpdatePoints(int from, int to, double grad)
        {
            this.curve.Points.Clear();
            from = Math.Max(from, 0);
            to = Math.Min(3200 - 1, to);
            for (int i = from; i < to; ++i)
            {
                Point point = new Point((i - from ) * grad, this.data[i]);
                Point p;
                this.Convert(point, out p);
                curve.Points.Add(p);
            }
        }

        public void TrackTimeLine(Point point, string timeLabel)
        {
            timeLine.X1 = timeLine.X2 = point.X;
            // this.centerX = point.X;
            this.UpdateValue(point, timeLabel);

            
        }

        private bool beginMoved = false;

        internal void MoveCurveLine(bool beginMoved)
        {
            this.beginMoved = beginMoved;
        }

        private void UpdateValue(Point point, string timeLabel)
        {
            double x = point.X;

            double xo = (x - this.centerX) / this.currentScale + this.centerX;

            double y = double.NaN;
            if (this.GetY(xo, out y))
            {
                double v = this.GetValue(y + ZeroOffset);
                v = ConvertDouble(v, 4);

                this.valueBorder.Visibility = Visibility.Visible;
                int i = (int)(xo / Grad);
                this.valueLabel.Text = string.Format("[能量:{0} 计数:{1}]", i, v);
            }
            else
            {
                this.valueBorder.Visibility = Visibility.Hidden;
            }
        }

        static double ConvertDouble(double d, int n)
        {
            if (d == 0.0) return 0;
            if (d > 1 || d < -1)
                n = n - (int)Math.Log10(Math.Abs(d)) - 1;
            else
                n = n + (int)Math.Log10(1.0 / Math.Abs(d));
            if (n < 0)
            {
                d = (int)(d / Math.Pow(10, 0 - n)) * Math.Pow(10, 0 - n);
                n = 0;
            }
            if (n > 15) // For some Channel Data, n would be over 15 and throw Exception.
                n = 15;
            return Math.Round(d, n);
        }

        private double Convert(double v)
        {
            double canvasHeight = this.CanvasHeight;
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
                return ZeroOffset;
            }

            double pos = canvasHeight / (pa / pb + 1);
            double y = canvasHeight - pos - ZeroOffset;
            return y;
        }

        private bool GetY(double x, out double y)
        {
            Point a = default(Point);
            Point b = default(Point);
            bool found = false;
            foreach (var p in curve.Points)
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
            double v = this.Max - (this.Max - this.Min) * y / this.CanvasHeight;
            return v;
        }

        private void Convert(Point p, out Point po)
        {
            po = new Point(p.X, this.Convert(p.Y));
        }

        public UIElement View
        {
            get
            {
                return (UIElement)this.CanvasView;
            }
        }

        public double Min
        {
            set;
            get;
        }

        public double Max
        {
            set;
            get;
        }

        public long PointAxisScale
        {
            get;
            internal set;
        }


        private void SetDisplayName(string displayName)
        {
            const double Top = 12.0;
            SolidColorBrush labelBrush = new SolidColorBrush(Color.FromRgb(219, 219, 219));
            
            TextBlock displayLabel = new TextBlock();
            displayLabel.Text = displayName;
            
            displayLabel.Foreground = Brushes.Black;
            displayLabel.SetValue(Canvas.RightProperty, 12.0);
            displayLabel.SetValue(Canvas.TopProperty, Top);
            this.CanvasView.Children.Add(displayLabel);
            // Value text Label.
            this.valueBorder = new Border();

            valueBorder.CornerRadius = new CornerRadius(1.0);
            valueBorder.BorderBrush = labelBrush;
            valueBorder.Padding = new Thickness(4.0, 0.0, 4.0, 3.0);
            valueBorder.SetValue(Canvas.RightProperty, 130.0);
            
            valueBorder.SetValue(Canvas.TopProperty, Top);
            this.valueLabel = new TextBlock();
            this.valueLabel.Foreground = Brushes.Black;
            this.valueLabel.TextAlignment = TextAlignment.Left;
            this.valueLabel.Width = 100;
            valueBorder.Child = valueLabel;
            this.CanvasView.Children.Add(valueBorder);
        }

        internal void ClearPoints()
        {
            this.curve.Points.Clear();
        }

        private bool mouseLeftButtonDown = false;

        private Point selBeginPoint;

        private Point selEndPoint;

        private Rectangle rangeSelectionRect;

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
                this.UpdateRange(beginPointX, endPointX);
            }
        }

        protected void OnResetClick(object sender, RoutedEventArgs e)
        {
            this.SetPoints(this.data);
        }

        private void UpdateRange(double beginPointX, double endPointX)
        {
            if (this.data == null)
                return;
            int from = (int)(beginPointX / 200 / Grad);
            int to = (int)(endPointX / 200 / Grad);

            if (from < 0)
                from = 0;
            if (to > 32)
                to = 32;
            if (from == to)
                to = from + 1;
            double grad = this.UpdatePointAxis(from, to);
            this.UpdatePoints(from * 200, to * 200, grad);
        }
    }
}

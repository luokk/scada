using System;
using System.Collections.Generic;
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

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for SmartDataPane.xaml
    /// </summary>
    public partial class SmartDataPane : UserControl
    {
        public SmartDataPane()
        {
            InitializeComponent();
        }

        private string deviceName;

        private bool isConnected;

        public string Title
        {
            get
            {
                return (string)this.DisplayName.Content;
            }
            set
            {
                if (this.deviceName == null)
                {
                    this.deviceName = value;
                }
                this.DisplayName.Content = value;
            }
        }

        private List<Label> labels = new List<Label>();

        public void Initialize(string[] names)
        {
            for (int i = 0; i < names.Length; ++i)
            {
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(30.0);
                this.CellGrid.RowDefinitions.Add(rowDef);
                Label label = new Label();
                label.Content = names[i];

                this.CellGrid.Children.Add(label);
                label.SetValue(Grid.RowProperty, i);
                label.SetValue(Grid.ColumnProperty, 0);

                Label valueLabel = new Label();
                this.labels.Add(valueLabel);

                this.CellGrid.Children.Add(valueLabel);
                valueLabel.SetValue(Grid.RowProperty, i);
                valueLabel.SetValue(Grid.ColumnProperty, 1);
            }   
        }

        public void SetData(params string[] values)
        {
            for (int i = 0; i< values.Length; ++i)
            {
                if (!this.isConnected)
                {
                    this.SetDataColor(i, Brushes.DarkGray, false);
                }
                else
                {
                    this.SetDataColor(i, Brushes.Black, false);
                }
                this.labels[i].Content = (string)values[i];
            }
        }

        public void SetDataColor(int index, Brush brush, bool bold)
        {
            this.labels[index].Foreground = brush;

            this.labels[index].FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
        }

        public void SetDataColor(int index, bool alarm)
        {
            if (!this.isConnected)
            {
                this.SetDataColor(index, Brushes.DarkGray, false);
                return;
            }

            if (alarm)
            {
                this.SetDataColor(index, Brushes.Red, true);
            }
            else
            {
                this.SetDataColor(index, Brushes.Green, true);
            }


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }

        internal void Check(string time)
        {
            DateTime d;
            if (DateTime.TryParse(time, out d))
            {
                if ((DateTime.Now - d).Ticks > 610 * 10000000L)
                {
                    this.DisplayName.Content = string.Format("{0}(未连接)", this.deviceName);
                    this.isConnected = false;
                }
                else
                {
                    this.DisplayName.Content = this.deviceName;
                    this.isConnected = true;
                }
            }
            else
            {
                this.DisplayName.Content = string.Format("{0}(未连接)", this.deviceName);
                this.isConnected = false;
            }
        }
    }
}

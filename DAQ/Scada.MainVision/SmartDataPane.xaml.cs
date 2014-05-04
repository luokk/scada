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

        public string Title
        {
            get
            {
                return (string)this.DisplayName.Content;
            }
            set
            {
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
                this.labels[i].Content = (string)values[i];
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }


    }
}

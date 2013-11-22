using System;
using System.Collections.Generic;
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

namespace Scada.MainVision
{

    public class HerePaneItemData
    {
        public string Title
        {
            get;
            set;
        }

        public string Data1
        {
            get;
            set;
        }

        public string Data2
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interaction logic for HerePaneItem.xaml
    /// </summary>
    public partial class HerePaneItem : UserControl
    {
        private HerePaneItemData data;


        public HerePaneItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get;
            set;
        }

        public string DeviceKey
        {
            get;
            set;
        }

        private void ItemLoaded(object sender, RoutedEventArgs e)
        {
            this.data = new HerePaneItemData() { Title = this.Title };
            this.itemGrid.DataContext = this.data;

            // data.Data1 = "31.4  ℃";
            // data.Data2 = "12 m/s";
        }

        public TextBlock this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.data1;
                    case 1:
                        return this.data2;
                    case 2:
                        return this.data3;
                    case 3:
                        return this.data4;
                    default:
                        return this.data1;
                }
            }
        }


        internal void SetIcon(string icon)
        {
            this.Icon.Source = new BitmapImage(new Uri("pack://application:,,,/" + icon));
            // this.Icon.ImageSource = new BitmapImage(new Uri("pack://application:,,,/" + icon));
        }

        private void OnMouseEnterRect(object sender, MouseEventArgs e)
        {
			// this.Rect.Fill = Brushes.AliceBlue;
        }

        private void OnMouseLeaveRect(object sender, MouseEventArgs e)
        {
            // this.Rect.Fill = Brushes.LightGray;
        }

        private void HidePanel(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

    }
}

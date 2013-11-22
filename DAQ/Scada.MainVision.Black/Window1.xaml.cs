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
using Scada.Controls;

namespace Scada.MainVision
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
		}

        private string message;

        public Window1(string message)
        {
            InitializeComponent();
            this.message = message;
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Message.Text = this.message;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowMoveHandler(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
	}
}

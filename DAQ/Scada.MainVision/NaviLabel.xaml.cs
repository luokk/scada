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
    /// Interaction logic for NaviLabel.xaml
    /// </summary>
    public partial class NaviLabel : UserControl
    {
        public NaviLabel()
        {
            InitializeComponent();
        }


        public string Text
        {
            get
            {
                return this.Title.Text;
            }

            set
            {
                this.Title.Text = value;
            }
        }
    }
}

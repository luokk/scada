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
    /// Interaction logic for StationInfoPage.xaml
    /// </summary>
    public partial class StationInfoPage : UserControl
    {
        public StationInfoPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string stationName = Settings.Instance.StationName;
            string status = Settings.Instance.Status;
            this.StationName.Content = string.Format("{0} ({1})", stationName, status);

            var stationId = string.Format("站点 ID: {0}", Settings.Instance.StationId);
            this.StationId.Content = stationId;
        }
    }
}

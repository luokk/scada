using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
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
    /// Interaction logic for SamplerControlPanel.xaml
    /// </summary>
    public partial class SamplerControlPanel : UserControl
    {
        public SamplerControlPanel(string deviceKey)
        {
            InitializeComponent();
            this.DeviceKey = deviceKey;
        }

        private void OnControl(object sender, RoutedEventArgs e)
        {
            if (IsStarted())
            {
            }
            else
            {
            }

            
        }

        public string DeviceKey
        {
            get;
            set;
        }

        private void IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void UpdateButtonStatus(bool started)
        {
            if (started)
            {
                this.StartButton.IsEnabled = false;
                this.StopButton.IsEnabled = true;
            }
            else
            {
                this.StartButton.IsEnabled = true;
                this.StopButton.IsEnabled = false;
            }
        }

        private bool IsStarted()
        {
            return true;
        }

    }
}

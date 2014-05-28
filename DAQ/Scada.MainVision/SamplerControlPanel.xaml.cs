using Scada.Common;
using Scada.Config;
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
using System.Windows.Threading;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for SamplerControlPanel.xaml
    /// </summary>
    public partial class SamplerControlPanel : UserControl
    {

        private DispatcherTimer dispatcherTimer;


        public SamplerControlPanel(string deviceKey)
        {
            InitializeComponent();
            this.DeviceKey = deviceKey;
        }

        private bool CheckDeviceFile()
        {
            string statusPath = ConfigPath.GetConfigFilePath("status");
            if (!Directory.Exists(statusPath))
            {
                Directory.CreateDirectory(statusPath);
            }

            string relFileName = string.Format("status\\@{0}-running", this.DeviceKey);
            string fileName = ConfigPath.GetConfigFilePath(relFileName);

            return File.Exists(fileName);
        }

        private string GetRemoteCommand(string cmd)
        {
            return string.Format("{0}:{1}", this.DeviceKey.ToLower(), cmd);
        }

        private void OnConnectButton(object sender, RoutedEventArgs e)
        {
            Command.Send(Ports.Main, GetRemoteCommand("connect"));
            this.ConnectButton.IsEnabled = false;
            this.DisconnectButton.IsEnabled = true;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimerTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        void dispatcherTimerTick(object sender, EventArgs e)
        {
            if (this.CheckDeviceFile())
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

        private void OnDisconnectButton(object sender, RoutedEventArgs e)
        {
            Command.Send(Ports.Main, GetRemoteCommand("disconnect"));
            this.ConnectButton.IsEnabled = true;
            this.DisconnectButton.IsEnabled = false;
            this.StartButton.IsEnabled = false;
            this.StopButton.IsEnabled = false;

            dispatcherTimer.Stop();
            dispatcherTimer = null;
        }

        private void OnStartButton(object sender, RoutedEventArgs e)
        {
            string cmd = string.Format("start:Sid={0}", this.SidText.Text);
            Command.Send(Ports.Main, GetRemoteCommand(cmd));

            this.StartButton.IsEnabled = false;
            this.StopButton.IsEnabled = true;
        }

        private void OnStopButton(object sender, RoutedEventArgs e)
        {
            Command.Send(Ports.Main, GetRemoteCommand("stop"));
            this.StartButton.IsEnabled = true;
            this.StopButton.IsEnabled = false;
        }

        public string DeviceKey
        {
            get;
            set;
        }

        private void IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private bool IsStarted()
        {
            return true;
        }

    }
}

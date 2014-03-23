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
    class DeviceItem
    {
        public string DisplayName { get; set; }

		public string DeviceKey { get; set; }
    }

	/// <summary>
	/// Interaction logic for DeviceListPanel.xaml
	/// </summary>
	public partial class DeviceListPanel : UserControl
	{
        private const string DeviceItemTemplate = "DeviceTreeViewItem";

		// private TreeViewItem deviceGroup;

		public DeviceListPanel()
		{
			InitializeComponent();
		}

		public void AddDevice(string deviceName, string deviceKey)
		{
            //
            Style ct = (Style)this.Resources[DeviceItemTemplate];

			TreeViewItem tvi = new TreeViewItem();
            tvi.Style = ct;
			tvi.DataContext = new DeviceItem()
            { 
                DisplayName = deviceName, 
                DeviceKey = deviceKey 
            };
            tvi.Selected += this.OnDeviceItemClick;
			tvi.Header = deviceName;
            tvi.FontSize = 14.0;
            tvi.FontFamily = new FontFamily("微软雅黑");
            this.DeviceList.Items.Add(tvi);
		}

		private void OnDeviceItemClick(object sender, RoutedEventArgs args)
		{
            TreeViewItem tvi = (TreeViewItem)sender;
            this.ClickDeviceItem(tvi.DataContext, args);
		}

		private void DeviceListLoaded(object sender, RoutedEventArgs e)
		{
			
		}



        public event EventHandler ClickDeviceItem;

        private void OnHideDeviceButton(object sender, RoutedEventArgs e)
        {
            this.MainWindow.OnHideDeviceButton(sender, e);
        }

        // Parent Window
        public MainWindow MainWindow 
        { 
            get; 
            set; 
        }
    }
}

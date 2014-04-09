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
    /// Interaction logic for ContainerPage.xaml
    /// </summary>
    public partial class ContainerPage : UserControl
    {
        public ContainerPage()
        {
            InitializeComponent();
        }

        public void AddTab(string name, string tabName, UserControl page)
        {

            TabItem tabItem = new TabItem();
            tabItem.Style = (Style)this.Resources["TabItemKey"];
            tabItem.Header = string.Format("  {0}  ", tabName);
            tabItem.Content = page;
            this.ContainerTab.Items.Add(tabItem);
        }
    }
}

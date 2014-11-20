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
    public partial class SttPanel : UserControl
    {
        private string deviceKey;

        public SttPanel(string deviceKey)
        {
            InitializeComponent();
            this.deviceKey = deviceKey;
        }

        private void DatePickerCalendarClosed(object sender, RoutedEventArgs e)
        {
            // From .Net3.5 => .Net4.0, 
            Microsoft.Windows.Controls.DatePicker picker = (Microsoft.Windows.Controls.DatePicker)sender;
            if (picker.Name == "FromDate")
            {
                DateTime? dt = picker.SelectedDate;
                if (dt.HasValue)
                {
                    this.FromDateText.Text = dt.Value.ToString();
                }

                if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
                {
                    this.FromDateText.Background = Brushes.Pink;
                }
                else
                {
                    this.FromDateText.Background = Brushes.White;
                    this.ToDateText.Background = Brushes.White;
                }
            }
            else if (picker.Name == "ToDate")
            {
                DateTime? dt = picker.SelectedDate;
                if (dt.HasValue)
                {
                    DateTime to = dt.Value.AddDays(1).AddSeconds(-1);
                    this.ToDateText.Text = to.ToString();
                }

                if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
                {
                    this.ToDateText.Background = Brushes.Pink;
                }
                else
                {
                    this.FromDateText.Background = Brushes.White;
                    this.ToDateText.Background = Brushes.White;
                }

            }

        }

        private bool ValidTimeRange(string fromDate, string toDate)
        {
            try
            {
                return DateTime.Parse(fromDate) < DateTime.Parse(toDate);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void SearchByDateRange(object sender, RoutedEventArgs e)
        {
            if (!this.ValidTimeRange(this.FromDateText.Text, this.ToDateText.Text))
            {
                this.FromDateText.Background = Brushes.Pink;
                this.ToDateText.Background = Brushes.Pink;
                return;
            }

            this.FromDateText.Background = Brushes.White;
            this.ToDateText.Background = Brushes.White;
            var dt1 = DateTime.Parse(this.FromDateText.Text);
            var dt2 = DateTime.Parse(this.ToDateText.Text);

            List<Dictionary<string, object>> data = DBDataProvider.Instance.GetSidData(this.deviceKey, dt1, dt2);
            
            this.listView.ItemsSource = null;

            this.listView.ItemsSource = data;

        }

        public ListView AddListView()
        {

            GridView gridView = new GridView();
            this.listView.View = gridView;

            var columnInfoList = new List<Scada.Controls.Data.ColumnInfo>();
            columnInfoList.Add(new Controls.Data.ColumnInfo(){ Header="采样ID", BindingName="sid"});
            columnInfoList.Add(new Controls.Data.ColumnInfo() { Header = "开始时间", BindingName = "begintime" });
            columnInfoList.Add(new Controls.Data.ColumnInfo() { Header = "结束时间", BindingName = "endtime" });
            columnInfoList.Add(new Controls.Data.ColumnInfo() { Header = "累计流量", BindingName = "volume" });
            columnInfoList.Add(new Controls.Data.ColumnInfo() { Header = "累计时间", BindingName = "hours" });

            foreach (var columnInfo in columnInfoList)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = columnInfo.Header;
                string bindingName = string.Format("[{0}]", columnInfo.BindingName);
                col.DisplayMemberBinding = new Binding(bindingName.ToLower());
                col.Width = columnInfo.Width;
                gridView.Columns.Add(col);
            }
            
            return listView;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddListView();
        }
    }
}

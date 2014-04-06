using Scada.Controls;
using Scada.Controls.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Scada.MainVision
{
    /**
     * PanelManager
     * Manage all the panel with tables.
     */
    public class PanelManager
	{
		private Grid theGrid;

		private List<ListViewPanel> panelList = new List<ListViewPanel>();

        private Dictionary<string, ListViewPanel> panelDict = new Dictionary<string, ListViewPanel>();

		private ListViewPanel currentPanel;

        public PanelManager(Grid theGrid)
		{
            this.theGrid = theGrid;
		}

        public ListViewPanel CreateDataViewPanel(DataProvider dataProvider, ConfigEntry entry, bool showList = true)
		{
            string deviceKey = entry.DeviceKey;
            string displayName = entry.DisplayName;
            if (this.panelDict.ContainsKey(deviceKey))
            {
                ListViewPanel panel = this.panelDict[deviceKey];
                if (this.currentPanel != panel)
                {
                    this.currentPanel.Visibility = Visibility.Hidden;
                }
                panel.Visibility = Visibility.Visible;
                this.currentPanel = panel;
                return panel;
            }
            else
            {
                // !
                ListViewPanel panel = new ListViewPanel(dataProvider, entry);
                DataListener dataListener = dataProvider.GetDataListener(deviceKey);
                panel.SetIcon(entry.Icon);
                panel.AddDataListener(dataListener);
                if (showList)
                {
                    panel.ListView = this.ShowListView(panel, dataListener);
                    panel.SearchView = this.ShowListView(panel, dataListener);
                    panel.GraphView = this.ShowGraphView(panel, dataListener);
                    panel.GraphSearchView = this.ShowSearchGraphView(panel, dataListener);

                    panel.ListRecentData();
                    // 是否显示 控制面板
                    if (deviceKey == DataProvider.DeviceKey_HvSampler)
                    {
                        panel.ControlPanel = this.ShowControlView(DataProvider.DeviceKey_HvSampler);
                    }
                    else if (deviceKey == DataProvider.DeviceKey_ISampler)
                    {
                        panel.ControlPanel = this.ShowControlView(DataProvider.DeviceKey_ISampler);
                    }
                    else if (deviceKey == DataProvider.DeviceKey_NaI)
                    {
                        panel.SetupContextMenu((ListView)panel.ListView);
                        panel.SetupContextMenu((ListView)panel.SearchView);
                        panel.EnergyPanel = this.ShowEnergyView(DataProvider.DeviceKey_NaI);
                    }
                }

                if (this.currentPanel != null)
                {
                    this.currentPanel.Visibility = Visibility.Hidden;
                }
                this.currentPanel = panel;

                this.panelDict.Add(deviceKey, panel);
                return panel;
            }
		}

        public ListView ShowListView(ListViewPanel panel, DataListener dataListener)
        {
            // ListView
            ListView listView = new ListView();
            GridView gridView = new GridView();
            listView.View = gridView;

            // 
            var columnInfoList = dataListener.GetColumnsInfo(); // new List<ColumnInfo>();

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

        // Real time graph
        public GraphView ShowGraphView(ListViewPanel panel, DataListener dataListener)
        {
            GraphView graphView = new GraphView();
            graphView.Interval = 30;
            if (dataListener.DeviceKey == "scada.naidevice")
            {
                graphView.Interval = 60 * 5;
            }
            graphView.AddDataListener(dataListener);

            var columnInfoList = dataListener.GetColumnsInfo();
            string deviceKey = dataListener.DeviceKey;

            foreach (var columnInfo in columnInfoList)
            {
                // Time would be deal as a Chart.
                if (columnInfo.BindingName.ToLower() == "time")
                {
                    continue; // Do nothing would be OK.
                }

                if (columnInfo.DisplayInChart)
                {
                    graphView.AddLineName(deviceKey, columnInfo.BindingName, columnInfo.Header);
                }
            }

            return graphView;
        }

        // Search graph
        public SearchGraphView ShowSearchGraphView(ListViewPanel panel, DataListener dataListener)
        {
            SearchGraphView graphView = new SearchGraphView();
            graphView.Interval = 30;
            if (dataListener.DeviceKey == "scada.naidevice")
            {
                graphView.Interval = 60 * 5;
            }
            /// graphView.AddDataListener(dataListener);

            var columnInfoList = dataListener.GetColumnsInfo();
            string deviceKey = dataListener.DeviceKey;

            foreach (var columnInfo in columnInfoList)
            {
                // Time would be deal as a Chart.
                if (columnInfo.BindingName.ToLower() == "time")
                {
                    continue;
                }

                if (columnInfo.DisplayInChart)
                {
                    graphView.AddLineName(deviceKey, columnInfo.BindingName, columnInfo.Header);
                }
            }

            return graphView;
        }

        private Control ShowControlView(string deviceKey)
        {
            SamplerControlPanel panel = new SamplerControlPanel(deviceKey);
            return panel;
        }

        private Control ShowEnergyView(string p)
        {
            EnergyPanel panel = new EnergyPanel();
            return panel;
        }

		public void SetListViewPanelPos(ListViewPanel listViewPanel, int row, int column)
		{
			listViewPanel.SetValue(Grid.ColumnProperty, column);
			listViewPanel.SetValue(Grid.RowProperty, row);
		}

        public void SetPage(Page page)
        {


            ContainerPage containerPage = new ContainerPage();

            containerPage.SetValue(Grid.ColumnProperty, 2);
            containerPage.SetValue(Grid.RowProperty, 2);
        }

		public void HideListViewPanel(ListViewPanel listViewPanel)
		{
			listViewPanel.Visibility = Visibility.Hidden;
		}

		public void CloseListViewPanel(ListViewPanel listViewPanel)
		{
			listViewPanel.Visibility = Visibility.Hidden;
		}
	}
}

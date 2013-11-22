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
     * 
     */
    public class PanelManager
	{
		private Window window;

		private List<ListViewPanel> panelList = new List<ListViewPanel>();

        private Dictionary<string, ListViewPanel> panelDict = new Dictionary<string, ListViewPanel>();

		private ListViewPanel currentPanel;

        

		public PanelManager(Window window)
		{
			this.window = window;
		}

		~PanelManager()
		{

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
                panel.AddDataListener(dataListener);
                if (showList)
                {
                    panel.ListView = this.ShowListView(panel, dataListener);
                    panel.SearchView = this.ShowListView(panel, dataListener);
                    panel.GraphView = this.ShowGraphView(panel, dataListener, true);
                    panel.GraphSearchView = this.ShowSearchGraphView(panel, dataListener, false);

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
        public GraphView ShowGraphView(ListViewPanel panel, DataListener dataListener, bool realTime)
        {
            GraphView graphView = new GraphView(realTime);
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
                    continue;
                }

                if (columnInfo.DisplayInChart)
                {
                    graphView.AddLineName(deviceKey, columnInfo.BindingName, columnInfo.Header);
                }
            }

            return graphView;
        }

        // Search graph
        public SearchGraphView ShowSearchGraphView(ListViewPanel panel, DataListener dataListener, bool realTime)
        {
            SearchGraphView graphView = new SearchGraphView(realTime);
            graphView.Interval = 30;
            if (dataListener.DeviceKey == "scada.naidevice")
            {
                graphView.Interval = 60 * 5;
            }
            // graphView.AddDataListener(dataListener);

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

		public void SetListViewPanelPos(ListViewPanel listViewPanel, int row, int column)
		{
			listViewPanel.SetValue(Grid.ColumnProperty, column);
			listViewPanel.SetValue(Grid.RowProperty, row);
		}

        public void SetGraphViewPanelPos(GraphView listViewPanel, int row, int column)
        {
            listViewPanel.SetValue(Grid.ColumnProperty, column);
            listViewPanel.SetValue(Grid.RowProperty, row);
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

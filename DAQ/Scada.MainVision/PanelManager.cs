using Scada.Common;
using Scada.Config;
using Scada.Controls;
using Scada.Controls.Data;
using System;
using System.Collections.Generic;
using System.IO;
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
        public const string StationIntroduction = "Station-Introduction";

        public const string DevicesRunStatus = "Devices-Run-Status";

        public const string CinderellaRunStatus = "Cinderella-Run-Status";

        public const string CurrentCommStatus = "Current-Comm-Status";

        public const string HistoryCommStatus = "History-Comm-Status";

        public const string DataCounter = "Station-Data-Counter";




		private Grid theGrid;

        private CinderellaPage cinderellaPage;

		private List<ListViewPanel> panelList = new List<ListViewPanel>();

        private Dictionary<string, ListViewPanel> panelDict = new Dictionary<string, ListViewPanel>();

		private UserControl currentPanel;

        public PanelManager(Grid theGrid)
		{
            this.theGrid = theGrid;
		}

        public ListViewPanel CreateDataViewPanel(DBDataProvider dataProvider, ConfigEntry entry, bool showList = true)
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

                    if (deviceKey != DataProvider.DeviceKey_Dwd && deviceKey != DataProvider.DeviceKey_Shelter)
                    {
                        panel.HasSerachDataChart = true;
                        if (deviceKey == DataProvider.DeviceKey_Hpic)
                        {
                            panel.GraphSearchView = this.ShowSearchHpicGraphView(panel, dataListener);
                        }
                        else
                        {
                            panel.GraphSearchView = this.ShowSearchGraphView(panel, dataListener);
                        }
                    }

                    if (deviceKey == DataProvider.DeviceKey_MDS)
                    {
                        panel.HasRealTimeChart = true;
                        panel.GraphView = this.ShowGraphView(panel, dataListener);
                        panel.selectedField = "flow";
                        panel.ControlPanel = this.ShowControlView(DataProvider.DeviceKey_MDS);
                    }
                    else if (deviceKey == DataProvider.DeviceKey_AIS)
                    {
                        panel.HasRealTimeChart = true;
                        panel.GraphView = this.ShowGraphView(panel, dataListener);
                        panel.selectedField = "flow";
                        panel.ControlPanel = this.ShowControlView(DataProvider.DeviceKey_AIS);
                    }
                    else if (deviceKey == DataProvider.DeviceKey_Weather)
                    {
                        panel.FieldSelect.Visibility = Visibility.Visible;
                        panel.FieldSelect.SelectedIndex = 0;
                        panel.selectedField = "temperature";
                    }
                    else if (deviceKey == DataProvider.DeviceKey_Hpic)
                    {
                        panel.IntervalSelect.Visibility = Visibility.Visible;
                        panel.IntervalSelect.SelectedIndex = 0;
                        panel.selectedField = "doserate";
                    }
                    else if (deviceKey == DataProvider.DeviceKey_NaI)
                    {
                        panel.SetupContextMenu((ListView)panel.ListView);
                        panel.SetupContextMenu((ListView)panel.SearchView);
                        panel.EnergyPanel = this.ShowEnergyView(DataProvider.DeviceKey_NaI);
                        panel.selectedField = "doserate";
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

        // RealTime GraphView (In f15 branch, only MDS and AIS has RealTime Graph...)
        public GraphView ShowGraphView(ListViewPanel panel, DataListener dataListener)
        {
            GraphView graphView = new GraphView();
            graphView.Interval = 30;

            graphView.AddDataListener(dataListener);

            var columnInfoList = dataListener.GetColumnsInfo();
            string deviceKey = dataListener.DeviceKey;

            foreach (var columnInfo in columnInfoList)
            {
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
        public SearchGraphView ShowSearchGraphView(ListViewPanel panel, DataListener dataListener)
        {
            SearchGraphView graphView = new SearchGraphView();
            if (dataListener.DeviceKey == DataProvider.DeviceKey_NaI)
            {
                graphView.Interval = 60 * 5;
            }
            else
            {
                graphView.Interval = 30;
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

        public SearchHpicGraphView ShowSearchHpicGraphView(ListViewPanel panel, DataListener dataListener)
        {
            SearchHpicGraphView graphView = new SearchHpicGraphView();
            graphView.Interval = 30;

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

        private Dictionary<string, UserControl> pageDict = new Dictionary<string, UserControl>();

        public UserControl GetPage(string name)
        {
            if (this.pageDict.ContainsKey(name))
            {
                return this.pageDict[name];
            }
            return null;
        }

        public void SetPage(string name, UserControl mainPage)
        {
            ContainerPage containerPage = (ContainerPage)this.GetPage(name);
            if (containerPage == null)
            {
                containerPage = new ContainerPage();
                this.theGrid.Children.Add(containerPage);
                containerPage.SetValue(Grid.ColumnProperty, 2);
                containerPage.SetValue(Grid.RowProperty, 2);
                
                this.pageDict.Add(name, containerPage);

                // --------------------------------------
                if (name == PanelManager.StationIntroduction)
                {
                    containerPage.AddTab(name, "自动站", mainPage);
                    // TODO: add other tab
                }
                else if (name == PanelManager.CurrentCommStatus)
                {
                    containerPage.AddTab(name, "通信状态", mainPage);
                    // TODO: add other tab
                }
                else if (name == PanelManager.HistoryCommStatus)
                {
                    containerPage.AddTab(name, "历史通信状态", mainPage);
                    // TODO: add other tab
                }
                else if (name == PanelManager.DataCounter)
                {
                    containerPage.AddTab(name, "数据统计", mainPage);
                    // TODO: add other tab
                }
                else if (name == PanelManager.DevicesRunStatus)
                {
                    containerPage.AddTab(name, "设备管理", mainPage);
                    // TODO: add other tab
                }
                else if (name == PanelManager.CinderellaRunStatus)
                {
                    containerPage.AddTab(name, "Cinderella管理", mainPage);
                }
            }
            else
            {
                containerPage.SetValue(Grid.ColumnProperty, 2);
                containerPage.SetValue(Grid.RowProperty, 2);
                containerPage.Visibility = Visibility.Visible;
            }

            if (this.currentPanel != null)
            {
                this.currentPanel.Visibility = Visibility.Hidden;
            }
            currentPanel = containerPage;
        }

		public void HideListViewPanel(ListViewPanel listViewPanel)
		{
			listViewPanel.Visibility = Visibility.Hidden;
		}

		public void CloseListViewPanel(ListViewPanel listViewPanel)
		{
			listViewPanel.Visibility = Visibility.Hidden;
		}

        internal UserControl CreatePage(string name, DBDataProvider dataProvider)
        {
            if (name == PanelManager.StationIntroduction)
            {
                return new StationInfoPage();
            }
            else if (name == PanelManager.CurrentCommStatus)
            {
                return new CommStatusPage();
            }
            else if (name == PanelManager.HistoryCommStatus)
            {
                return new CommStatusPage2();
            }
            else if (name == PanelManager.DataCounter)
            {
                return new DataCounterPane();
            }
            else if (name == PanelManager.CinderellaRunStatus)
            {
                this.cinderellaPage = new CinderellaPage();
                this.cinderellaPage.SetDataProvider(dataProvider);
                return this.cinderellaPage;
            }
            else if (name == PanelManager.DevicesRunStatus)
            {
                if (!Settings.Instance.IsCAS)
                {
                    AllDevicesPage page = new AllDevicesPage();
                    page.SetDataProvider(dataProvider);
                    return page;
                }
                else
                {
                    AllDevicesPage2 page = new AllDevicesPage2();
                    page.SetDataProvider(dataProvider);
                    return page;
                }
            }
            return null;
        }

        public void SendCommandToCinderellaPage(Command cmd)
        {
            if (this.cinderellaPage != null)
            {
                this.cinderellaPage.OnReceivedCommand(cmd);
            }
        }

        internal void SendFileCreatedToCinderellaPage(string filePath)
        {
            if (this.cinderellaPage != null)
            {
                this.cinderellaPage.OnFileCreated(filePath);
            }
        }
    }
}

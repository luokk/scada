namespace Scada.Data.Client
{
    partial class MainDataAgentWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainDataAgentWindow));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.eventTabPage = new System.Windows.Forms.TabPage();
            this.mainListBox = new System.Windows.Forms.ListBox();
            this.packTabPage = new System.Windows.Forms.TabPage();
            this.detailsListView = new System.Windows.Forms.ListView();
            this.deviceHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.countHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.percentHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.historyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.historyLatestHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.detailsButton = new System.Windows.Forms.ToolStripButton();
            this.testToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.SendHpicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendWeatherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendLabrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendHPGeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendCinderellaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendTTLModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendShelterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sysNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.操作ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.eventTabPage.SuspendLayout();
            this.packTabPage.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(844, 350);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 25);
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.RightToolStripPanel
            // 
            this.toolStripContainer1.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStripContainer1.Size = new System.Drawing.Size(844, 397);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(844, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 350);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mainTabControl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(844, 350);
            this.panel1.TabIndex = 1;
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.eventTabPage);
            this.mainTabControl.Controls.Add(this.packTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(844, 350);
            this.mainTabControl.TabIndex = 2;
            // 
            // eventTabPage
            // 
            this.eventTabPage.Controls.Add(this.mainListBox);
            this.eventTabPage.Location = new System.Drawing.Point(4, 22);
            this.eventTabPage.Name = "eventTabPage";
            this.eventTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.eventTabPage.Size = new System.Drawing.Size(836, 324);
            this.eventTabPage.TabIndex = 0;
            this.eventTabPage.Text = "事件记录";
            this.eventTabPage.UseVisualStyleBackColor = true;
            // 
            // mainListBox
            // 
            this.mainListBox.BackColor = System.Drawing.SystemColors.Info;
            this.mainListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mainListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainListBox.FormattingEnabled = true;
            this.mainListBox.ItemHeight = 12;
            this.mainListBox.Location = new System.Drawing.Point(3, 3);
            this.mainListBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 28);
            this.mainListBox.Name = "mainListBox";
            this.mainListBox.Size = new System.Drawing.Size(830, 318);
            this.mainListBox.TabIndex = 1;
            // 
            // packTabPage
            // 
            this.packTabPage.Controls.Add(this.detailsListView);
            this.packTabPage.Location = new System.Drawing.Point(4, 22);
            this.packTabPage.Name = "packTabPage";
            this.packTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.packTabPage.Size = new System.Drawing.Size(836, 349);
            this.packTabPage.TabIndex = 1;
            this.packTabPage.Text = "数据统计";
            this.packTabPage.UseVisualStyleBackColor = true;
            // 
            // detailsListView
            // 
            this.detailsListView.BackColor = System.Drawing.SystemColors.Info;
            this.detailsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.deviceHeader,
            this.countHeader,
            this.percentHeader,
            this.historyHeader,
            this.historyLatestHeader});
            this.detailsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsListView.FullRowSelect = true;
            this.detailsListView.Location = new System.Drawing.Point(3, 3);
            this.detailsListView.MultiSelect = false;
            this.detailsListView.Name = "detailsListView";
            this.detailsListView.Size = new System.Drawing.Size(830, 343);
            this.detailsListView.TabIndex = 0;
            this.detailsListView.UseCompatibleStateImageBehavior = false;
            this.detailsListView.View = System.Windows.Forms.View.Details;
            // 
            // deviceHeader
            // 
            this.deviceHeader.Text = "设备";
            this.deviceHeader.Width = 100;
            // 
            // countHeader
            // 
            this.countHeader.Text = "实时数据上传数量";
            this.countHeader.Width = 121;
            // 
            // percentHeader
            // 
            this.percentHeader.Text = "实时数据上传率(当日)";
            this.percentHeader.Width = 139;
            // 
            // historyHeader
            // 
            this.historyHeader.Text = "历史数据上传数量";
            this.historyHeader.Width = 143;
            // 
            // historyLatestHeader
            // 
            this.historyLatestHeader.Text = "历史数据最后上传时间";
            this.historyLatestHeader.Width = 207;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripSeparator1,
            this.detailsButton,
            this.testToolStripButton,
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(231, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // detailsButton
            // 
            this.detailsButton.Image = ((System.Drawing.Image)(resources.GetObject("detailsButton.Image")));
            this.detailsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.detailsButton.Name = "detailsButton";
            this.detailsButton.Size = new System.Drawing.Size(52, 22);
            this.detailsButton.Text = "详情";
            this.detailsButton.Click += new System.EventHandler(this.OnDetailsButtonClick);
            // 
            // testToolStripButton
            // 
            this.testToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("testToolStripButton.Image")));
            this.testToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.testToolStripButton.Name = "testToolStripButton";
            this.testToolStripButton.Size = new System.Drawing.Size(52, 22);
            this.testToolStripButton.Text = "同步";
            this.testToolStripButton.Click += new System.EventHandler(this.testToolStripButton_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SendHpicToolStripMenuItem,
            this.SendWeatherToolStripMenuItem,
            this.SendLabrToolStripMenuItem,
            this.SendHPGeToolStripMenuItem,
            this.SendCinderellaToolStripMenuItem,
            this.SendTTLModuleToolStripMenuItem,
            this.SendShelterToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(109, 22);
            this.toolStripDropDownButton1.Text = "数据通道测试";
            // 
            // SendHpicToolStripMenuItem
            // 
            this.SendHpicToolStripMenuItem.Name = "SendHpicToolStripMenuItem";
            this.SendHpicToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendHpicToolStripMenuItem.Tag = "scada.hpic";
            this.SendHpicToolStripMenuItem.Text = "高压电离室";
            this.SendHpicToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendWeatherToolStripMenuItem
            // 
            this.SendWeatherToolStripMenuItem.Name = "SendWeatherToolStripMenuItem";
            this.SendWeatherToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendWeatherToolStripMenuItem.Tag = "scada.weather";
            this.SendWeatherToolStripMenuItem.Text = "气象站";
            this.SendWeatherToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendLabrToolStripMenuItem
            // 
            this.SendLabrToolStripMenuItem.Name = "SendLabrToolStripMenuItem";
            this.SendLabrToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendLabrToolStripMenuItem.Tag = "scada.labr";
            this.SendLabrToolStripMenuItem.Text = "Labr谱仪";
            this.SendLabrToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendHPGeToolStripMenuItem
            // 
            this.SendHPGeToolStripMenuItem.Name = "SendHPGeToolStripMenuItem";
            this.SendHPGeToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendHPGeToolStripMenuItem.Tag = "scada.hpge";
            this.SendHPGeToolStripMenuItem.Text = "高纯锗";
            this.SendHPGeToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendCinderellaToolStripMenuItem
            // 
            this.SendCinderellaToolStripMenuItem.Name = "SendCinderellaToolStripMenuItem";
            this.SendCinderellaToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendCinderellaToolStripMenuItem.Tag = "scada.cinderella";
            this.SendCinderellaToolStripMenuItem.Text = "Cinderella";
            this.SendCinderellaToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendTTLModuleToolStripMenuItem
            // 
            this.SendTTLModuleToolStripMenuItem.Name = "SendTTLModuleToolStripMenuItem";
            this.SendTTLModuleToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendTTLModuleToolStripMenuItem.Tag = "scada.ttlm";
            this.SendTTLModuleToolStripMenuItem.Text = "TTL模块";
            this.SendTTLModuleToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // SendShelterToolStripMenuItem
            // 
            this.SendShelterToolStripMenuItem.Name = "SendShelterToolStripMenuItem";
            this.SendShelterToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.SendShelterToolStripMenuItem.Tag = "scada.shelter";
            this.SendShelterToolStripMenuItem.Text = "环境与安防监控";
            this.SendShelterToolStripMenuItem.Click += new System.EventHandler(this.SendDataToolStripMenuItem_Click);
            // 
            // sysNotifyIcon
            // 
            this.sysNotifyIcon.Text = "数据上传";
            this.sysNotifyIcon.Visible = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.操作ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(844, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.文件ToolStripMenuItem.Text = "文件";
            // 
            // 操作ToolStripMenuItem
            // 
            this.操作ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dataUploadToolStripMenuItem});
            this.操作ToolStripMenuItem.Name = "操作ToolStripMenuItem";
            this.操作ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.操作ToolStripMenuItem.Text = "操作";
            // 
            // dataUploadToolStripMenuItem
            // 
            this.dataUploadToolStripMenuItem.Checked = true;
            this.dataUploadToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dataUploadToolStripMenuItem.Name = "dataUploadToolStripMenuItem";
            this.dataUploadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.dataUploadToolStripMenuItem.Text = "数据上传";
            this.dataUploadToolStripMenuItem.Click += new System.EventHandler(this.dataUploadToolStripMenuItem_Click);
            // 
            // MainDataAgentWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(844, 422);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainDataAgentWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据上传 v2.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AgentWindowClosingForm);
            this.Load += new System.EventHandler(this.AgentWindow_Load);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.mainTabControl.ResumeLayout(false);
            this.eventTabPage.ResumeLayout(false);
            this.packTabPage.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox mainListBox;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripButton detailsButton;
        private System.Windows.Forms.NotifyIcon sysNotifyIcon;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage eventTabPage;
        private System.Windows.Forms.TabPage packTabPage;
        private System.Windows.Forms.ListView detailsListView;
        private System.Windows.Forms.ColumnHeader deviceHeader;
        private System.Windows.Forms.ColumnHeader historyHeader;
        private System.Windows.Forms.ColumnHeader countHeader;
        private System.Windows.Forms.ColumnHeader percentHeader;
        private System.Windows.Forms.ColumnHeader historyLatestHeader;
        private System.Windows.Forms.ToolStripButton testToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem SendHpicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendWeatherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendLabrToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendHPGeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendCinderellaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendTTLModuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SendShelterToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 操作ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataUploadToolStripMenuItem;

    }
}


namespace Scada.Data.Client.Tcp
{
    partial class AgentWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentWindow));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.MainConnStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SubConnStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainTabCtrl = new System.Windows.Forms.TabControl();
            this.connPage = new System.Windows.Forms.TabPage();
            this.mainListBox = new System.Windows.Forms.ListBox();
            this.historyPage = new System.Windows.Forms.TabPage();
            this.connHistoryList = new System.Windows.Forms.ListBox();
            this.dataPage = new System.Windows.Forms.TabPage();
            this.detailsListView = new System.Windows.Forms.ListView();
            this.deviceCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.countCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timeCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.historyTimeCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.startStripButton = new System.Windows.Forms.ToolStripButton();
            this.loggerStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.OpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setTimeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.SetExceptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.QuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DispToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LoggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sysNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mainTabCtrl.SuspendLayout();
            this.connPage.SuspendLayout();
            this.historyPage.SuspendLayout();
            this.dataPage.SuspendLayout();
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(520, 351);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.RightToolStripPanel
            // 
            this.toolStripContainer1.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStripContainer1.Size = new System.Drawing.Size(520, 422);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MainConnStatusLabel,
            this.SubConnStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(520, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // MainConnStatusLabel
            // 
            this.MainConnStatusLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.MainConnStatusLabel.Name = "MainConnStatusLabel";
            this.MainConnStatusLabel.Size = new System.Drawing.Size(135, 17);
            this.MainConnStatusLabel.Text = "省中心连接状态: 未连接";
            // 
            // SubConnStatusLabel
            // 
            this.SubConnStatusLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.SubConnStatusLabel.Name = "SubConnStatusLabel";
            this.SubConnStatusLabel.Size = new System.Drawing.Size(147, 17);
            this.SubConnStatusLabel.Text = "国家中心连接状态: 未连接";
            this.SubConnStatusLabel.Click += new System.EventHandler(this.SubConnStatusLabel_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 351);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mainTabCtrl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 351);
            this.panel1.TabIndex = 1;
            // 
            // mainTabCtrl
            // 
            this.mainTabCtrl.Controls.Add(this.connPage);
            this.mainTabCtrl.Controls.Add(this.historyPage);
            this.mainTabCtrl.Controls.Add(this.dataPage);
            this.mainTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabCtrl.Location = new System.Drawing.Point(0, 0);
            this.mainTabCtrl.Name = "mainTabCtrl";
            this.mainTabCtrl.SelectedIndex = 0;
            this.mainTabCtrl.Size = new System.Drawing.Size(520, 351);
            this.mainTabCtrl.TabIndex = 4;
            this.mainTabCtrl.SelectedIndexChanged += new System.EventHandler(this.mainTabCtrl_SelectedIndexChanged);
            // 
            // connPage
            // 
            this.connPage.Controls.Add(this.mainListBox);
            this.connPage.Location = new System.Drawing.Point(4, 22);
            this.connPage.Name = "connPage";
            this.connPage.Padding = new System.Windows.Forms.Padding(3);
            this.connPage.Size = new System.Drawing.Size(512, 325);
            this.connPage.TabIndex = 0;
            this.connPage.Text = "处理事件";
            this.connPage.UseVisualStyleBackColor = true;
            // 
            // mainListBox
            // 
            this.mainListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainListBox.FormattingEnabled = true;
            this.mainListBox.ItemHeight = 12;
            this.mainListBox.Location = new System.Drawing.Point(3, 3);
            this.mainListBox.Name = "mainListBox";
            this.mainListBox.Size = new System.Drawing.Size(506, 319);
            this.mainListBox.TabIndex = 0;
            // 
            // historyPage
            // 
            this.historyPage.Controls.Add(this.connHistoryList);
            this.historyPage.Location = new System.Drawing.Point(4, 22);
            this.historyPage.Name = "historyPage";
            this.historyPage.Padding = new System.Windows.Forms.Padding(3);
            this.historyPage.Size = new System.Drawing.Size(512, 325);
            this.historyPage.TabIndex = 1;
            this.historyPage.Text = "连接历史";
            this.historyPage.UseVisualStyleBackColor = true;
            // 
            // connHistoryList
            // 
            this.connHistoryList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connHistoryList.FormattingEnabled = true;
            this.connHistoryList.ItemHeight = 12;
            this.connHistoryList.Location = new System.Drawing.Point(3, 3);
            this.connHistoryList.Name = "connHistoryList";
            this.connHistoryList.Size = new System.Drawing.Size(506, 319);
            this.connHistoryList.TabIndex = 0;
            // 
            // dataPage
            // 
            this.dataPage.Controls.Add(this.detailsListView);
            this.dataPage.Location = new System.Drawing.Point(4, 22);
            this.dataPage.Name = "dataPage";
            this.dataPage.Padding = new System.Windows.Forms.Padding(3);
            this.dataPage.Size = new System.Drawing.Size(512, 325);
            this.dataPage.TabIndex = 2;
            this.dataPage.Text = "数据上传";
            this.dataPage.UseVisualStyleBackColor = true;
            // 
            // detailsListView
            // 
            this.detailsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.deviceCol,
            this.countCol,
            this.timeCol,
            this.historyTimeCol});
            this.detailsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsListView.FullRowSelect = true;
            this.detailsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.detailsListView.Location = new System.Drawing.Point(3, 3);
            this.detailsListView.MultiSelect = false;
            this.detailsListView.Name = "detailsListView";
            this.detailsListView.Size = new System.Drawing.Size(506, 319);
            this.detailsListView.TabIndex = 0;
            this.detailsListView.UseCompatibleStateImageBehavior = false;
            this.detailsListView.View = System.Windows.Forms.View.Details;
            // 
            // deviceCol
            // 
            this.deviceCol.Text = "设备";
            this.deviceCol.Width = 119;
            // 
            // countCol
            // 
            this.countCol.Text = "发送数量";
            this.countCol.Width = 71;
            // 
            // timeCol
            // 
            this.timeCol.Text = "最新上传时间";
            this.timeCol.Width = 144;
            // 
            // historyTimeCol
            // 
            this.historyTimeCol.Text = "最新历史数据上传时间";
            this.historyTimeCol.Width = 171;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startStripButton,
            this.loggerStripButton1});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(5, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(105, 24);
            this.toolStrip1.TabIndex = 1;
            // 
            // startStripButton
            // 
            this.startStripButton.Image = ((System.Drawing.Image)(resources.GetObject("startStripButton.Image")));
            this.startStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startStripButton.Name = "startStripButton";
            this.startStripButton.Size = new System.Drawing.Size(52, 21);
            this.startStripButton.Text = "启动";
            this.startStripButton.Click += new System.EventHandler(this.startStripButton_Click);
            // 
            // loggerStripButton1
            // 
            this.loggerStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("loggerStripButton1.Image")));
            this.loggerStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loggerStripButton1.Name = "loggerStripButton1";
            this.loggerStripButton1.Size = new System.Drawing.Size(52, 21);
            this.loggerStripButton1.Tag = "";
            this.loggerStripButton1.Text = "日志";
            this.loggerStripButton1.Click += new System.EventHandler(this.loggerStripButton1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpToolStripMenuItem,
            this.DispToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 24);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(520, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // OpToolStripMenuItem
            // 
            this.OpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartToolStripMenuItem,
            this.setTimeToolStripMenuItem1,
            this.SetExceptionToolStripMenuItem,
            this.toolStripSeparator2,
            this.QuitToolStripMenuItem});
            this.OpToolStripMenuItem.Name = "OpToolStripMenuItem";
            this.OpToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.OpToolStripMenuItem.Text = "操作";
            // 
            // StartToolStripMenuItem
            // 
            this.StartToolStripMenuItem.Name = "StartToolStripMenuItem";
            this.StartToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.StartToolStripMenuItem.Text = "启动";
            this.StartToolStripMenuItem.Click += new System.EventHandler(this.StartToolStripMenuItem_Click);
            // 
            // setTimeToolStripMenuItem1
            // 
            this.setTimeToolStripMenuItem1.CheckOnClick = true;
            this.setTimeToolStripMenuItem1.Name = "setTimeToolStripMenuItem1";
            this.setTimeToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.setTimeToolStripMenuItem1.Text = "处理校时";
            this.setTimeToolStripMenuItem1.Click += new System.EventHandler(this.SetTimeToolStripMenuItemClick);
            // 
            // SetExceptionToolStripMenuItem
            // 
            this.SetExceptionToolStripMenuItem.CheckOnClick = true;
            this.SetExceptionToolStripMenuItem.Name = "SetExceptionToolStripMenuItem";
            this.SetExceptionToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.SetExceptionToolStripMenuItem.Text = "异常模拟";
            this.SetExceptionToolStripMenuItem.Click += new System.EventHandler(this.SetExceptionToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(121, 6);
            // 
            // QuitToolStripMenuItem
            // 
            this.QuitToolStripMenuItem.Name = "QuitToolStripMenuItem";
            this.QuitToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.QuitToolStripMenuItem.Text = "退出";
            this.QuitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // DispToolStripMenuItem
            // 
            this.DispToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LoggerToolStripMenuItem,
            this.ClsToolStripMenuItem1});
            this.DispToolStripMenuItem.Name = "DispToolStripMenuItem";
            this.DispToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.DispToolStripMenuItem.Text = "显示";
            // 
            // LoggerToolStripMenuItem
            // 
            this.LoggerToolStripMenuItem.Name = "LoggerToolStripMenuItem";
            this.LoggerToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.LoggerToolStripMenuItem.Text = "日志";
            this.LoggerToolStripMenuItem.Click += new System.EventHandler(this.LoggerToolStripMenuItem_Click);
            // 
            // ClsToolStripMenuItem1
            // 
            this.ClsToolStripMenuItem1.Name = "ClsToolStripMenuItem1";
            this.ClsToolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
            this.ClsToolStripMenuItem1.Text = "清屏";
            // 
            // sysNotifyIcon
            // 
            this.sysNotifyIcon.Text = "数据上传";
            this.sysNotifyIcon.Visible = true;
            // 
            // AgentWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(520, 422);
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "AgentWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据中心代理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AgentWindow_FormClosing);
            this.Load += new System.EventHandler(this.AgentWindowLoad);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.mainTabCtrl.ResumeLayout(false);
            this.connPage.ResumeLayout(false);
            this.historyPage.ResumeLayout(false);
            this.dataPage.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.NotifyIcon sysNotifyIcon;
        private System.Windows.Forms.TabControl mainTabCtrl;
        private System.Windows.Forms.TabPage connPage;
        private System.Windows.Forms.TabPage historyPage;
        private System.Windows.Forms.ListBox mainListBox;
        private System.Windows.Forms.TabPage dataPage;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem OpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetExceptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem QuitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DispToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem LoggerToolStripMenuItem;
        private System.Windows.Forms.ListView detailsListView;
        private System.Windows.Forms.ColumnHeader deviceCol;
        private System.Windows.Forms.ColumnHeader timeCol;
        private System.Windows.Forms.ColumnHeader historyTimeCol;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton startStripButton;
        private System.Windows.Forms.ToolStripButton loggerStripButton1;
        private System.Windows.Forms.ListBox connHistoryList;
        private System.Windows.Forms.ColumnHeader countCol;
        private System.Windows.Forms.ToolStripMenuItem setTimeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel MainConnStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel SubConnStatusLabel;

    }
}


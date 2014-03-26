namespace Scada.Main
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.sysNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.operateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.stopMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMainVisionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataCenterSetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logBankMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logDelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.docMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.startToolBarButton = new System.Windows.Forms.ToolStripButton();
            this.stopToolBarButton = new System.Windows.Forms.ToolStripButton();
            this.dataUploadItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.loggerServer = new System.Windows.Forms.ToolStripButton();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.deviceListView = new System.Windows.Forms.ListView();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // sysNotifyIcon
            // 
            this.sysNotifyIcon.Text = "notifyIcon1";
            this.sysNotifyIcon.Visible = true;
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.operateMenuItem,
            this.dataMenuItem,
            this.logMenuItem,
            this.helpMenuItem});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(501, 24);
            this.menuBar.TabIndex = 2;
            this.menuBar.Text = "menuStrip1";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(43, 20);
            this.fileMenuItem.Text = "文件";
            this.fileMenuItem.Click += new System.EventHandler(this.fileMenuItem_Click);
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.settingToolStripMenuItem.Text = "设置";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitMenuItem.Text = "退出";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // operateMenuItem
            // 
            this.operateMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startMenuItem,
            this.StartAllToolStripMenuItem2,
            this.stopMenuItem,
            this.toolStripMenuItem1,
            this.selectAllToolStripMenuItem});
            this.operateMenuItem.Name = "operateMenuItem";
            this.operateMenuItem.Size = new System.Drawing.Size(43, 20);
            this.operateMenuItem.Text = "操作";
            // 
            // startMenuItem
            // 
            this.startMenuItem.Name = "startMenuItem";
            this.startMenuItem.Size = new System.Drawing.Size(152, 22);
            this.startMenuItem.Text = "启动";
            this.startMenuItem.Click += new System.EventHandler(this.startMenuItem_Click);
            // 
            // StartAllToolStripMenuItem2
            // 
            this.StartAllToolStripMenuItem2.Name = "StartAllToolStripMenuItem2";
            this.StartAllToolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.StartAllToolStripMenuItem2.Text = "启动全部";
            this.StartAllToolStripMenuItem2.Click += new System.EventHandler(this.StartAllToolStripMenuItem2_Click);
            // 
            // stopMenuItem
            // 
            this.stopMenuItem.Name = "stopMenuItem";
            this.stopMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stopMenuItem.Text = "停止";
            this.stopMenuItem.Click += new System.EventHandler(this.stopMenuItem_Click);
            // 
            // dataMenuItem
            // 
            this.dataMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startMainVisionMenuItem,
            this.dataUploadToolStripMenuItem,
            this.dataCenterSetMenuItem});
            this.dataMenuItem.Name = "dataMenuItem";
            this.dataMenuItem.Size = new System.Drawing.Size(43, 20);
            this.dataMenuItem.Text = "数据";
            // 
            // startMainVisionMenuItem
            // 
            this.startMainVisionMenuItem.Name = "startMainVisionMenuItem";
            this.startMainVisionMenuItem.Size = new System.Drawing.Size(146, 22);
            this.startMainVisionMenuItem.Text = "启动数据视图";
            this.startMainVisionMenuItem.Click += new System.EventHandler(this.startMainVisionMenuItem_Click);
            // 
            // dataUploadToolStripMenuItem
            // 
            this.dataUploadToolStripMenuItem.Name = "dataUploadToolStripMenuItem";
            this.dataUploadToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.dataUploadToolStripMenuItem.Text = "数据上传";
            this.dataUploadToolStripMenuItem.Click += new System.EventHandler(this.dataUploadToolStripMenuItem_Click);
            // 
            // dataCenterSetMenuItem
            // 
            this.dataCenterSetMenuItem.Name = "dataCenterSetMenuItem";
            this.dataCenterSetMenuItem.Size = new System.Drawing.Size(146, 22);
            this.dataCenterSetMenuItem.Text = "数据中心设置";
            // 
            // logMenuItem
            // 
            this.logMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolMenuItem,
            this.logBankMenuItem,
            this.logDelMenuItem});
            this.logMenuItem.Name = "logMenuItem";
            this.logMenuItem.Size = new System.Drawing.Size(43, 20);
            this.logMenuItem.Text = "日志";
            // 
            // logToolMenuItem
            // 
            this.logToolMenuItem.Name = "logToolMenuItem";
            this.logToolMenuItem.Size = new System.Drawing.Size(146, 22);
            this.logToolMenuItem.Text = "日志分析工具";
            this.logToolMenuItem.Click += new System.EventHandler(this.logToolMenuItem_Click);
            // 
            // logBankMenuItem
            // 
            this.logBankMenuItem.Name = "logBankMenuItem";
            this.logBankMenuItem.Size = new System.Drawing.Size(146, 22);
            this.logBankMenuItem.Text = "备份日志";
            this.logBankMenuItem.Click += new System.EventHandler(this.logBankMenuItem_Click);
            // 
            // logDelMenuItem
            // 
            this.logDelMenuItem.Name = "logDelMenuItem";
            this.logDelMenuItem.Size = new System.Drawing.Size(146, 22);
            this.logDelMenuItem.Text = "删除日志";
            this.logDelMenuItem.Click += new System.EventHandler(this.logDelMenuItem_Click);
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.docMenuItem,
            this.toolStripSeparator2,
            this.aboutMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(43, 20);
            this.helpMenuItem.Text = "帮助";
            // 
            // docMenuItem
            // 
            this.docMenuItem.Name = "docMenuItem";
            this.docMenuItem.Size = new System.Drawing.Size(98, 22);
            this.docMenuItem.Text = "文档";
            this.docMenuItem.Click += new System.EventHandler(this.docMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(95, 6);
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(98, 22);
            this.aboutMenuItem.Text = "关于";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(150, 150);
            // 
            // toolBar
            // 
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolBarButton,
            this.stopToolBarButton,
            this.dataUploadItem,
            this.toolStripButton2,
            this.loggerServer});
            this.toolBar.Location = new System.Drawing.Point(0, 24);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(501, 28);
            this.toolBar.TabIndex = 3;
            this.toolBar.Text = "toolStrip1";
            // 
            // startToolBarButton
            // 
            this.startToolBarButton.Image = ((System.Drawing.Image)(resources.GetObject("startToolBarButton.Image")));
            this.startToolBarButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startToolBarButton.Name = "startToolBarButton";
            this.startToolBarButton.Size = new System.Drawing.Size(51, 25);
            this.startToolBarButton.Text = "启动";
            this.startToolBarButton.Click += new System.EventHandler(this.startToolBarButton_Click);
            // 
            // stopToolBarButton
            // 
            this.stopToolBarButton.Image = ((System.Drawing.Image)(resources.GetObject("stopToolBarButton.Image")));
            this.stopToolBarButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopToolBarButton.Name = "stopToolBarButton";
            this.stopToolBarButton.Size = new System.Drawing.Size(51, 25);
            this.stopToolBarButton.Text = "停止";
            this.stopToolBarButton.Click += new System.EventHandler(this.stopMenuItem_Click);
            // 
            // dataUploadItem
            // 
            this.dataUploadItem.Image = ((System.Drawing.Image)(resources.GetObject("dataUploadItem.Image")));
            this.dataUploadItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dataUploadItem.Name = "dataUploadItem";
            this.dataUploadItem.Size = new System.Drawing.Size(75, 25);
            this.dataUploadItem.Text = "数据上传";
            this.dataUploadItem.Click += new System.EventHandler(this.dataUploadItem_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(51, 20);
            this.toolStripButton2.Text = "设置";
            this.toolStripButton2.Click += new System.EventHandler(this.settingClick);
            // 
            // loggerServer
            // 
            this.loggerServer.Image = ((System.Drawing.Image)(resources.GetObject("loggerServer.Image")));
            this.loggerServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loggerServer.Name = "loggerServer";
            this.loggerServer.Size = new System.Drawing.Size(51, 25);
            this.loggerServer.Text = "日志";
            this.loggerServer.Click += new System.EventHandler(this.loggerServer_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.toolStripStatusLabel1});
            this.statusBar.Location = new System.Drawing.Point(0, 482);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(501, 22);
            this.statusBar.TabIndex = 4;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(55, 17);
            this.statusLabel.Text = "系统就绪";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStripStatusLabel1.IsLink = true;
            this.toolStripStatusLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            this.toolStripStatusLabel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(31, 17);
            this.toolStripStatusLabel1.Text = "帮助";
            // 
            // deviceListView
            // 
            this.deviceListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.deviceListView.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.deviceListView.CheckBoxes = true;
            this.deviceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceListView.FullRowSelect = true;
            this.deviceListView.HideSelection = false;
            this.deviceListView.Location = new System.Drawing.Point(0, 52);
            this.deviceListView.Margin = new System.Windows.Forms.Padding(5);
            this.deviceListView.Name = "deviceListView";
            this.deviceListView.Size = new System.Drawing.Size(501, 430);
            this.deviceListView.TabIndex = 0;
            this.deviceListView.UseCompatibleStateImageBehavior = false;
            this.deviceListView.View = System.Windows.Forms.View.Details;
            this.deviceListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.deviceListView_ItemCheck);
            this.deviceListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.deviceListView_ItemChecked);
            this.deviceListView.SelectedIndexChanged += new System.EventHandler(this.deviceListView_SelectedIndexChanged);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Checked = true;
            this.selectAllToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.selectAllToolStripMenuItem.Text = "选择全部";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 504);
            this.Controls.Add(this.deviceListView);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.menuBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuBar;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "系统设备管理器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon sysNotifyIcon;
		private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
		private System.Windows.Forms.ToolStripMenuItem operateMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopMenuItem;
		private System.Windows.Forms.ToolStripMenuItem dataMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startMainVisionMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logToolMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logBankMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logDelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
		private System.Windows.Forms.ToolStripMenuItem docMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ListView deviceListView;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.ToolStrip toolBar;
		private System.Windows.Forms.StatusStrip statusBar;
		private System.Windows.Forms.ToolStripMenuItem dataCenterSetMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton stopToolBarButton;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripButton loggerServer;
        private System.Windows.Forms.ToolStripMenuItem StartAllToolStripMenuItem2;
        private System.Windows.Forms.ToolStripButton startToolBarButton;
        private System.Windows.Forms.ToolStripButton dataUploadItem;
        private System.Windows.Forms.ToolStripMenuItem dataUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
    }
}


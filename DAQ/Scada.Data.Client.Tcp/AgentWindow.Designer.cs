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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainTabCtrl = new System.Windows.Forms.TabControl();
            this.connPage = new System.Windows.Forms.TabPage();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.historyPage = new System.Windows.Forms.TabPage();
            this.dataPage = new System.Windows.Forms.TabPage();
            this.tabDevices = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.checkBoxUpdateNaI = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.OpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.QuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DispToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sysNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mainTabCtrl.SuspendLayout();
            this.connPage.SuspendLayout();
            this.dataPage.SuspendLayout();
            this.tabDevices.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(875, 411);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.RightToolStripPanel
            // 
            this.toolStripContainer1.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStripContainer1.Size = new System.Drawing.Size(875, 457);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(875, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 411);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mainTabCtrl);
            this.panel1.Controls.Add(this.checkBoxUpdateNaI);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(875, 411);
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
            this.mainTabCtrl.Size = new System.Drawing.Size(875, 411);
            this.mainTabCtrl.TabIndex = 4;
            this.mainTabCtrl.SelectedIndexChanged += new System.EventHandler(this.mainTabCtrl_SelectedIndexChanged);
            // 
            // connPage
            // 
            this.connPage.Controls.Add(this.listBox1);
            this.connPage.Location = new System.Drawing.Point(4, 22);
            this.connPage.Name = "connPage";
            this.connPage.Padding = new System.Windows.Forms.Padding(3);
            this.connPage.Size = new System.Drawing.Size(867, 385);
            this.connPage.TabIndex = 0;
            this.connPage.Text = "连接信息";
            this.connPage.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(3, 3);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(861, 379);
            this.listBox1.TabIndex = 0;
            // 
            // historyPage
            // 
            this.historyPage.Location = new System.Drawing.Point(4, 22);
            this.historyPage.Name = "historyPage";
            this.historyPage.Padding = new System.Windows.Forms.Padding(3);
            this.historyPage.Size = new System.Drawing.Size(867, 385);
            this.historyPage.TabIndex = 1;
            this.historyPage.Text = "连接历史";
            this.historyPage.UseVisualStyleBackColor = true;
            // 
            // dataPage
            // 
            this.dataPage.Controls.Add(this.tabDevices);
            this.dataPage.Location = new System.Drawing.Point(4, 22);
            this.dataPage.Name = "dataPage";
            this.dataPage.Padding = new System.Windows.Forms.Padding(3);
            this.dataPage.Size = new System.Drawing.Size(867, 385);
            this.dataPage.TabIndex = 2;
            this.dataPage.Text = "数据上传详情";
            this.dataPage.UseVisualStyleBackColor = true;
            // 
            // tabDevices
            // 
            this.tabDevices.Controls.Add(this.tabPage1);
            this.tabDevices.Controls.Add(this.tabPage2);
            this.tabDevices.Controls.Add(this.tabPage3);
            this.tabDevices.Controls.Add(this.tabPage4);
            this.tabDevices.Controls.Add(this.tabPage5);
            this.tabDevices.Controls.Add(this.tabPage6);
            this.tabDevices.Controls.Add(this.tabPage7);
            this.tabDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDevices.Location = new System.Drawing.Point(3, 3);
            this.tabDevices.Name = "tabDevices";
            this.tabDevices.SelectedIndex = 0;
            this.tabDevices.Size = new System.Drawing.Size(861, 379);
            this.tabDevices.TabIndex = 2;
            this.tabDevices.SelectedIndexChanged += new System.EventHandler(this.tabDeviceDataSelectionChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(853, 353);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "高压电离室";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(853, 353);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "碘化钠谱仪";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(853, 353);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "气象站";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(853, 353);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "超大流量气溶胶采样器";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(853, 353);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "碘采样器";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(853, 353);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "环境与安防监控";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // tabPage7
            // 
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(853, 353);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "干湿沉降采集器";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // checkBoxUpdateNaI
            // 
            this.checkBoxUpdateNaI.AutoSize = true;
            this.checkBoxUpdateNaI.Checked = true;
            this.checkBoxUpdateNaI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUpdateNaI.Location = new System.Drawing.Point(12, 379);
            this.checkBoxUpdateNaI.Name = "checkBoxUpdateNaI";
            this.checkBoxUpdateNaI.Size = new System.Drawing.Size(91, 17);
            this.checkBoxUpdateNaI.TabIndex = 3;
            this.checkBoxUpdateNaI.Text = "上传NaI数据";
            this.checkBoxUpdateNaI.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpToolStripMenuItem,
            this.DispToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(875, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // OpToolStripMenuItem
            // 
            this.OpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartToolStripMenuItem,
            this.PauseToolStripMenuItem,
            this.toolStripSeparator2,
            this.QuitToolStripMenuItem});
            this.OpToolStripMenuItem.Name = "OpToolStripMenuItem";
            this.OpToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.OpToolStripMenuItem.Text = "操作";
            // 
            // StartToolStripMenuItem
            // 
            this.StartToolStripMenuItem.Name = "StartToolStripMenuItem";
            this.StartToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.StartToolStripMenuItem.Text = "启动";
            this.StartToolStripMenuItem.Click += new System.EventHandler(this.StartToolStripMenuItem_Click);
            // 
            // PauseToolStripMenuItem
            // 
            this.PauseToolStripMenuItem.Name = "PauseToolStripMenuItem";
            this.PauseToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.PauseToolStripMenuItem.Text = "暂停";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(97, 6);
            // 
            // QuitToolStripMenuItem
            // 
            this.QuitToolStripMenuItem.Name = "QuitToolStripMenuItem";
            this.QuitToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.QuitToolStripMenuItem.Text = "退出";
            this.QuitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // DispToolStripMenuItem
            // 
            this.DispToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClsToolStripMenuItem1});
            this.DispToolStripMenuItem.Name = "DispToolStripMenuItem";
            this.DispToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.DispToolStripMenuItem.Text = "显示";
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(875, 457);
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "AgentWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据中心代理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AgentWindow_FormClosing);
            this.Load += new System.EventHandler(this.AgentWindow_Load);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.mainTabCtrl.ResumeLayout(false);
            this.connPage.ResumeLayout(false);
            this.dataPage.ResumeLayout(false);
            this.tabDevices.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.NotifyIcon sysNotifyIcon;
        private System.Windows.Forms.CheckBox checkBoxUpdateNaI;
        private System.Windows.Forms.TabControl mainTabCtrl;
        private System.Windows.Forms.TabPage connPage;
        private System.Windows.Forms.TabPage historyPage;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TabPage dataPage;
        private System.Windows.Forms.TabControl tabDevices;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem OpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem QuitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DispToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClsToolStripMenuItem1;

    }
}


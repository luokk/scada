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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.packTabPage = new System.Windows.Forms.TabPage();
            this.dataListView = new System.Windows.Forms.ListView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.detailsButton = new System.Windows.Forms.ToolStripButton();
            this.sysNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.eventTabPage.SuspendLayout();
            this.packTabPage.SuspendLayout();
            this.toolStrip1.SuspendLayout();
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(687, 410);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.RightToolStripPanel
            // 
            this.toolStripContainer1.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStripContainer1.Size = new System.Drawing.Size(687, 457);
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
            this.statusStrip.Size = new System.Drawing.Size(687, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 410);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mainTabControl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(687, 410);
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
            this.mainTabControl.Size = new System.Drawing.Size(687, 410);
            this.mainTabControl.TabIndex = 2;
            // 
            // eventTabPage
            // 
            this.eventTabPage.Controls.Add(this.listBox1);
            this.eventTabPage.Location = new System.Drawing.Point(4, 22);
            this.eventTabPage.Name = "eventTabPage";
            this.eventTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.eventTabPage.Size = new System.Drawing.Size(679, 384);
            this.eventTabPage.TabIndex = 0;
            this.eventTabPage.Text = "事件记录";
            this.eventTabPage.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(3, 3);
            this.listBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 30);
            this.listBox1.MultiColumn = true;
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(673, 378);
            this.listBox1.TabIndex = 1;
            // 
            // packTabPage
            // 
            this.packTabPage.Controls.Add(this.dataListView);
            this.packTabPage.Location = new System.Drawing.Point(4, 22);
            this.packTabPage.Name = "packTabPage";
            this.packTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.packTabPage.Size = new System.Drawing.Size(679, 384);
            this.packTabPage.TabIndex = 1;
            this.packTabPage.Text = "数据统计";
            this.packTabPage.UseVisualStyleBackColor = true;
            // 
            // dataListView
            // 
            this.dataListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataListView.Location = new System.Drawing.Point(3, 3);
            this.dataListView.Name = "dataListView";
            this.dataListView.Size = new System.Drawing.Size(673, 378);
            this.dataListView.TabIndex = 0;
            this.dataListView.UseCompatibleStateImageBehavior = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripSeparator1,
            this.detailsButton});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(71, 25);
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
            this.detailsButton.Size = new System.Drawing.Size(53, 22);
            this.detailsButton.Text = "详情";
            this.detailsButton.Click += new System.EventHandler(this.OnDetailsButtonClick);
            // 
            // sysNotifyIcon
            // 
            this.sysNotifyIcon.Text = "数据上传";
            this.sysNotifyIcon.Visible = true;
            // 
            // MainDataAgentWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(687, 457);
            this.Controls.Add(this.toolStripContainer1);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripButton detailsButton;
        private System.Windows.Forms.NotifyIcon sysNotifyIcon;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage eventTabPage;
        private System.Windows.Forms.TabPage packTabPage;
        private System.Windows.Forms.ListView dataListView;

    }
}


namespace Scada.Watch
{
    partial class WatchForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonWatch = new System.Windows.Forms.Button();
            this.buttonPath = new System.Windows.Forms.Button();
            this.textPath = new System.Windows.Forms.TextBox();
            this.watchNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.aisCheck = new System.Windows.Forms.CheckBox();
            this.mdsCheck = new System.Windows.Forms.CheckBox();
            this.versionCheck = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.updateStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonWatch);
            this.groupBox2.Controls.Add(this.buttonPath);
            this.groupBox2.Controls.Add(this.textPath);
            this.groupBox2.Location = new System.Drawing.Point(12, 214);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(359, 87);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "自动更新";
            // 
            // buttonWatch
            // 
            this.buttonWatch.Location = new System.Drawing.Point(270, 51);
            this.buttonWatch.Name = "buttonWatch";
            this.buttonWatch.Size = new System.Drawing.Size(75, 23);
            this.buttonWatch.TabIndex = 2;
            this.buttonWatch.Text = "监控目录";
            this.buttonWatch.UseVisualStyleBackColor = true;
            this.buttonWatch.Click += new System.EventHandler(this.buttonWatch_Click);
            // 
            // buttonPath
            // 
            this.buttonPath.Location = new System.Drawing.Point(301, 19);
            this.buttonPath.Name = "buttonPath";
            this.buttonPath.Size = new System.Drawing.Size(44, 23);
            this.buttonPath.TabIndex = 1;
            this.buttonPath.Text = "...";
            this.buttonPath.UseVisualStyleBackColor = true;
            this.buttonPath.Click += new System.EventHandler(this.buttonPathClick);
            // 
            // textPath
            // 
            this.textPath.Location = new System.Drawing.Point(7, 20);
            this.textPath.Name = "textPath";
            this.textPath.Size = new System.Drawing.Size(287, 20);
            this.textPath.TabIndex = 0;
            // 
            // watchNotifyIcon
            // 
            this.watchNotifyIcon.Text = "notifyIcon1";
            this.watchNotifyIcon.Visible = true;
            this.watchNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.watchNotifyIconDoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.aisCheck);
            this.groupBox1.Controls.Add(this.mdsCheck);
            this.groupBox1.Controls.Add(this.versionCheck);
            this.groupBox1.Location = new System.Drawing.Point(12, 83);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(359, 125);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "进程监控";
            // 
            // aisCheck
            // 
            this.aisCheck.AutoSize = true;
            this.aisCheck.Location = new System.Drawing.Point(12, 71);
            this.aisCheck.Name = "aisCheck";
            this.aisCheck.Size = new System.Drawing.Size(87, 17);
            this.aisCheck.TabIndex = 0;
            this.aisCheck.Text = "启用AIS.exe";
            this.aisCheck.UseVisualStyleBackColor = true;
            // 
            // mdsCheck
            // 
            this.mdsCheck.AutoSize = true;
            this.mdsCheck.Location = new System.Drawing.Point(12, 48);
            this.mdsCheck.Name = "mdsCheck";
            this.mdsCheck.Size = new System.Drawing.Size(94, 17);
            this.mdsCheck.TabIndex = 0;
            this.mdsCheck.Text = "启用MDS.exe";
            this.mdsCheck.UseVisualStyleBackColor = true;
            // 
            // versionCheck
            // 
            this.versionCheck.AutoSize = true;
            this.versionCheck.Location = new System.Drawing.Point(12, 25);
            this.versionCheck.Name = "versionCheck";
            this.versionCheck.Size = new System.Drawing.Size(122, 17);
            this.versionCheck.TabIndex = 0;
            this.versionCheck.Text = "启动数据上传 v2.0";
            this.versionCheck.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 433);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(383, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // updateStripStatusLabel
            // 
            this.updateStripStatusLabel.Name = "updateStripStatusLabel";
            this.updateStripStatusLabel.Size = new System.Drawing.Size(124, 17);
            this.updateStripStatusLabel.Text = "更新:0000-00-00 00:00";
            // 
            // WatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 455);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "WatchForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Watch";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WatchForm_FormClosing);
            this.Load += new System.EventHandler(this.WatchForm_Load);
            this.SizeChanged += new System.EventHandler(this.WatchForm_SizeChanged);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonWatch;
        private System.Windows.Forms.Button buttonPath;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.NotifyIcon watchNotifyIcon;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox versionCheck;
        private System.Windows.Forms.CheckBox mdsCheck;
        private System.Windows.Forms.CheckBox aisCheck;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel updateStripStatusLabel;
    }
}


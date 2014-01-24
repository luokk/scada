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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textPath = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonWatch = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(558, 60);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据上传";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonWatch);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.textPath);
            this.groupBox2.Location = new System.Drawing.Point(12, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(558, 103);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "自动更新";
            // 
            // textPath
            // 
            this.textPath.Location = new System.Drawing.Point(7, 20);
            this.textPath.Name = "textPath";
            this.textPath.Size = new System.Drawing.Size(494, 20);
            this.textPath.TabIndex = 0;
            this.textPath.TextChanged += new System.EventHandler(this.textPath_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(508, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(44, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // buttonWatch
            // 
            this.buttonWatch.Location = new System.Drawing.Point(425, 67);
            this.buttonWatch.Name = "buttonWatch";
            this.buttonWatch.Size = new System.Drawing.Size(75, 23);
            this.buttonWatch.TabIndex = 2;
            this.buttonWatch.Text = "监控";
            this.buttonWatch.UseVisualStyleBackColor = true;
            this.buttonWatch.Click += new System.EventHandler(this.buttonWatch_Click);
            // 
            // WatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 329);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "WatchForm";
            this.Text = "Watch";
            this.Load += new System.EventHandler(this.WatchForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonWatch;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textPath;
    }
}


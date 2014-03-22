namespace Scada.About
{
    partial class AboutForm
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
            this.featureTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buildLabel = new System.Windows.Forms.Label();
            this.sureButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // featureTextBox
            // 
            this.featureTextBox.Location = new System.Drawing.Point(25, 82);
            this.featureTextBox.Multiline = true;
            this.featureTextBox.Name = "featureTextBox";
            this.featureTextBox.ReadOnly = true;
            this.featureTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.featureTextBox.Size = new System.Drawing.Size(419, 232);
            this.featureTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "北京中检维康";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "版本: 1.0.0.2";
            // 
            // buildLabel
            // 
            this.buildLabel.AutoSize = true;
            this.buildLabel.Location = new System.Drawing.Point(22, 57);
            this.buildLabel.Name = "buildLabel";
            this.buildLabel.Size = new System.Drawing.Size(77, 12);
            this.buildLabel.TabIndex = 2;
            this.buildLabel.Text = "Build: 0127A";
            // 
            // sureButton
            // 
            this.sureButton.Location = new System.Drawing.Point(368, 330);
            this.sureButton.Name = "sureButton";
            this.sureButton.Size = new System.Drawing.Size(75, 28);
            this.sureButton.TabIndex = 0;
            this.sureButton.Text = "确定";
            this.sureButton.UseVisualStyleBackColor = true;
            this.sureButton.Click += new System.EventHandler(this.sureButton_Click);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 370);
            this.Controls.Add(this.sureButton);
            this.Controls.Add(this.buildLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.featureTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "关于(Scada)辐射环境自动监测站系统管理";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox featureTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label buildLabel;
        private System.Windows.Forms.Button sureButton;
    }
}


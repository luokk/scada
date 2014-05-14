namespace Scada.Device.Siemens
{
    partial class DeviceForm
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
            this.btnConnMDS = new System.Windows.Forms.Button();
            this.btnConnAIS = new System.Windows.Forms.Button();
            this.btnDisconnMDS = new System.Windows.Forms.Button();
            this.btnDisconnAIS = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnMDS
            // 
            this.btnConnMDS.Location = new System.Drawing.Point(12, 12);
            this.btnConnMDS.Name = "btnConnMDS";
            this.btnConnMDS.Size = new System.Drawing.Size(160, 23);
            this.btnConnMDS.TabIndex = 0;
            this.btnConnMDS.Text = "(MDS) Connect To CPU";
            this.btnConnMDS.UseVisualStyleBackColor = true;
            this.btnConnMDS.Click += new System.EventHandler(this.btnConnMDS_Click);
            // 
            // btnConnAIS
            // 
            this.btnConnAIS.Location = new System.Drawing.Point(12, 41);
            this.btnConnAIS.Name = "btnConnAIS";
            this.btnConnAIS.Size = new System.Drawing.Size(160, 23);
            this.btnConnAIS.TabIndex = 0;
            this.btnConnAIS.Text = "(AIS) Connect To CPU";
            this.btnConnAIS.UseVisualStyleBackColor = true;
            this.btnConnAIS.Click += new System.EventHandler(this.btnConnAIS_Click);
            // 
            // btnDisconnMDS
            // 
            this.btnDisconnMDS.Location = new System.Drawing.Point(178, 12);
            this.btnDisconnMDS.Name = "btnDisconnMDS";
            this.btnDisconnMDS.Size = new System.Drawing.Size(160, 23);
            this.btnDisconnMDS.TabIndex = 0;
            this.btnDisconnMDS.Text = "(MDS) Disconnect";
            this.btnDisconnMDS.UseVisualStyleBackColor = true;
            this.btnDisconnMDS.Click += new System.EventHandler(this.btnDisconnMDS_Click);
            // 
            // btnDisconnAIS
            // 
            this.btnDisconnAIS.Location = new System.Drawing.Point(178, 41);
            this.btnDisconnAIS.Name = "btnDisconnAIS";
            this.btnDisconnAIS.Size = new System.Drawing.Size(160, 23);
            this.btnDisconnAIS.TabIndex = 0;
            this.btnDisconnAIS.Text = "(AIS) Disconnect";
            this.btnDisconnAIS.UseVisualStyleBackColor = true;
            this.btnDisconnAIS.Click += new System.EventHandler(this.btnDisconnAIS_Click);
            // 
            // DeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 217);
            this.Controls.Add(this.btnDisconnAIS);
            this.Controls.Add(this.btnConnAIS);
            this.Controls.Add(this.btnDisconnMDS);
            this.Controls.Add(this.btnConnMDS);
            this.Name = "DeviceForm";
            this.Text = "SIEMENS  S7200";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnConnMDS;
        private System.Windows.Forms.Button btnConnAIS;
        private System.Windows.Forms.Button btnDisconnMDS;
        private System.Windows.Forms.Button btnDisconnAIS;
    }
}


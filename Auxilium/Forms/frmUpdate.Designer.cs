namespace Auxilium.Forms
{
    partial class frmUpdate
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
            this.pictureInfo = new System.Windows.Forms.PictureBox();
            this.progressUpdateBar = new System.Windows.Forms.ProgressBar();
            this.labelInfo = new SmoothLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureInfo.Location = new System.Drawing.Point(12, 12);
            this.pictureInfo.Name = "pictureInfo";
            this.pictureInfo.Size = new System.Drawing.Size(32, 32);
            this.pictureInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureInfo.TabIndex = 1;
            this.pictureInfo.TabStop = false;
            // 
            // progressUpdateBar
            // 
            this.progressUpdateBar.Location = new System.Drawing.Point(12, 50);
            this.progressUpdateBar.Name = "progressUpdateBar";
            this.progressUpdateBar.Size = new System.Drawing.Size(271, 16);
            this.progressUpdateBar.TabIndex = 3;
            // 
            // smoothLabel1
            // 
            this.labelInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(200)))));
            this.labelInfo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(160)))));
            this.labelInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(120)))));
            this.labelInfo.Location = new System.Drawing.Point(50, 12);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(233, 32);
            this.labelInfo.TabIndex = 2;
            // 
            // frmUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 78);
            this.ControlBox = false;
            this.Controls.Add(this.progressUpdateBar);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.pictureInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Updating!";
            ((System.ComponentModel.ISupportInitialize)(this.pictureInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureInfo;
        private SmoothLabel labelInfo;
        private System.Windows.Forms.ProgressBar progressUpdateBar;
    }
}
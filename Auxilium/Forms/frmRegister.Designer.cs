namespace Auxilium
{
    partial class frmRegister
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
            this.tbUser = new System.Windows.Forms.TextBox();
            this.lbUser = new System.Windows.Forms.Label();
            this.lbPass = new System.Windows.Forms.Label();
            this.tbPass = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbUser
            // 
            this.tbUser.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbUser.Location = new System.Drawing.Point(72, 12);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(151, 21);
            this.tbUser.TabIndex = 0;
            // 
            // lbUser
            // 
            this.lbUser.AutoSize = true;
            this.lbUser.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUser.Location = new System.Drawing.Point(14, 15);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(38, 13);
            this.lbUser.TabIndex = 1;
            this.lbUser.Text = "User:";
            // 
            // lbPass
            // 
            this.lbPass.AutoSize = true;
            this.lbPass.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPass.Location = new System.Drawing.Point(14, 41);
            this.lbPass.Name = "lbPass";
            this.lbPass.Size = new System.Drawing.Size(38, 13);
            this.lbPass.TabIndex = 2;
            this.lbPass.Text = "Pass:";
            // 
            // tbPass
            // 
            this.tbPass.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPass.Location = new System.Drawing.Point(72, 38);
            this.tbPass.Name = "tbPass";
            this.tbPass.PasswordChar = '•';
            this.tbPass.Size = new System.Drawing.Size(151, 21);
            this.tbPass.TabIndex = 4;
            // 
            // btnRegister
            // 
            this.btnRegister.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRegister.Location = new System.Drawing.Point(126, 64);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(98, 23);
            this.btnRegister.TabIndex = 6;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // frmRegister
            // 
            this.AcceptButton = this.btnRegister;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 99);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.tbPass);
            this.Controls.Add(this.lbPass);
            this.Controls.Add(this.lbUser);
            this.Controls.Add(this.tbUser);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmRegister";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Register";
            this.Load += new System.EventHandler(this.frmRegister_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.Label lbPass;
        private System.Windows.Forms.TextBox tbPass;
        private System.Windows.Forms.Button btnRegister;
    }
}
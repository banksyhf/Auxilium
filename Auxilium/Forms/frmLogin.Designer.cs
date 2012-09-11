
namespace Auxilium
{
    partial class frmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            this.btnLogin = new System.Windows.Forms.Button();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.lbUser = new System.Windows.Forms.Label();
            this.lbPass = new System.Windows.Forms.Label();
            this.tbPass = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.cbRemember = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Image = ((System.Drawing.Image)(resources.GetObject("btnLogin.Image")));
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogin.Location = new System.Drawing.Point(188, 88);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(93, 23);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tbUser
            // 
            this.tbUser.Location = new System.Drawing.Point(59, 12);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(222, 21);
            this.tbUser.TabIndex = 1;
            // 
            // lbUser
            // 
            this.lbUser.AutoSize = true;
            this.lbUser.Location = new System.Drawing.Point(9, 15);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(38, 13);
            this.lbUser.TabIndex = 2;
            this.lbUser.Text = "User:";
            // 
            // lbPass
            // 
            this.lbPass.AutoSize = true;
            this.lbPass.Location = new System.Drawing.Point(9, 41);
            this.lbPass.Name = "lbPass";
            this.lbPass.Size = new System.Drawing.Size(38, 13);
            this.lbPass.TabIndex = 3;
            this.lbPass.Text = "Pass:";
            // 
            // tbPass
            // 
            this.tbPass.Location = new System.Drawing.Point(59, 38);
            this.tbPass.Name = "tbPass";
            this.tbPass.PasswordChar = '•';
            this.tbPass.Size = new System.Drawing.Size(222, 21);
            this.tbPass.TabIndex = 4;
            // 
            // btnRegister
            // 
            this.btnRegister.Image = ((System.Drawing.Image)(resources.GetObject("btnRegister.Image")));
            this.btnRegister.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRegister.Location = new System.Drawing.Point(12, 88);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(93, 23);
            this.btnRegister.TabIndex = 5;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // cbRemember
            // 
            this.cbRemember.AutoSize = true;
            this.cbRemember.Location = new System.Drawing.Point(173, 65);
            this.cbRemember.Name = "cbRemember";
            this.cbRemember.Size = new System.Drawing.Size(108, 17);
            this.cbRemember.TabIndex = 6;
            this.cbRemember.Text = "Remember Me";
            this.cbRemember.UseVisualStyleBackColor = true;
            // 
            // frmLogin
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 123);
            this.Controls.Add(this.cbRemember);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.tbUser);
            this.Controls.Add(this.lbUser);
            this.Controls.Add(this.tbPass);
            this.Controls.Add(this.lbPass);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login to Auxilium";
            this.Load += new System.EventHandler(this.frmLogin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.Label lbPass;
        private System.Windows.Forms.TextBox tbPass;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.CheckBox cbRemember;
    }
}
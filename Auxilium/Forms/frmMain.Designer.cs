namespace Auxilium
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.lbUsers = new System.Windows.Forms.ListBox();
            this.rtbMessage = new System.Windows.Forms.RichTextBox();
            this.lblMessages = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblUsers = new System.Windows.Forms.Label();
            this.niChat = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.tsJoinNew = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDisable = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tslChatting = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbChat
            // 
            this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbChat.BackColor = System.Drawing.Color.White;
            this.rtbChat.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbChat.Location = new System.Drawing.Point(12, 40);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbChat.Size = new System.Drawing.Size(365, 198);
            this.rtbChat.TabIndex = 7;
            this.rtbChat.Text = "";
            this.rtbChat.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbChat_LinkClicked);
            this.rtbChat.TextChanged += new System.EventHandler(this.rtbChat_TextChanged);
            // 
            // lbUsers
            // 
            this.lbUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbUsers.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUsers.FormattingEnabled = true;
            this.lbUsers.Location = new System.Drawing.Point(383, 40);
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size(147, 225);
            this.lbUsers.TabIndex = 8;
            // 
            // rtbMessage
            // 
            this.rtbMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbMessage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMessage.Location = new System.Drawing.Point(12, 244);
            this.rtbMessage.MaxLength = 255;
            this.rtbMessage.Name = "rtbMessage";
            this.rtbMessage.Size = new System.Drawing.Size(253, 21);
            this.rtbMessage.TabIndex = 12;
            this.rtbMessage.Text = "";
            // 
            // lblMessages
            // 
            this.lblMessages.AutoSize = true;
            this.lblMessages.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessages.Location = new System.Drawing.Point(12, 24);
            this.lblMessages.Name = "lblMessages";
            this.lblMessages.Size = new System.Drawing.Size(67, 13);
            this.lblMessages.TabIndex = 9;
            this.lblMessages.Text = "Messages:";
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSend.Location = new System.Drawing.Point(271, 244);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(106, 21);
            this.btnSend.TabIndex = 11;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblUsers
            // 
            this.lblUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUsers.AutoSize = true;
            this.lblUsers.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsers.Location = new System.Drawing.Point(380, 24);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Size = new System.Drawing.Size(44, 13);
            this.lblUsers.TabIndex = 10;
            this.lblUsers.Text = "Users:";
            // 
            // niChat
            // 
            this.niChat.Icon = ((System.Drawing.Icon)(resources.GetObject("niChat.Icon")));
            this.niChat.Text = "Auxilium";
            this.niChat.Visible = true;
            this.niChat.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niChat_MouseDoubleClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(542, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsMenu
            // 
            this.tsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsJoinNew,
            this.tsDisable});
            this.tsMenu.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsMenu.Name = "tsMenu";
            this.tsMenu.Size = new System.Drawing.Size(49, 20);
            this.tsMenu.Text = "Menu";
            // 
            // tsJoinNew
            // 
            this.tsJoinNew.Name = "tsJoinNew";
            this.tsJoinNew.Size = new System.Drawing.Size(220, 22);
            this.tsJoinNew.Text = "Join Different Channel";
            this.tsJoinNew.Click += new System.EventHandler(this.tsJoinNew_Click);
            // 
            // tsDisable
            // 
            this.tsDisable.CheckOnClick = true;
            this.tsDisable.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsDisable.Name = "tsDisable";
            this.tsDisable.Size = new System.Drawing.Size(220, 22);
            this.tsDisable.Text = "Disable Chat Notifications";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslChatting});
            this.statusStrip1.Location = new System.Drawing.Point(0, 282);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(542, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tslChatting
            // 
            this.tslChatting.Name = "tslChatting";
            this.tslChatting.Size = new System.Drawing.Size(112, 17);
            this.tslChatting.Text = "Chatting in: Lounge";
            // 
            // frmMain
            // 
            this.AcceptButton = this.btnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 304);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.lbUsers);
            this.Controls.Add(this.rtbMessage);
            this.Controls.Add(this.lblMessages);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.lblUsers);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Auxilium";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.ListBox lbUsers;
        private System.Windows.Forms.RichTextBox rtbMessage;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblUsers;
        private System.Windows.Forms.NotifyIcon niChat;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsMenu;
        private System.Windows.Forms.ToolStripMenuItem tsDisable;
        private System.Windows.Forms.ToolStripMenuItem tsJoinNew;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tslChatting;
    }
}


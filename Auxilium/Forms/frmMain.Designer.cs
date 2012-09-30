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
            this.niAux = new System.Windows.Forms.NotifyIcon(this.components);
            this.msMenu = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmSignOut = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTimestampsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showChatNotificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spaceOutMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeMessagesToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.changeFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pMsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sStatus = new System.Windows.Forms.StatusStrip();
            this.tslChatting = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslUsersOnline = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmsUsers = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sendPMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ilUsers = new System.Windows.Forms.ImageList(this.components);
            this.cmsClipboard = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hiddenTab1 = new HiddenTab();
            this.tpLogin = new System.Windows.Forms.TabPage();
            this.cbRemember = new System.Windows.Forms.CheckBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.lbUser = new System.Windows.Forms.Label();
            this.tbPass = new System.Windows.Forms.TextBox();
            this.lbPass = new System.Windows.Forms.Label();
            this.tpRegister = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tpChat = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.rtbMessage = new System.Windows.Forms.RichTextBox();
            this.lvUsers = new ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.tpReconnect = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.msMenu.SuspendLayout();
            this.sStatus.SuspendLayout();
            this.cmsUsers.SuspendLayout();
            this.cmsClipboard.SuspendLayout();
            this.hiddenTab1.SuspendLayout();
            this.tpLogin.SuspendLayout();
            this.tpRegister.SuspendLayout();
            this.tpChat.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tpReconnect.SuspendLayout();
            this.SuspendLayout();
            // 
            // niAux
            // 
            this.niAux.Icon = ((System.Drawing.Icon)(resources.GetObject("niAux.Icon")));
            this.niAux.Text = "Auxilium";
            this.niAux.Visible = true;
            this.niAux.BalloonTipClicked += new System.EventHandler(this.niChat_BalloonTipClicked);
            // 
            // msMenu
            // 
            this.msMenu.BackColor = System.Drawing.Color.Transparent;
            this.msMenu.Enabled = false;
            this.msMenu.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.msMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.pMsToolStripMenuItem});
            this.msMenu.Location = new System.Drawing.Point(0, 0);
            this.msMenu.Name = "msMenu";
            this.msMenu.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.msMenu.Size = new System.Drawing.Size(608, 24);
            this.msMenu.TabIndex = 13;
            this.msMenu.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmSignOut});
            this.menuToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("menuToolStripMenuItem.Image")));
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(65, 22);
            this.menuToolStripMenuItem.Text = "Menu";
            // 
            // tsmSignOut
            // 
            this.tsmSignOut.Enabled = false;
            this.tsmSignOut.Name = "tsmSignOut";
            this.tsmSignOut.Size = new System.Drawing.Size(123, 22);
            this.tsmSignOut.Text = "Sign Out";
            this.tsmSignOut.Click += new System.EventHandler(this.tsmSignOut_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.showChatNotificationsToolStripMenuItem,
            this.spaceOutMessagesToolStripMenuItem,
            this.showTimestampsToolStripMenuItem,
            this.writeMessagesToFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.changeFontToolStripMenuItem});
            this.optionsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("optionsToolStripMenuItem.Image")));
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(78, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // showTimestampsToolStripMenuItem
            // 
            this.showTimestampsToolStripMenuItem.CheckOnClick = true;
            this.showTimestampsToolStripMenuItem.Name = "showTimestampsToolStripMenuItem";
            this.showTimestampsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.showTimestampsToolStripMenuItem.Text = "Show User/Join Events";
            this.showTimestampsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showTimestampsToolStripMenuItem_CheckedChanged);
            this.showTimestampsToolStripMenuItem.Click += new System.EventHandler(this.showTimestampsToolStripMenuItem_Click);
            // 
            // showChatNotificationsToolStripMenuItem
            // 
            this.showChatNotificationsToolStripMenuItem.Checked = true;
            this.showChatNotificationsToolStripMenuItem.CheckOnClick = true;
            this.showChatNotificationsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showChatNotificationsToolStripMenuItem.Name = "showChatNotificationsToolStripMenuItem";
            this.showChatNotificationsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.showChatNotificationsToolStripMenuItem.Text = "Show Chat Notifications";
            this.showChatNotificationsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showChatNotificationsToolStripMenuItem_CheckedChanged);
            // 
            // spaceOutMessagesToolStripMenuItem
            // 
            this.spaceOutMessagesToolStripMenuItem.Checked = true;
            this.spaceOutMessagesToolStripMenuItem.CheckOnClick = true;
            this.spaceOutMessagesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.spaceOutMessagesToolStripMenuItem.Name = "spaceOutMessagesToolStripMenuItem";
            this.spaceOutMessagesToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.spaceOutMessagesToolStripMenuItem.Text = "Space Out Messages";
            this.spaceOutMessagesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.spaceOutMessagesToolStripMenuItem_CheckedChanged);
            // 
            // writeMessagesToFileToolStripMenuItem
            // 
            this.writeMessagesToFileToolStripMenuItem.CheckOnClick = true;
            this.writeMessagesToFileToolStripMenuItem.Enabled = false;
            this.writeMessagesToFileToolStripMenuItem.Name = "writeMessagesToFileToolStripMenuItem";
            this.writeMessagesToFileToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.writeMessagesToFileToolStripMenuItem.Text = "Write Messages To File";
            this.writeMessagesToFileToolStripMenuItem.CheckedChanged += new System.EventHandler(this.writeMessagesToFileToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(206, 6);
            // 
            // changeFontToolStripMenuItem
            // 
            this.changeFontToolStripMenuItem.Name = "changeFontToolStripMenuItem";
            this.changeFontToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.changeFontToolStripMenuItem.Text = "Change Font..";
            this.changeFontToolStripMenuItem.Click += new System.EventHandler(this.changeFontToolStripMenuItem_Click);
            // 
            // pMsToolStripMenuItem
            // 
            this.pMsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pMsToolStripMenuItem.Image")));
            this.pMsToolStripMenuItem.Name = "pMsToolStripMenuItem";
            this.pMsToolStripMenuItem.Size = new System.Drawing.Size(57, 22);
            this.pMsToolStripMenuItem.Text = "PMs";
            this.pMsToolStripMenuItem.Click += new System.EventHandler(this.pMsToolStripMenuItem_Click);
            // 
            // sStatus
            // 
            this.sStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslChatting,
            this.toolStripStatusLabel2,
            this.tslUsersOnline});
            this.sStatus.Location = new System.Drawing.Point(0, 330);
            this.sStatus.Name = "sStatus";
            this.sStatus.Size = new System.Drawing.Size(608, 22);
            this.sStatus.SizingGrip = false;
            this.sStatus.TabIndex = 14;
            this.sStatus.Text = "statusStrip1";
            // 
            // tslChatting
            // 
            this.tslChatting.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tslChatting.Margin = new System.Windows.Forms.Padding(6, 3, 0, 2);
            this.tslChatting.Name = "tslChatting";
            this.tslChatting.Size = new System.Drawing.Size(157, 17);
            this.tslChatting.Text = "Status: Checking for updates..";
            this.tslChatting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(351, 17);
            this.toolStripStatusLabel2.Spring = true;
            this.toolStripStatusLabel2.Text = "      ";
            // 
            // tslUsersOnline
            // 
            this.tslUsersOnline.Margin = new System.Windows.Forms.Padding(0, 3, -6, 2);
            this.tslUsersOnline.Name = "tslUsersOnline";
            this.tslUsersOnline.Size = new System.Drawing.Size(85, 17);
            this.tslUsersOnline.Text = "Users Online: 0";
            // 
            // cmsUsers
            // 
            this.cmsUsers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendPMToolStripMenuItem});
            this.cmsUsers.Name = "contextMenuStrip1";
            this.cmsUsers.Size = new System.Drawing.Size(120, 26);
            this.cmsUsers.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // sendPMToolStripMenuItem
            // 
            this.sendPMToolStripMenuItem.Name = "sendPMToolStripMenuItem";
            this.sendPMToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.sendPMToolStripMenuItem.Text = "Send PM";
            this.sendPMToolStripMenuItem.Click += new System.EventHandler(this.sendPMToolStripMenuItem_Click);
            // 
            // ilUsers
            // 
            this.ilUsers.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilUsers.ImageStream")));
            this.ilUsers.TransparentColor = System.Drawing.Color.Transparent;
            this.ilUsers.Images.SetKeyName(0, "user.ico");
            this.ilUsers.Images.SetKeyName(1, "user_red.ico");
            // 
            // cmsClipboard
            // 
            this.cmsClipboard.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmsClipboard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteToolStripMenuItem});
            this.cmsClipboard.Name = "cmsClipboard";
            this.cmsClipboard.Size = new System.Drawing.Size(100, 26);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // hiddenTab1
            // 
            this.hiddenTab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hiddenTab1.Controls.Add(this.tpLogin);
            this.hiddenTab1.Controls.Add(this.tpRegister);
            this.hiddenTab1.Controls.Add(this.tpChat);
            this.hiddenTab1.Controls.Add(this.tpReconnect);
            this.hiddenTab1.DesignerIndex = 2;
            this.hiddenTab1.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiddenTab1.Location = new System.Drawing.Point(9, 32);
            this.hiddenTab1.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.hiddenTab1.Name = "hiddenTab1";
            this.hiddenTab1.SelectedIndex = 0;
            this.hiddenTab1.Size = new System.Drawing.Size(590, 290);
            this.hiddenTab1.TabIndex = 22;
            // 
            // tpLogin
            // 
            this.tpLogin.BackColor = System.Drawing.SystemColors.Control;
            this.tpLogin.Controls.Add(this.cbRemember);
            this.tpLogin.Controls.Add(this.btnRegister);
            this.tpLogin.Controls.Add(this.btnLogin);
            this.tpLogin.Controls.Add(this.tbUser);
            this.tpLogin.Controls.Add(this.lbUser);
            this.tpLogin.Controls.Add(this.tbPass);
            this.tpLogin.Controls.Add(this.lbPass);
            this.tpLogin.Location = new System.Drawing.Point(0, 0);
            this.tpLogin.Name = "tpLogin";
            this.tpLogin.Padding = new System.Windows.Forms.Padding(3);
            this.tpLogin.Size = new System.Drawing.Size(590, 290);
            this.tpLogin.TabIndex = 0;
            this.tpLogin.Text = "tabPage1";
            // 
            // cbRemember
            // 
            this.cbRemember.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbRemember.AutoSize = true;
            this.cbRemember.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbRemember.Location = new System.Drawing.Point(247, 142);
            this.cbRemember.Name = "cbRemember";
            this.cbRemember.Size = new System.Drawing.Size(94, 16);
            this.cbRemember.TabIndex = 12;
            this.cbRemember.Text = "Remember me";
            this.cbRemember.UseVisualStyleBackColor = true;
            // 
            // btnRegister
            // 
            this.btnRegister.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRegister.Enabled = false;
            this.btnRegister.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRegister.Image = ((System.Drawing.Image)(resources.GetObject("btnRegister.Image")));
            this.btnRegister.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRegister.Location = new System.Drawing.Point(247, 197);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(159, 27);
            this.btnRegister.TabIndex = 11;
            this.btnRegister.Text = "Create new account";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLogin.Enabled = false;
            this.btnLogin.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.Image = ((System.Drawing.Image)(resources.GetObject("btnLogin.Image")));
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogin.Location = new System.Drawing.Point(247, 164);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(159, 27);
            this.btnLogin.TabIndex = 10;
            this.btnLogin.Text = "Sign into account";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tbUser
            // 
            this.tbUser.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbUser.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbUser.Location = new System.Drawing.Point(247, 90);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(159, 21);
            this.tbUser.TabIndex = 8;
            // 
            // lbUser
            // 
            this.lbUser.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbUser.AutoSize = true;
            this.lbUser.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUser.Location = new System.Drawing.Point(185, 94);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(56, 12);
            this.lbUser.TabIndex = 9;
            this.lbUser.Text = "Username";
            // 
            // tbPass
            // 
            this.tbPass.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbPass.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPass.Location = new System.Drawing.Point(247, 116);
            this.tbPass.Name = "tbPass";
            this.tbPass.PasswordChar = '•';
            this.tbPass.Size = new System.Drawing.Size(159, 21);
            this.tbPass.TabIndex = 9;
            this.tbPass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPass_KeyDown);
            // 
            // lbPass
            // 
            this.lbPass.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbPass.AutoSize = true;
            this.lbPass.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPass.Location = new System.Drawing.Point(185, 120);
            this.lbPass.Name = "lbPass";
            this.lbPass.Size = new System.Drawing.Size(53, 12);
            this.lbPass.TabIndex = 10;
            this.lbPass.Text = "Password";
            // 
            // tpRegister
            // 
            this.tpRegister.BackColor = System.Drawing.SystemColors.Control;
            this.tpRegister.Controls.Add(this.button3);
            this.tpRegister.Controls.Add(this.button1);
            this.tpRegister.Controls.Add(this.textBox1);
            this.tpRegister.Controls.Add(this.label1);
            this.tpRegister.Controls.Add(this.label2);
            this.tpRegister.Controls.Add(this.textBox2);
            this.tpRegister.Location = new System.Drawing.Point(0, 0);
            this.tpRegister.Name = "tpRegister";
            this.tpRegister.Padding = new System.Windows.Forms.Padding(3);
            this.tpRegister.Size = new System.Drawing.Size(590, 290);
            this.tpRegister.TabIndex = 1;
            this.tpRegister.Text = "tabPage2";
            // 
            // button3
            // 
            this.button3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button3.Font = new System.Drawing.Font("Verdana", 6.75F);
            this.button3.Location = new System.Drawing.Point(248, 173);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(77, 27);
            this.button3.TabIndex = 12;
            this.button3.Text = "Return";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button1.Font = new System.Drawing.Font("Verdana", 6.75F);
            this.button1.Location = new System.Drawing.Point(330, 173);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 27);
            this.button1.TabIndex = 11;
            this.button1.Text = "Register";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(248, 146);
            this.textBox1.Name = "textBox1";
            this.textBox1.PasswordChar = '•';
            this.textBox1.Size = new System.Drawing.Size(159, 21);
            this.textBox1.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 6.75F);
            this.label1.Location = new System.Drawing.Point(186, 149);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "Password";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 6.75F);
            this.label2.Location = new System.Drawing.Point(186, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "Username";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(248, 119);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(159, 21);
            this.textBox2.TabIndex = 7;
            // 
            // tpChat
            // 
            this.tpChat.BackColor = System.Drawing.SystemColors.Control;
            this.tpChat.Controls.Add(this.splitContainer2);
            this.tpChat.Location = new System.Drawing.Point(0, 0);
            this.tpChat.Name = "tpChat";
            this.tpChat.Size = new System.Drawing.Size(590, 290);
            this.tpChat.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lvUsers);
            this.splitContainer2.Panel2.Controls.Add(this.comboBox1);
            this.splitContainer2.Size = new System.Drawing.Size(590, 290);
            this.splitContainer2.SplitterDistance = 433;
            this.splitContainer2.TabIndex = 21;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.rtbChat);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbMessage);
            this.splitContainer1.Panel2MinSize = 20;
            this.splitContainer1.Size = new System.Drawing.Size(433, 290);
            this.splitContainer1.SplitterDistance = 265;
            this.splitContainer1.TabIndex = 20;
            // 
            // rtbChat
            // 
            this.rtbChat.BackColor = System.Drawing.Color.White;
            this.rtbChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbChat.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbChat.Location = new System.Drawing.Point(0, 0);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbChat.Size = new System.Drawing.Size(433, 265);
            this.rtbChat.TabIndex = 7;
            this.rtbChat.Text = "";
            this.rtbChat.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbChat_LinkClicked);
            this.rtbChat.VScroll += new System.EventHandler(this.rtbChat_VScroll);
            // 
            // rtbMessage
            // 
            this.rtbMessage.ContextMenuStrip = this.cmsClipboard;
            this.rtbMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbMessage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMessage.Location = new System.Drawing.Point(0, 0);
            this.rtbMessage.MaxLength = 1024;
            this.rtbMessage.Name = "rtbMessage";
            this.rtbMessage.Size = new System.Drawing.Size(433, 21);
            this.rtbMessage.TabIndex = 12;
            this.rtbMessage.Text = "";
            this.rtbMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtbMessage_KeyDown);
            // 
            // lvUsers
            // 
            this.lvUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvUsers.ContextMenuStrip = this.cmsUsers;
            this.lvUsers.FullRowSelect = true;
            this.lvUsers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvUsers.Location = new System.Drawing.Point(0, 0);
            this.lvUsers.Name = "lvUsers";
            this.lvUsers.Size = new System.Drawing.Size(153, 264);
            this.lvUsers.SmallImageList = this.ilUsers;
            this.lvUsers.TabIndex = 20;
            this.lvUsers.UseCompatibleStateImageBehavior = false;
            this.lvUsers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 147;
            // 
            // comboBox1
            // 
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBox1.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(0, 270);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(153, 20);
            this.comboBox1.TabIndex = 19;
            this.comboBox1.Text = "Channels";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // tpReconnect
            // 
            this.tpReconnect.BackColor = System.Drawing.SystemColors.Control;
            this.tpReconnect.Controls.Add(this.button2);
            this.tpReconnect.Location = new System.Drawing.Point(0, 0);
            this.tpReconnect.Name = "tpReconnect";
            this.tpReconnect.Size = new System.Drawing.Size(590, 290);
            this.tpReconnect.TabIndex = 3;
            this.tpReconnect.Text = "tabPage4";
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button2.Font = new System.Drawing.Font("Verdana", 6.75F);
            this.button2.Location = new System.Drawing.Point(216, 144);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(159, 27);
            this.button2.TabIndex = 12;
            this.button2.Text = "Reconnect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Checked = true;
            this.toolStripMenuItem1.CheckOnClick = true;
            this.toolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(209, 22);
            this.toolStripMenuItem1.Text = "Show Timestamps";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 352);
            this.Controls.Add(this.hiddenTab1);
            this.Controls.Add(this.sStatus);
            this.Controls.Add(this.msMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.msMenu;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Auxilium";
            this.Activated += new System.EventHandler(this.frmMain_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.msMenu.ResumeLayout(false);
            this.msMenu.PerformLayout();
            this.sStatus.ResumeLayout(false);
            this.sStatus.PerformLayout();
            this.cmsUsers.ResumeLayout(false);
            this.cmsClipboard.ResumeLayout(false);
            this.hiddenTab1.ResumeLayout(false);
            this.tpLogin.ResumeLayout(false);
            this.tpLogin.PerformLayout();
            this.tpRegister.ResumeLayout(false);
            this.tpRegister.PerformLayout();
            this.tpChat.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tpReconnect.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.RichTextBox rtbMessage;
        private System.Windows.Forms.MenuStrip msMenu;
        private System.Windows.Forms.StatusStrip sStatus;
        private System.Windows.Forms.ToolStripStatusLabel tslChatting;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private HiddenTab hiddenTab1;
        private System.Windows.Forms.TabPage tpLogin;
        private System.Windows.Forms.TabPage tpRegister;
        private System.Windows.Forms.TabPage tpChat;
        private System.Windows.Forms.CheckBox cbRemember;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.TextBox tbPass;
        private System.Windows.Forms.Label lbPass;
        private System.Windows.Forms.ToolStripMenuItem showTimestampsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spaceOutMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem changeFontToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel tslUsersOnline;
        private System.Windows.Forms.TabPage tpReconnect;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmSignOut;
        private System.Windows.Forms.ToolStripMenuItem showChatNotificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeMessagesToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pMsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsUsers;
        private System.Windows.Forms.ToolStripMenuItem sendPMToolStripMenuItem;
        public System.Windows.Forms.NotifyIcon niAux;
        private System.Windows.Forms.ImageList ilUsers;
        private ListView lvUsers;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip cmsClipboard;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}


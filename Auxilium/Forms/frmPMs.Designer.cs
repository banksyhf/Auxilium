namespace Auxilium.Forms
{
    partial class frmPMs
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
            this.cmsPM = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.readMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hiddenTab1 = new HiddenTab();
            this.tpPMs = new System.Windows.Forms.TabPage();
            this.lvPMs = new ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tpReply = new System.Windows.Forms.TabPage();
            this.rtbReply = new System.Windows.Forms.RichTextBox();
            this.btnReplyBack = new System.Windows.Forms.Button();
            this.btnReplySend = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbReplyFrom = new System.Windows.Forms.TextBox();
            this.rtbReplyMessage = new System.Windows.Forms.RichTextBox();
            this.tpRead = new System.Windows.Forms.TabPage();
            this.btnReadBack = new System.Windows.Forms.Button();
            this.btnReply = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbReadFrom = new System.Windows.Forms.TextBox();
            this.rtbReadMessage = new System.Windows.Forms.RichTextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSendBack = new System.Windows.Forms.Button();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.rtbSendMessage = new System.Windows.Forms.RichTextBox();
            this.cmsPM.SuspendLayout();
            this.hiddenTab1.SuspendLayout();
            this.tpPMs.SuspendLayout();
            this.tpReply.SuspendLayout();
            this.tpRead.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsPM
            // 
            this.cmsPM.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readMessageToolStripMenuItem,
            this.replyToolStripMenuItem});
            this.cmsPM.Name = "cmsPM";
            this.cmsPM.Size = new System.Drawing.Size(150, 48);
            this.cmsPM.Opening += new System.ComponentModel.CancelEventHandler(this.cmsPM_Opening);
            // 
            // readMessageToolStripMenuItem
            // 
            this.readMessageToolStripMenuItem.Name = "readMessageToolStripMenuItem";
            this.readMessageToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.readMessageToolStripMenuItem.Text = "Read Message";
            this.readMessageToolStripMenuItem.Click += new System.EventHandler(this.readMessageToolStripMenuItem_Click);
            // 
            // replyToolStripMenuItem
            // 
            this.replyToolStripMenuItem.Name = "replyToolStripMenuItem";
            this.replyToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.replyToolStripMenuItem.Text = "Reply";
            this.replyToolStripMenuItem.Click += new System.EventHandler(this.replyToolStripMenuItem_Click);
            // 
            // hiddenTab1
            // 
            this.hiddenTab1.Controls.Add(this.tpPMs);
            this.hiddenTab1.Controls.Add(this.tpReply);
            this.hiddenTab1.Controls.Add(this.tpRead);
            this.hiddenTab1.Controls.Add(this.tabPage1);
            this.hiddenTab1.DesignerIndex = 0;
            this.hiddenTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hiddenTab1.Location = new System.Drawing.Point(0, 0);
            this.hiddenTab1.Name = "hiddenTab1";
            this.hiddenTab1.SelectedIndex = 0;
            this.hiddenTab1.Size = new System.Drawing.Size(468, 270);
            this.hiddenTab1.TabIndex = 3;
            this.hiddenTab1.SelectedIndexChanged += new System.EventHandler(this.hiddenTab1_SelectedIndexChanged);
            // 
            // tpPMs
            // 
            this.tpPMs.Controls.Add(this.lvPMs);
            this.tpPMs.Location = new System.Drawing.Point(0, 0);
            this.tpPMs.Name = "tpPMs";
            this.tpPMs.Padding = new System.Windows.Forms.Padding(3);
            this.tpPMs.Size = new System.Drawing.Size(468, 270);
            this.tpPMs.TabIndex = 0;
            this.tpPMs.UseVisualStyleBackColor = true;
            // 
            // lvPMs
            // 
            this.lvPMs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvPMs.ContextMenuStrip = this.cmsPM;
            this.lvPMs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPMs.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvPMs.FullRowSelect = true;
            this.lvPMs.GridLines = true;
            this.lvPMs.Location = new System.Drawing.Point(3, 3);
            this.lvPMs.MultiSelect = false;
            this.lvPMs.Name = "lvPMs";
            this.lvPMs.Size = new System.Drawing.Size(462, 264);
            this.lvPMs.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.lvPMs.TabIndex = 1;
            this.lvPMs.UseCompatibleStateImageBehavior = false;
            this.lvPMs.View = System.Windows.Forms.View.Details;
            this.lvPMs.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvPMs_MouseDoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Time";
            this.columnHeader4.Width = 85;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Sent/Received";
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader1.Width = 90;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Subject";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 164;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "User";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 95;
            // 
            // tpReply
            // 
            this.tpReply.Controls.Add(this.rtbReply);
            this.tpReply.Controls.Add(this.btnReplyBack);
            this.tpReply.Controls.Add(this.btnReplySend);
            this.tpReply.Controls.Add(this.label2);
            this.tpReply.Controls.Add(this.tbReplyFrom);
            this.tpReply.Controls.Add(this.rtbReplyMessage);
            this.tpReply.Location = new System.Drawing.Point(0, 0);
            this.tpReply.Name = "tpReply";
            this.tpReply.Padding = new System.Windows.Forms.Padding(3);
            this.tpReply.Size = new System.Drawing.Size(468, 270);
            this.tpReply.TabIndex = 1;
            this.tpReply.Text = "tabPage2";
            this.tpReply.UseVisualStyleBackColor = true;
            // 
            // rtbReply
            // 
            this.rtbReply.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbReply.BackColor = System.Drawing.Color.White;
            this.rtbReply.Font = new System.Drawing.Font("Verdana", 8F);
            this.rtbReply.Location = new System.Drawing.Point(12, 162);
            this.rtbReply.Name = "rtbReply";
            this.rtbReply.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbReply.Size = new System.Drawing.Size(444, 67);
            this.rtbReply.TabIndex = 17;
            this.rtbReply.Text = "";
            this.rtbReply.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.LinkClicked);
            // 
            // btnReplyBack
            // 
            this.btnReplyBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReplyBack.Location = new System.Drawing.Point(12, 235);
            this.btnReplyBack.Name = "btnReplyBack";
            this.btnReplyBack.Size = new System.Drawing.Size(83, 23);
            this.btnReplyBack.TabIndex = 15;
            this.btnReplyBack.Text = "Back";
            this.btnReplyBack.UseVisualStyleBackColor = true;
            this.btnReplyBack.Click += new System.EventHandler(this.btnReplyBack_Click);
            // 
            // btnReplySend
            // 
            this.btnReplySend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplySend.Location = new System.Drawing.Point(373, 235);
            this.btnReplySend.Name = "btnReplySend";
            this.btnReplySend.Size = new System.Drawing.Size(83, 23);
            this.btnReplySend.TabIndex = 14;
            this.btnReplySend.Text = "Send";
            this.btnReplySend.UseVisualStyleBackColor = true;
            this.btnReplySend.Click += new System.EventHandler(this.btnReplySend_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "Message From:";
            // 
            // tbReplyFrom
            // 
            this.tbReplyFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbReplyFrom.BackColor = System.Drawing.Color.White;
            this.tbReplyFrom.Location = new System.Drawing.Point(98, 12);
            this.tbReplyFrom.Name = "tbReplyFrom";
            this.tbReplyFrom.ReadOnly = true;
            this.tbReplyFrom.Size = new System.Drawing.Size(358, 18);
            this.tbReplyFrom.TabIndex = 12;
            this.tbReplyFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rtbReplyMessage
            // 
            this.rtbReplyMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbReplyMessage.BackColor = System.Drawing.Color.White;
            this.rtbReplyMessage.Font = new System.Drawing.Font("Verdana", 8F);
            this.rtbReplyMessage.Location = new System.Drawing.Point(12, 36);
            this.rtbReplyMessage.Name = "rtbReplyMessage";
            this.rtbReplyMessage.ReadOnly = true;
            this.rtbReplyMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbReplyMessage.Size = new System.Drawing.Size(444, 108);
            this.rtbReplyMessage.TabIndex = 11;
            this.rtbReplyMessage.Text = "";
            this.rtbReplyMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.LinkClicked);
            // 
            // tpRead
            // 
            this.tpRead.Controls.Add(this.btnReadBack);
            this.tpRead.Controls.Add(this.btnReply);
            this.tpRead.Controls.Add(this.label1);
            this.tpRead.Controls.Add(this.tbReadFrom);
            this.tpRead.Controls.Add(this.rtbReadMessage);
            this.tpRead.Location = new System.Drawing.Point(0, 0);
            this.tpRead.Name = "tpRead";
            this.tpRead.Size = new System.Drawing.Size(468, 270);
            this.tpRead.TabIndex = 2;
            this.tpRead.Text = "tabPage1";
            this.tpRead.UseVisualStyleBackColor = true;
            // 
            // btnReadBack
            // 
            this.btnReadBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReadBack.Location = new System.Drawing.Point(12, 235);
            this.btnReadBack.Name = "btnReadBack";
            this.btnReadBack.Size = new System.Drawing.Size(83, 23);
            this.btnReadBack.TabIndex = 12;
            this.btnReadBack.Text = "Back";
            this.btnReadBack.UseVisualStyleBackColor = true;
            this.btnReadBack.Click += new System.EventHandler(this.btnReadBack_Click);
            // 
            // btnReply
            // 
            this.btnReply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReply.Location = new System.Drawing.Point(373, 235);
            this.btnReply.Name = "btnReply";
            this.btnReply.Size = new System.Drawing.Size(83, 23);
            this.btnReply.TabIndex = 11;
            this.btnReply.Text = "Reply";
            this.btnReply.UseVisualStyleBackColor = true;
            this.btnReply.Click += new System.EventHandler(this.btnReply_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "Message From:";
            // 
            // tbReadFrom
            // 
            this.tbReadFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbReadFrom.Location = new System.Drawing.Point(98, 12);
            this.tbReadFrom.Name = "tbReadFrom";
            this.tbReadFrom.ReadOnly = true;
            this.tbReadFrom.Size = new System.Drawing.Size(358, 18);
            this.tbReadFrom.TabIndex = 9;
            this.tbReadFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rtbReadMessage
            // 
            this.rtbReadMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbReadMessage.BackColor = System.Drawing.Color.White;
            this.rtbReadMessage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbReadMessage.Location = new System.Drawing.Point(12, 36);
            this.rtbReadMessage.Name = "rtbReadMessage";
            this.rtbReadMessage.ReadOnly = true;
            this.rtbReadMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbReadMessage.Size = new System.Drawing.Size(444, 193);
            this.rtbReadMessage.TabIndex = 8;
            this.rtbReadMessage.Text = "";
            this.rtbReadMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.LinkClicked);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnSendBack);
            this.tabPage1.Controls.Add(this.btnSendMessage);
            this.tabPage1.Controls.Add(this.rtbSendMessage);
            this.tabPage1.Location = new System.Drawing.Point(0, 0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(468, 270);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "tpSend";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnSendBack
            // 
            this.btnSendBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendBack.Location = new System.Drawing.Point(12, 235);
            this.btnSendBack.Name = "btnSendBack";
            this.btnSendBack.Size = new System.Drawing.Size(83, 23);
            this.btnSendBack.TabIndex = 17;
            this.btnSendBack.Text = "Back";
            this.btnSendBack.UseVisualStyleBackColor = true;
            this.btnSendBack.Click += new System.EventHandler(this.btnSendBack_Click);
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendMessage.Location = new System.Drawing.Point(373, 235);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(83, 23);
            this.btnSendMessage.TabIndex = 16;
            this.btnSendMessage.Text = "Send";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // rtbSendMessage
            // 
            this.rtbSendMessage.AcceptsTab = true;
            this.rtbSendMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSendMessage.Location = new System.Drawing.Point(12, 12);
            this.rtbSendMessage.MaxLength = 1024;
            this.rtbSendMessage.Name = "rtbSendMessage";
            this.rtbSendMessage.Size = new System.Drawing.Size(444, 217);
            this.rtbSendMessage.TabIndex = 0;
            this.rtbSendMessage.Text = "";
            this.rtbSendMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.LinkClicked);
            // 
            // frmPMs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 270);
            this.Controls.Add(this.hiddenTab1);
            this.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(476, 238);
            this.Name = "FrmPMs";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Private Messages";
            this.cmsPM.ResumeLayout(false);
            this.hiddenTab1.ResumeLayout(false);
            this.tpPMs.ResumeLayout(false);
            this.tpReply.ResumeLayout(false);
            this.tpReply.PerformLayout();
            this.tpRead.ResumeLayout(false);
            this.tpRead.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ListView lvPMs;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ContextMenuStrip cmsPM;
        private System.Windows.Forms.ToolStripMenuItem readMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replyToolStripMenuItem;
        private HiddenTab hiddenTab1;
        private System.Windows.Forms.TabPage tpPMs;
        private System.Windows.Forms.TabPage tpReply;
        private System.Windows.Forms.TabPage tpRead;
        private System.Windows.Forms.Button btnReply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbReadFrom;
        private System.Windows.Forms.RichTextBox rtbReadMessage;
        private System.Windows.Forms.Button btnReadBack;
        private System.Windows.Forms.Button btnReplyBack;
        private System.Windows.Forms.Button btnReplySend;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbReplyFrom;
        private System.Windows.Forms.RichTextBox rtbReplyMessage;
        private System.Windows.Forms.RichTextBox rtbReply;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnSendBack;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.RichTextBox rtbSendMessage;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
using System;
using Auxilium.Classes;
using System.Windows.Forms;
using System.ComponentModel;

namespace Auxilium.Forms
{
    public partial class frmPMs : Form
    {
        #region Global Variables

        public PrivateMessage[] PMs;

        private string _sender = string.Empty;
        private string _recipient = string.Empty;
        private string _subject = string.Empty;
        private string _message = string.Empty;
        private string _username = string.Empty;
        private ushort _id;

        private Pack Packer = new Pack();
        private Client Connection;

        #endregion

        #region Initialization

        public frmPMs(PrivateMessage[] pms, Client c)
        {
            InitializeComponent();
            PMs = pms;
            Connection = c;
            LoadPMs();
        }

        public frmPMs(PrivateMessage[] pms, ushort index, string recipient, string subject, Client c, string username)
        {
            InitializeComponent();
            PMs = pms;
            hiddenTab1.SelectedIndex = index;
            _recipient = recipient;
            _subject = subject;
            Connection = c;
            _username = username;
            LoadPMs();
        }

        #endregion

        #region UI Events

        private void readMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadVariables(2);
        }

        private void btnReply_Click(object sender, EventArgs e)
        {
            _recipient = tbReadFrom.Text;
            _message = rtbReadMessage.Text;
            hiddenTab1.SelectedIndex = 1;
        }

        private void btnReadBack_Click(object sender, EventArgs e)
        {
            hiddenTab1.SelectedIndex = 0;
        }

        private void btnReplyBack_Click(object sender, EventArgs e)
        {
            hiddenTab1.SelectedIndex = 0;
        }

        private void replyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadVariables(1);
        }

        private void cmsPM_Opening(object sender, CancelEventArgs e)
        {
            if (lvPMs.SelectedItems.Count < 1)
                e.Cancel = true;
        }

        private void btnReplySend_Click(object sender, EventArgs e)
        {
            byte[] data = Packer.Serialize((byte)ClientPacket.PM, _recipient, rtbReply.Text, _subject, _id);

            Connection.Send(data);
            hiddenTab1.SelectedIndex = 0;
            rtbReply.Text = string.Empty;
        }

        private void btnSendBack_Click(object sender, EventArgs e)
        {
            hiddenTab1.SelectedIndex = 0;
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            byte[] data = Packer.Serialize((byte)ClientPacket.PM, _recipient, rtbSendMessage.Text, _subject);
            Connection.Send(data);
            hiddenTab1.SelectedIndex = 0;
        }

        private void hiddenTab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (hiddenTab1.SelectedIndex)
            {
                case 0:
                    Text = "Private Messages";
                    LoadPMs();
                    break;
                case 1:
                    Text = string.Format("Replying to: {0}. Subject: {1}", _sender, _subject);
                    tbReplyFrom.Text = _sender;
                    rtbReplyMessage.Text = _message;
                    break;
                case 2:
                    Text = string.Format("Message from: {0}. Subject: {1}", _sender, _subject);
                    rtbReadMessage.Text = _message;
                    tbReadFrom.Text = _sender;
                    break;
                case 3:
                    Text = string.Format("New Message To: {0}. Subject: {1}", _recipient, _subject);
                    break;
            }
        }

        private void lvPMs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LoadVariables(2);
        }

        private void LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try {
                System.Diagnostics.Process.Start(e.LinkText);
            } catch {
                //Lazy way to catch exceptions in the link.
            }
        }

        #endregion

        #region Helper Methods

        public void LoadPMs()
        {
            lvPMs.BeginUpdate();
            lvPMs.Items.Clear();
            foreach (PrivateMessage pm in PMs)
            {
                ListViewItem lvi = new ListViewItem(pm.Time.ToShortTimeString());
                lvi.SubItems.AddRange(new string[] { pm.Sender.ToLower() == _username.ToLower() ? "Sent" : "Received", pm.Subject, pm.Sender.ToLower() == _username.ToLower() ? pm.Recipient : pm.Sender });
                lvi.Tag = pm;
                lvPMs.Items.Add(lvi);
            }
            lvPMs.EndUpdate();
        }

        private void LoadVariables(int tabIndex)
        {
            _recipient = ((PrivateMessage)lvPMs.SelectedItems[0].Tag).Recipient;
            _sender = ((PrivateMessage)lvPMs.SelectedItems[0].Tag).Sender;
            _subject = lvPMs.SelectedItems[0].SubItems[2].Text;
            _message = ((PrivateMessage)lvPMs.SelectedItems[0].Tag).GetMessages();
            _id = ((PrivateMessage)lvPMs.SelectedItems[0].Tag).Id;

            hiddenTab1.SelectedIndex = tabIndex;
        }

        #endregion
    }
}

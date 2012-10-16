using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using Auxilium.Classes;
using System.Text;
using System.Windows.Forms;

namespace Auxilium.Forms
{
    public partial class PrivateMessages : Form
    {
        #region Global Variables
        public PrivateMessage[] PMs;

        private string UserName = string.Empty;
        private string Subject = string.Empty;
        private string Message = string.Empty;

        private Pack Packer = new Pack();
        private Client Connection;
        #endregion

        #region Initialization
        public PrivateMessages(PrivateMessage[] pms, Client c)
        {
            InitializeComponent();
            PMs = pms;
            Connection = c;
            LoadPMs();
        }

        public PrivateMessages(PrivateMessage[] pms, ushort index, string user, string subject, Client c)
        {
            InitializeComponent();
            PMs = pms;
            hiddenTab1.SelectedIndex = index;
            UserName = user;
            Subject = subject;
            Connection = c;
            LoadPMs();
        }
        #endregion

        #region UI Events

        private void readMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserName = lvPMs.SelectedItems[0].SubItems[1].Text;
            Subject = lvPMs.SelectedItems[0].SubItems[0].Text;
            Message = (string)lvPMs.SelectedItems[0].Tag;
            hiddenTab1.SelectedIndex = 2;
        }

        private void btnReply_Click(object sender, EventArgs e)
        {
            UserName = tbReadFrom.Text;
            Message = rtbReadMessage.Text;
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
            Subject = lvPMs.SelectedItems[0].SubItems[0].Text;
            UserName = lvPMs.SelectedItems[0].SubItems[1].Text;
            hiddenTab1.SelectedIndex = 1;
        }

        private void cmsPM_Opening(object sender, CancelEventArgs e)
        {
            if (lvPMs.SelectedItems.Count < 1)
                e.Cancel = true;
        }

        private void btnReplySend_Click(object sender, EventArgs e)
        {
            byte[] data = Packer.Serialize((byte)ClientPacket.PM, UserName, rtbReply.Text, Subject);
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
            byte[] data = Packer.Serialize((byte)ClientPacket.PM, UserName, rtbSendMessage.Text, Subject);
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
                    Text = string.Format("Replying to: {0}. Subject: {1}", UserName, Subject);
                    tbReplyFrom.Text = UserName;
                    rtbReplyMessage.Text = Message;
                    break;
                case 2:
                    Text = string.Format("Message from: {0}. Subject: {1}", UserName, Subject);
                    rtbReadMessage.Text = Message;
                    tbReadFrom.Text = UserName;
                    break;
                case 3:
                    Text = string.Format("New Message To: {0}. Subject: {1}", UserName, Subject);
                    break;
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
                ListViewItem lvi = new ListViewItem(pm.Subject);
                lvi.SubItems.AddRange(new string[] { pm.Sender, pm.Time.ToShortTimeString() });
                lvi.Tag = pm.Message;
                lvPMs.Items.Add(lvi);
            }
            lvPMs.EndUpdate();
        }

        #endregion

        private void lvPMs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            UserName = lvPMs.SelectedItems[0].SubItems[1].Text;
            Subject = lvPMs.SelectedItems[0].SubItems[0].Text;
            Message = (string)lvPMs.SelectedItems[0].Tag;
            hiddenTab1.SelectedIndex = 2;
        }

        private void LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch
            {
                //Do nothing.
            }
        }
    }
}

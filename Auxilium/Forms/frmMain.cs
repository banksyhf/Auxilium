using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

namespace Auxilium
{
    public partial class frmMain : Form
    {
        private Nexus.Client Client = new Nexus.Client();
        private string Username = string.Empty;
        #region Delegates
        private delegate void UsersDelegate(object users);
        private delegate void AddToChatDelegate(string text);
        #endregion
        public frmMain()
        {
            Client.Incoming += Client_Incoming;
            Client.Connect("10.36.6.105", 3357);
            string[] update = new System.Net.WebClient().DownloadString("http://coleak.com/auxilium/update.txt").Split('~');
            if (update[0] != Application.ProductVersion)
                Auxilium.Update(update[1]);
            frmLogin Login = new frmLogin(Client);
            if (Login.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InitializeComponent();
                Username = Login.User;
            }
            else
                Environment.Exit(0);
        }
        void Client_Incoming(Nexus.Client c, byte[] d)
        {
            object[] data = (object[])Nexus.Deserialize(d);
            switch ((int)data[0])
            {
                case (int)Nexus.Headers.Users:
                    Thread.Sleep(200);
                    Users((string[])data[1]);
                    break;
                case (int)Nexus.Headers.Message:
                    AddToChat(string.Format("{0}: {1}\n", data[1].ToString(), data[2].ToString()));
                    if (!tsDisable.Checked && !Auxilium.ApplicationIsActivated())
                        niChat.ShowBalloonTip(100, "Message Received", String.Format("Message From: {0}\n\"{1}\"", data[1].ToString(), data[2].ToString()), ToolTipIcon.Info);
                    break;
                case (int)Nexus.Headers.MOTD:
                    Thread.Sleep(200);
                    AddToChat(string.Format("MOTD: {0}\n",data[1].ToString()));
                    break;
            }
        }
        private void Users(object users)
        {
            if (InvokeRequired)
            {
                Invoke(new UsersDelegate(Users), users);
                return;
            }
            lbUsers.BeginUpdate();
            lbUsers.Items.Clear();
            lbUsers.Items.AddRange((string[])users);
            lbUsers.EndUpdate();
        }
        private void AddToChat(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new AddToChatDelegate(AddToChat), text);
                return;
            }
            rtbChat.Text += text;
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbMessage.Text))
            {
                rtbChat.Text += "Me: " + rtbMessage.Text + Environment.NewLine;
                Client.Send(Nexus.Serialize(new object[] { 1, rtbMessage.Text }));
                rtbMessage.Text = string.Empty;
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            niChat.Visible = false;
            niChat.Dispose();
        }
        private void tsJoinNew_Click(object sender, EventArgs e)
        {
            frmNewChannel ChangeChannel = new frmNewChannel(Client);
            if (ChangeChannel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tslChatting.Text = string.Format("Chatting in: {0}", ChangeChannel.Channel);
                rtbChat.Clear();
            }
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            rtbMessage.Select();
        }

        private void rtbChat_TextChanged(object sender, EventArgs e)
        {
            rtbChat.SelectionStart = rtbChat.Text.Length;
            rtbChat.ScrollToCaret();
        }

        private void rtbChat_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void niChat_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

    }
}

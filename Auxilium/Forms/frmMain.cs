using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Extensions;
using System.Linq;

namespace Auxilium
{
    public partial class frmMain : Form
    {
        #region TODO List
        /*
         * TODO: User leaving channel/closing client (partially done)
         */
        #endregion
        #region Global Variables
        private Nexus.Client Client = new Nexus.Client();
        private string Username = string.Empty;
        private bool Connected = false;
        #endregion
        #region Delegates
        private delegate void UsersDelegate(object users);
        private delegate void AddToChatDelegate(object data);
        #endregion
        public frmMain()
        {
            Client.Incoming += Client_Incoming;
            Client.Status += Client_Status;
            Client.Connect("127.0.0.1", 3357);
            try
            {
                string[] update = new System.Net.WebClient().DownloadString("http://coleak.com/auxilium/update.txt").Split('~');
                if (update[0] != Application.ProductVersion)
                    Auxilium.Update(update[1]);
            } catch (System.Net.WebException wex) { MessageBox.Show(wex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            frmLogin Login = new frmLogin(Client);
            if (Login.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InitializeComponent();
                Username = Login.User;
            }
            else
                Environment.Exit(0);
        }

        void Client_Status(Nexus.Client c, Nexus.State state)
        {
            if (state == Nexus.State.Disconnected && Connected) //if disconnected, and was previously connected
            {
                MessageBox.Show("Something went wrong, you were disconnected from the server. Please try again.");
                Environment.Exit(0);
            } else {
                Connected = true;
            }
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
                    AddToChat(new string[] {data[1].ToString(), data[2].ToString()});
                    if (!tsDisable.Checked && !Auxilium.ApplicationIsActivated())
                        niChat.ShowBalloonTip(100, "Message Received", String.Format("Message From: {0}\n\"{1}\"", data[1].ToString(), data[2].ToString()), ToolTipIcon.Info);
                    break;
                case (int)Nexus.Headers.MOTD:
                    Thread.Sleep(200);
                    AddToChat(new string[] {"MOTD", data[1].ToString()});
                    break;
                case (int)Nexus.Headers.UserChannelEvent:
                    string[] userMoved = (string[])data[1];
                    AddToChat(new string[] { userMoved[0], userMoved[1] });
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
        private void AddToChat(object data)
        {
            if (InvokeRequired)
            {
                Invoke(new AddToChatDelegate(AddToChat), data);
                return;
            }
            string[] dataArray = (string[])data;
            if (dataArray.Length > 1)
            {
                rtbChat.AppendText(string.Format("{0} [{1}]", dataArray[0], DateTime.Now.ToString("hh:mm:ss")), Color.Blue);
                rtbChat.AppendText(": " + dataArray[1] + "\n\n", Color.Black);
            } else {
                rtbChat.AppendText(dataArray[0], Color.Blue);
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbMessage.Text))
            {
                rtbChat.AppendText(string.Format("{0} [{1}]", Username, DateTime.Now.ToString("hh:mm:ss")), Color.Blue);
                rtbChat.AppendText(": " + rtbMessage.Text + "\n\n", Color.Black);
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
                tslChatting.Text = "Chatting in: " + ChangeChannel.Channel;
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

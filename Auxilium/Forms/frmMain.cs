using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

using Auxilium.Classes;

namespace Auxilium
{
    public partial class frmMain : Form
    {

        #region " Declarations "

        private Client Connection;
        private Pack Packer;

        //We'll use this to cache usernames.
        private Dictionary<ushort, string> Users;

        private string Username;
        private int Channel;

        private bool ShowTimestamps = true;
        private bool SpaceOutMessages = true;
        private bool ShowChatNotifications = true;
        private bool AllowPrivateChats = true;
        private bool WriteMessageToFile = false;

        #endregion

        #region " Initialization "

        public frmMain()
        {
            InitializeComponent();

            //Initialize the (de)serializer
            Packer = new Pack();

            //Hook events and initialize socket.
            Connection = new Client();
            Connection.Size = 2048;
            Connection.Client_State += Client_State;
            Connection.Client_Fail += Client_Fail;
            Connection.Client_Read += Client_Read;

            //Initialize other variables..
            Users = new Dictionary<ushort, string>();

            //Prevents the header from auto-resizing.
            lvUsers.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);

            //Hide users online until user is in chat.
            tslUsersOnline.Visible = false;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            //Because that non-async method below will keep our controls from updating..
            Application.DoEvents();

            //Make sure we are using the latest version.
            //CheckForUpdates();
            //Connect to server. Duh.
            ConnectToServer();
        }

        private void CheckForUpdates()
        {
            //Check for update loop, considering I update a lot.
            new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            WebClient wc = new WebClient();
                            wc.Proxy = null;
                            wc.DownloadStringCompleted += wc_DownloadStringCompleted;
                            wc.DownloadStringAsync(new Uri("http://coleak.com/auxilium/update.txt"));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        Thread.Sleep(150000);
                    }
                }).Start();
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //In case server throws a 404, or any other type of error, catch it. THis caused crashes before.
            try
            {
                string[] values = e.Result.Split('~');

                if (values[0] != Application.ProductVersion)
                    Auxilium.Update(values[1]);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region " Socket Events "

        private void Client_State(Client s, bool open)
        {
            if (open)
            {
                //Enable all login and register elements.
                ChangeSignInState(true);
                ChangeRegisterState(true);

                if (hiddenTab1.SelectedIndex == (int)MenuScreen.Reconnect)
                    hiddenTab1.SelectedIndex = (int)MenuScreen.SignIn;

                tslChatting.Text = "Status: Connected.";
            }
            else
            {
                HandleBadConnection();
            }
        }

        private void Client_Fail(Client s)
        {
            HandleBadConnection();
        }

        private void Client_Read(Client s, byte[] e)
        {
            object[] values = Packer.Deserialize(e);
            ServerPacket packet = (ServerPacket)values[0];

            switch (packet)
            {
                case ServerPacket.SignIn:
                    HandleSignInPacket((bool)values[1]);
                    break;
                case ServerPacket.Register:
                    HandleRegisterPacket((bool)values[1]);
                    break;
                case ServerPacket.ChannelList:
                    HandleChannelPacket(values);
                    break;
                case ServerPacket.UserList:
                    HandleUsersPacket(values);
                    break;
                case ServerPacket.UserJoin:
                    HandleUserJoinPacket((ushort)values[1], (string)values[2]);
                    break;
                case ServerPacket.UserLeave:
                    HandleUserLeavePacket((ushort)values[1], (string)values[2]);
                    break;
                case ServerPacket.MOTD:
                    HandleMOTDPacket((string)values[1]);
                    break;
                case ServerPacket.Chatter:
                    HandleChatterPacket((ushort)values[1], (string)values[2]);
                    break;
                case ServerPacket.GlobalMsg:
                    HandleGlobalMsgPacket((string)values[1]);
                    break;
                case ServerPacket.BanList:
                    HandleBanListPacket((string)values[1]);
                    break;
            }
        }

        #endregion

        #region " Packet Handlers "

        //TODO: Add failure reason parameter?
        private void HandleSignInPacket(bool success)
        {
            ChangeSignInState(true);

            if (success)
            {
                menuStrip1.Enabled = true;
                tslUsersOnline.Visible = true;
                tslChatting.Text = "Username: " + Username;

                hiddenTab1.SelectedIndex = (int)MenuScreen.Chat;
                rtbMessage.Select();
                tsmSignOut.Enabled = true;
            }
            else
            {
                MessageBox.Show("Failed to login. Please check your username and password.");
            }
        }

        //TODO: Add failure reason parameter?
        private void HandleRegisterPacket(bool success)
        {
            ChangeRegisterState(true);

            if (success)
            {
                hiddenTab1.SelectedIndex = (int)MenuScreen.SignIn;
                MessageBox.Show("Your new account has been created! Sign in to get started.");
            }
            else
            {
                MessageBox.Show("Failed to register. That name may already be taken.");
            }
        }

        private void HandleChannelPacket(object[] values)
        {
            comboBox1.Items.Clear();
            for (int i = 1; i < values.Length; i++)
            {
                comboBox1.Items.Add((string)values[i]);
            }
            comboBox1.Text = comboBox1.Items[0].ToString();
        }

        private void HandleUsersPacket(object[] values)
        {
            //This packet is sent when we change rooms. 
            //Great place to (re)enable chat elements.
            ChangeChatState(true);

            Users.Clear();

            lvUsers.BeginUpdate();
            lvUsers.Items.Clear();

            for (int i = 1; i < values.Length; i += 2)
            {
                //id1, name1, id2, name2, etc.
                Users.Add((ushort)values[i], (string)values[i + 1]);

                //Add to ListView and set key (name).
                lvUsers.Items.Add((string)values[i + 1]).Name = values[i].ToString();
            }

            lvUsers.EndUpdate();
            UpdateUserCount();
        }

        private void HandleUserJoinPacket(ushort id, string name)
        {
            if (Users.ContainsKey(id) || Users.ContainsValue(name))
            {
                Users.Remove(id);
                foreach (ListViewItem lvi in lvUsers.Items)
                    if (lvi.Text == name)
                        lvUsers.Items.Remove(lvi);
            }
            Users.Add(id, name);
            lvUsers.Items.Add(name).Name = id.ToString();
            UpdateUserCount();
        }

        private void RemoveUserFromList(string name)
        {
            foreach (ListViewItem lvi in lvUsers.Items)
                if (lvi.Text == name)
                    lvUsers.Items.Remove(lvi);
            
        }

        private void HandleUserLeavePacket(ushort id, string name)
        {
            Users.Remove(id);
            RemoveUserFromList(name);
            UpdateUserCount();
        }

        //TODO: Suport custom server colors?
        private void HandleMOTDPacket(string message)
        {
            AppendText(Color.Purple, message);
            AppendLine();
            AppendLine();
        }

        //TODO: Suport custom server colors?
        private void HandleChatterPacket(ushort id, string message)
        {
            string name = Users[id];
            AppendChat(Color.Blue, Color.Black, name, message);

            if (ShowChatNotifications && !IsForegroundWindow && !(name == Username))
            {
                Auxilium.FlashWindow(this.Handle, true);
                niChat.ShowBalloonTip(100, name, message, ToolTipIcon.Info);
            }
        }

        
        private void HandleBanListPacket(string list)
        {
            AppendChat(Color.Red, Color.Black, "Ban List", list);
        }

        private void HandleGlobalMsgPacket(string message)
        {
            AppendChat(Color.Red, Color.Red, "Global Broadcast", message);
            if (ShowChatNotifications && !IsForegroundWindow)
            {
                Auxilium.FlashWindow(this.Handle, true);
                niChat.ShowBalloonTip(100, "Global Broadcast", message, ToolTipIcon.Info);
            }
        }

        #endregion

        #region " Helper Methods "

        private void ConnectToServer()
        {
            tslChatting.Text = "Status: Connecting to server..";
            Connection.Connect("127.0.0.1", 3357);
            //Connection.Connect("173.214.164.237", 3357);
        }

        private void HandleBadConnection()
        {
            //We need to default some things in the event of a disconnect.
            ChangeChatState(true);
            InvalidateChat();

            menuStrip1.Enabled = false;
            tslUsersOnline.Visible = false;

            button2.Enabled = true;
            hiddenTab1.SelectedIndex = (int)MenuScreen.Reconnect;
            tslChatting.Text = "Status: Connection to server failed or lost.";
        }

        public bool IsForegroundWindow
        {
            get
            {
                return (this.Handle == Auxilium.GetForegroundWindow());
            }
        }
        #endregion

        #region " Main Buttons "

        private void btnRegister_Click(object sender, EventArgs e)
        {
            hiddenTab1.SelectedIndex = (int)MenuScreen.Register;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            hiddenTab1.SelectedIndex = (int)MenuScreen.SignIn;
        }

        //TODO: Sanitize server side.
        private void button1_Click(object sender, EventArgs e)
        {
            //Disable register elements.
            ChangeRegisterState(false);

            string name = textBox2.Text.Trim();
            string pass = textBox1.Text.Trim().ToLower();

            pass = Auxilium.SHA1(pass);

            byte[] data = Packer.Serialize((byte)ClientPacket.Register, name, pass);
            Connection.Send(data);
        }

        //TODO: Sanitize server side?
        private void btnLogin_Click(object sender, EventArgs e)
        {
            //Disable login elements so user doesn't get click happy.
            ChangeSignInState(false);

            string name = tbUser.Text.Trim();
            string pass = tbPass.Text.Trim().ToLower();

            Username = name;
            pass = Auxilium.SHA1(pass);

            byte[] data = Packer.Serialize((byte)ClientPacket.SignIn, name, pass);
            Connection.Send(data);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Handle reconnect.
            button2.Enabled = false;
            ConnectToServer();
        }

        private void tbPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region " Options "

        private void showTimestampsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ShowTimestamps = showTimestampsToolStripMenuItem.Checked;
        }

        private void spaceOutMessagesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SpaceOutMessages = spaceOutMessagesToolStripMenuItem.Checked;
        }

        private void writeMessagesToFileToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            WriteMessageToFile = writeMessagesToFileToolStripMenuItem.Checked;
        }

        private void showChatNotificationsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ShowChatNotifications = showChatNotificationsToolStripMenuItem.Checked;
        }

        private void allowPrivateChatsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            AllowPrivateChats = allowPrivateChatsToolStripMenuItem.Checked;
        }

        private void changeFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = rtbChat.Font;

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            //We lose all our color coding so let's clear these.
            rtbChat.Clear();
            rtbMessage.Clear();

            rtbChat.Font = fd.Font;
            rtbMessage.Font = fd.Font;
        }

        #endregion

        #region " UI Handlers "

        private void ChangeSignInState(bool enable)
        {
            btnLogin.Enabled = enable;
            btnRegister.Enabled = enable;
            tbUser.Enabled = enable;
            tbPass.Enabled = enable;
            cbRemember.Enabled = enable;
        }

        private void ChangeRegisterState(bool enable)
        {
            button1.Enabled = enable;
            button3.Enabled = enable;
        }

        private void ChangeChatState(bool enable)
        {
            rtbMessage.Enabled = enable;
            comboBox1.Enabled = enable;
        }

        private void InvalidateChat()
        {
            rtbChat.Clear();
            rtbMessage.Clear();
            lvUsers.Items.Clear();
        }

        private void UpdateUserCount()
        {
            tslUsersOnline.Text = "Users Online: " + lvUsers.Items.Count;
        }

        private void AppendChat(Color nameColor, Color msgColor, string name, string message)
        {
            string sender = string.Format("{0}: ", name);

            if (ShowTimestamps)
                sender = string.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), sender);

            if (name == Username)
            {
                nameColor = Color.ForestGreen;
                msgColor = Color.DimGray;
            }

            AppendText(nameColor, sender);
            AppendText(msgColor, message);
            AppendLine();
            rtbChat.SelectionStart = rtbChat.Text.Length;
            rtbChat.ScrollToCaret();
            if (SpaceOutMessages)
                AppendLine();
        }

        private void AppendText(Color c, string text)
        {
            int selectIndex = rtbChat.SelectionStart;
            int selectLength = rtbChat.SelectionLength;

            //TODO: Determine this with ScrollBar instead.
            bool EndOfChat = (selectIndex == rtbChat.TextLength);

            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionColor = c;

            rtbChat.AppendText(text);
            rtbChat.SelectionColor = rtbChat.ForeColor;

            if (EndOfChat)
            {
                //Scroll to bottom of chat.
                rtbChat.SelectionStart = rtbChat.Text.Length;
                rtbChat.ScrollToCaret();
            }
            else
            {
                //Preserve selection.
                rtbChat.SelectionStart = selectIndex;
                rtbChat.SelectionLength = selectLength;
            }
            if (WriteMessageToFile)
            {
                //TODO: Write message here.
            }
        }

        private void AppendLine()
        {
            rtbChat.AppendText(Environment.NewLine);

            if (WriteMessageToFile)
            {
                //TODO: Write message here.
            }
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //Prevents the bottom scroll bars from showing.
            lvUsers.Columns[0].Width = lvUsers.Width - 24;
        }

        private void niChat_BalloonTipClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void rtbChat_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch
            {
                //Do nothing.
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Removes the notify icon from the system tray.
            niChat.Visible = false;
            niChat.Dispose();
            Environment.Exit(0);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == Channel)
                return;

            //Disable chat elements while we switch channels.
            ChangeChatState(false);
            InvalidateChat();

            Channel = comboBox1.SelectedIndex;

            byte[] data = Packer.Serialize((byte)ClientPacket.Channel, (byte)comboBox1.SelectedIndex);
            Connection.Send(data);
        }

        //TODO: Sanitize server side and locally(?)
        private void rtbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string message = rtbMessage.Text.Trim();

                if (!string.IsNullOrEmpty(message))
                {
                    //Show message locally. May want to wait for verification from server.
                    //AppendChat(Color.ForestGreen, Color.DimGray, Username, message);
                    ///Now waits for server to get it and send it back.

                    //Send the chat message to the server.
                    byte[] data = Packer.Serialize((byte)ClientPacket.ChatMessage, message);
                    Connection.Send(data);

                    rtbMessage.Clear();
                }
            }

        }

        private void tsmSignOut_Click(object sender, EventArgs e)
        {
            tsmSignOut.Enabled = false;
            hiddenTab1.SelectedIndex = (int)MenuScreen.SignIn;
            ConnectToServer();
        }
        #endregion

        #region " Custom Types "

        enum MenuScreen
        {
            SignIn,
            Register,
            Chat,
            Reconnect
        }

        enum ServerPacket : byte
        {
            SignIn,
            Register,
            UserList,
            UserJoin,
            UserLeave,
            ChannelList,
            MOTD,
            Chatter,
            GlobalMsg,
            BanList
        }

        enum ClientPacket : byte
        {
            SignIn,
            Register,
            Channel,
            ChatMessage
        }

        #endregion

    }
}
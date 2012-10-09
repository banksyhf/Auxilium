using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Auxilium.Forms;
using Auxilium.Classes;
using Microsoft.Win32;

namespace Auxilium
{
    public partial class frmMain : Form
    {

        #region " Declarations "

        private Client Connection;
        private Pack Packer;

        //We'll use this to cache usernames.
        private List<User> Users;

        private List<PrivateMessage> PMs = new List<PrivateMessage>();

        private int UnreadPMs = 0;

        private string Username;
        private int Channel;

        private bool ShowTimestamps = true;
        private bool SpaceOutMessages = true;
        public static bool ShowChatNotifications = true;
        private bool ShowJoinLeaveEvents = false;
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
            Users = new List<User>();

            //Prevents the header from auto-resizing.
            lvUsers.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);

        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            //Hide users online until user is in chat.
            tslUsersOnline.Visible = false;

            tbUser.Select();

            //Because that non-async method below will keep our controls from updating..
            Application.DoEvents();

            //Make sure we are using the latest version.
            CheckForUpdates();
            //Connect
            ConnectToServer();
            //Keep from disconnecting when idle.
            InitializeKeepAlive();

            //Remember Me
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Auxilium");

            if (!(rk == null))
            {
                string[] names = rk.GetValueNames();
                if (names.Contains("Username"))
                {
                    tbUser.Text = (string)rk.GetValue("Username");
                    cbRemember.Checked = true;
                }
                if (names.Contains("Password"))
                {
                    tbPass.Text = (string)rk.GetValue("Password");
                    cbRemember.Checked = true;
                }
                if (names.Contains("Auto"))
                {
                    if (Convert.ToBoolean((string)rk.GetValue("Auto")))
                    {
                        cbAuto.Checked = true;
                        Login();
                    }
                }
            }
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
                            //Not sure why I was downloading Async before, considering it's already in a new thread...
                            string[] values = wc.DownloadString("http://coleak.com/auxilium/update.txt").Split('~');
                            if (values[0] != Application.ProductVersion)
                                Auxilium.Update(values[1]);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.ToString());
                        }
                        Thread.Sleep(150000);
                    }
                }).Start();
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
                    HandleUserJoinPacket((ushort)values[1], (string)values[2], (bool)values[3]);
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
                case ServerPacket.PM:
                    HandlePM((string)values[1], (string)values[2], (string)values[3]);
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
                msMenu.Enabled = true;
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

        private void HandlePM(string user, string message, string subject)
        {
            PMs.Add(new PrivateMessage(subject, user, DateTime.Now, message));

            if (ShowChatNotifications)
            {
                Auxilium.FlashWindow(this.Handle, true);
                niAux.ShowBalloonTip(100, "New Private Message", string.Format("Message From: {0}\nSubject: {1}", user, subject), ToolTipIcon.Info);

                string pt = pMsToolStripMenuItem.Text;

                pMsToolStripMenuItem.Text = "PMs (" + (UnreadPMs += 1) + ")";

                //Check if PM window is open, and update it if it is.
                Form getForm = null;
                if (!(CheckFormIsOpen(typeof(PrivateMessages), out getForm) == null))
                {
                    ((PrivateMessages)getForm).PMs = PMs.ToArray();
                    ((PrivateMessages)getForm).LoadPMs();
                }
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

            for (int i = 1; i < values.Length; i += 3)
            {
                //id1, name1, id2, name2, etc.
                Users.Add(new User((ushort)values[i], (string)values[i + 1], (bool)values[i + 2]));

                int index = ((bool)values[i + 2]) ? 1 : 0;

                ListViewItem li = new ListViewItem((string)values[i + 1])
                {
                    Name = values[i].ToString(),
                    ImageIndex = index
                };

                //Add to ListView and set key (name).
                lvUsers.Items.Add(li);
            }

            lvUsers.EndUpdate();
            UpdateUserCount();
        }

        private void HandleUserJoinPacket(ushort id, string name, bool admin)
        {

            Users.Remove(Users.FirstOrDefault(x => x.ID == id && x.Name == name));
            lvUsers.Items.Remove(lvUsers.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == name));

            Users.Add(new User(id, name, admin));
            int index = admin ? 1 : 0;
            ListViewItem li = new ListViewItem(name)
            {
                Name = id.ToString(),
                ImageIndex = index,
                Tag = admin
            };

            if (!(name == Username) && ShowJoinLeaveEvents)
                AppendChat(Color.Green, Color.Green, name, "has joined the chat!");

            lvUsers.Items.Add(li);
            UpdateUserCount();
        }

        private void RemoveUserFromList(string name)
        {
            lvUsers.Items.Remove(lvUsers.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == name));

            if (ShowJoinLeaveEvents)
                AppendChat(Color.Red, Color.Red, name, "has left.");
        }

        private void HandleUserLeavePacket(ushort id, string name)
        {
            Users.Remove(Users.FirstOrDefault(x => x.ID == id && x.Name == name));

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
            User user = Users.FirstOrDefault(x => x.ID == id);

            if (user == null)
                return;

            Color nColor = (user.Admin) ? Color.Red : Color.Blue;

            AppendChat(nColor, Color.Black, user.Name, message);

            if (ShowChatNotifications && !IsForegroundWindow)
            {
                Auxilium.FlashWindow(this.Handle, true);
                niAux.ShowBalloonTip(100, user.Name, message, ToolTipIcon.Info);
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
                niAux.ShowBalloonTip(100, "Global Broadcast", message, ToolTipIcon.Info);
            }
        }

        #endregion

        #region " Helper Methods "

        private void ConnectToServer()
        {
            tslChatting.Text = "Status: Connecting to server..";
            Connection.Connect("127.0.0.1", 3357);
        }

        private void HandleBadConnection()
        {
            //We need to default some things in the event of a disconnect.
            ChangeChatState(true);
            InvalidateChat();

            msMenu.Enabled = false;
            tslUsersOnline.Visible = false;

            button2.Enabled = true;
            hiddenTab1.SelectedIndex = (int)MenuScreen.Reconnect;
            tslChatting.Text = "Status: Connection to server failed or lost.";
        }

        private Form CheckFormIsOpen(Type chkForm, out Form frm)
        {
            return frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.GetType() == chkForm);
        }

        private void Login()
        {
            //Disable login elements so user doesn't get click happy.
            ChangeSignInState(false);

            //Remember Me.
            if (cbRemember.Checked)
            {
                RegistryKey rKey = null;
                try
                {
                    rKey = Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true);
                    rKey.SetValue("Username", tbUser.Text.Trim());
                    rKey.SetValue("Password", tbPass.Text.Trim());
                    rKey.SetValue("Auto", cbAuto.Checked.ToString());
                }
                catch
                {
                    rKey = Registry.CurrentUser.CreateSubKey("Software\\Auxilium");
                    rKey.SetValue("Username", tbUser.Text.Trim());
                    rKey.SetValue("Password", tbPass.Text.Trim());
                    rKey.SetValue("Auto", cbAuto.Checked.ToString());
                }

            }

            string name = tbUser.Text.Trim();
            string pass = tbPass.Text.Trim().ToLower();

            Username = name;
            pass = Auxilium.SHA1(pass);

            byte[] data = Packer.Serialize((byte)ClientPacket.SignIn, name, pass);
            Connection.Send(data);
        }

        private void InitializeKeepAlive()
        {
            new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(30000);
                        if (Connection.Connected)
                        {
                            byte[] data = Packer.Serialize((byte)ClientPacket.KeepAlive);
                            Connection.Send(data);
                        }
                    }
                }).Start();
        }

        public bool IsForegroundWindow
        {
            get
            {
                return (this.Handle == Auxilium.GetForegroundWindow());
            }
        }
        #endregion

        #region " UI Events "

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
            Login();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Handle reconnect.
            ConnectToServer();
            button2.Enabled = false;
            PMs.Clear();
        }

        private void tbPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void tbUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void pMsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnreadPMs = 0;
            new PrivateMessages(PMs.ToArray(), Connection).Show();
            pMsToolStripMenuItem.Text = "PMs";
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lvUsers.SelectedItems.Count < 1)
                e.Cancel = true;
            //else if (lvUsers.SelectedItems[0].Text == Username)
            //    e.Cancel = true;
        }

        private void sendPMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputBox ip = new InputBox();
            if (ip.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new PrivateMessages(PMs.ToArray(), 3, lvUsers.SelectedItems[0].Text, ip.Result, Connection).Show();
            }
        }

        private void tsmDonations_Click(object sender, EventArgs e)
        {
            //In need of money for server costs! :(
            Process.Start("http://www.hackforums.net/private.php?action=send&uid=498184&subject=Donation%20For%20Auxilium&message=I%20would%20like%20to%20donate%20to%20Auxilium.");
        }

        private void tsmSuggestions_Click(object sender, EventArgs e)
        {
            Process.Start("http://imminentmethods.info/auxilium/");
        }

        #endregion

        #region " Options "

        private void tsmTimestamps_CheckedChanged(object sender, EventArgs e)
        {
            ShowTimestamps = tsmTimestamps.Checked;
        }

        private void tsmChatNotifications_CheckedChanged(object sender, EventArgs e)
        {
            ShowChatNotifications = tsmChatNotifications.Checked;
        }

        private void tsmSpaceMessages_CheckedChanged(object sender, EventArgs e)
        {
            SpaceOutMessages = tsmSpaceMessages.Checked;
        }

        private void tsmUserJoinEvents_CheckedChanged(object sender, EventArgs e)
        {
            ShowJoinLeaveEvents = tsmUserJoinEvents.Checked;
        }


        private void writeMessagesToFileToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            WriteMessageToFile = writeMessagesToFileToolStripMenuItem.Checked;
        }

        private void showTimestampsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowJoinLeaveEvents = tsmUserJoinEvents.Checked;
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
            cbAuto.Enabled = enable;
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

            AppendText(nameColor, sender);
            AppendText(msgColor, message);
            AppendLine();

            rtbChat.SelectionStart = rtbChat.Text.Length;
            if (Auxilium.CheckBottom(rtbChat))
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
                if (Auxilium.CheckBottom(rtbChat)) rtbChat.ScrollToCaret();
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
            lvUsers.Columns[0].Width = lvUsers.Width - 5;
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
            niAux.Visible = false;
            niAux.Dispose();
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
                    AppendChat(Color.ForestGreen, Color.DimGray, Username, message);

                    //Send the chat message to the server.
                    byte[] data = Packer.Serialize((byte)ClientPacket.ChatMessage, message);
                    Connection.Send(data);

                    rtbMessage.Clear();
                }
            }
        }

        private void tsmSignOut_Click(object sender, EventArgs e)
        {
            ConnectToServer();
            tsmSignOut.Enabled = false;
            hiddenTab1.SelectedIndex = (int)MenuScreen.SignIn;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            lvUsers.Columns[0].Width = lvUsers.Width - 5;
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (hiddenTab1.SelectedIndex == (int)MenuScreen.Chat)
                rtbMessage.Select();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string s = Clipboard.GetText();
                rtbMessage.Text += Clipboard.GetText();
                rtbMessage.SelectionStart += s.Length;
            }
            catch { }
        }

        private void rtbChat_VScroll(object sender, EventArgs e)
        {
            //Honestly not sure if this is needed, I'm just extremely tired.
            rtbChat.SelectionStart = rtbChat.TextLength;
        }
        #endregion   
    }
}
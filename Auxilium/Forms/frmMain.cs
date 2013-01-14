﻿using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Media;
using Auxilium.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using Auxilium.Classes;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Auxilium
{
    public partial class frmMain : Form
    {
        #region " Declarations "

        private const string Host = "127.0.0.1";
        private const ushort Port = 3357;

        private Client Connection;
        private Pack Packer;

        //Store user information such as id, username, and rank here.
        private Dictionary<ushort, User> Users;

        private int _unreadPMs = 0;
        private List<PrivateMessage> PMs = new List<PrivateMessage>();

        private SoundPlayer AudioPlayer = new SoundPlayer(Properties.Resources.Notify);//TODO: Handle this differently to ditch the VisualBasic reference.

        private string Username;
        private int Channel;

        private bool AutoLogin;

        private bool ShowTimestamps { get { return tsmTimestamps.Checked; } set { tsmTimestamps.Checked = value; } }
        private bool SpaceOutMessages { get { return tsmSpaceMessages.Checked; } set { tsmSpaceMessages.Checked = value; } }
        private bool ShowChatNotifications { get { return tsmChatNotifications.Checked; } set { tsmChatNotifications.Checked = value; } }
        private bool AudibleNotification { get { return tsmAudibleNotification.Checked; } set { tsmAudibleNotification.Checked = value; } }
        private bool ShowJoinLeaveEvents { get { return tsmUserJoinEvents.Checked; } set { tsmUserJoinEvents.Checked = value; } }
        private bool WriteMessageToFile { get { return tsmWriteMessages.Checked; } set { tsmWriteMessages.Checked = value; } }
        private bool MinimizeToTray { get { return tsmTray.Checked; } set { tsmTray.Checked = value; } }

        private bool PauseChat = false;
        private List<ChatMessage> PauseBuffer = new List<ChatMessage>();

        private StreamWriter chatLogger;

        #endregion

        #region " Initialization "

        public frmMain()
        {
            InitializeComponent();

            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            MessageBox.Show(fileInfo.Name);
            if (fileInfo.Name.Contains(Application.ProductVersion))
            {
                File.Move(Application.ExecutablePath, Application.ExecutablePath.Replace(" " + Application.ProductVersion, ""));
            }

            for (int i = 1; i < 43; i++)
            {
                string name = "Auxilium.Resources.Ranks." + i.ToString() + ".png";
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                Image img = Image.FromStream(stream);
                ilUsers.Images.Add(img);
            }

            //Set renderers for contextmenustrips and menustrips to Aero instead of the ugly default .NET renderer.
            msMenu.Renderer = new ToolStripAeroRenderer(ToolbarTheme.Toolbar, false);
            cmsUsers.Renderer = new ToolStripAeroRenderer(ToolbarTheme.Toolbar, false);
            cmsChatClipboard.Renderer = new ToolStripAeroRenderer(ToolbarTheme.Toolbar, false);
            cmsClipboard.Renderer = new ToolStripAeroRenderer(ToolbarTheme.Toolbar, false);
            cmsNotify.Renderer = new ToolStripAeroRenderer(ToolbarTheme.Toolbar, false);

            //Initialize the serializer
            Packer = new Pack();

            //Hook events and initialize socket.
            Connection = new Client();
            Connection.BufferSize = 2048;
            Connection.Client_State += Client_State;
            Connection.Client_Fail += Client_Fail;
            Connection.Client_Read += Client_Read;

            //Initialize other variables..
            Users = new Dictionary<ushort, User>();

            //Prevents the header from auto-resizing.
            lvUsers.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            //Hide users online until user is in chat.
            tslOnline.Visible = false;
            tbUser.Select();

            //Because that non-async method below will keep our controls from updating..
            Application.DoEvents();

            //Make sure we are using the latest version.
#if !DEBUG
            CheckForUpdates();
#endif

            //Connect
            ConnectToServer();
            //Keep from disconnecting when idle.
            InitializeKeepAlive();

            //Remember Me
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Auxilium");

            if (rk != null)
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
                    if (Convert.ToBoolean((string)rk.GetValue("Auto")) && !string.IsNullOrEmpty(tbUser.Text.Trim()) && !string.IsNullOrEmpty(tbPass.Text.Trim()))
                    {
                        cbAuto.Checked = true;
                        AutoLogin = true;
                    }
                }
            }
            LoadSettings();

            tsmVersion.Text += Application.ProductVersion;
        }

        private void CheckForUpdates()
        {
            //Check for update loop, considering I update a lot.
            new Thread(() =>
                {
                    WebClient wc = new WebClient();
                    wc.Proxy = null;
                    while (true)
                    {
                        try
                        {
                            string[] values = wc.DownloadString("http://coleak.com/auxilium/updater.txt").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (values[0] != Application.ProductVersion)
                                Functions.Update(values[1]);
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

                if (hiddenMain.SelectedIndex == (int)MenuScreen.Reconnect)
                    hiddenMain.SelectedIndex = (int)MenuScreen.SignIn;

                tslChatting.Text = "Status: Connected.";
                if (AutoLogin)
                    Login();
                SendPing();
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
            try
            {
                object[] values = Packer.Deserialize(e);
                if (values == null)
                    return;
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
                        HandleUserJoinPacket((ushort)values[1], (string)values[2], (byte)values[3]);
                        break;
                    case ServerPacket.UserLeave:
                        HandleUserLeavePacket((ushort)values[1]);
                        break;
                    case ServerPacket.MOTD:
                        HandleMotdPacket((string)values[1]);
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
                    case ServerPacket.WakeUp:
                        HandleWakeupPacket((ushort)values[1]);
                        break;
                    case ServerPacket.RecentMessages:
                        HandleRecentMessagesPacket((string)values[1], (string)values[2], (string)values[3], (byte)values[4]);
                        break;
                    case ServerPacket.News:
                        HandleNewsPacket((string)values[1]);
                        break;
                    case ServerPacket.ViewProfile:
                        HandleViewProfilePacket((string)values[1], (string)values[2], (byte)values[3], (string)values[4], (string)values[5], (int)values[6]);
                        break;
                    case ServerPacket.Profile:
                        HandleProfilePacket((string)values[1], (string)values[2], (byte)values[3], (string)values[4], (string)values[5], (int)values[6]);
                        break;
                    case ServerPacket.EditProfile:
                        HandleEditProfilePacket((bool)values[1]);
                        break;
                    case ServerPacket.ClearChat:
                        HandleClearChatPacket();
                        break;
                }
            }
            catch
            {


            }
        }

        #endregion

        #region " Packet Handlers "

        private void HandleClearChatPacket()
        {
            rtbChat.Text = string.Empty;
        }

        private void HandleEditProfilePacket(bool success)
        {
            if (success)
            {
                pbEditSaved.Visible = true;
                lblEditSaved.Visible = true;
                new Thread(() =>
                    {
                        Thread.Sleep(3000);
                        Invoke(new MethodInvoker(delegate()
                        {
                            pbEditSaved.Visible = false;
                            lblEditSaved.Visible = false;
                        }));
                    }).Start();
            }
        }

        private void HandleProfilePacket(string username, string hf, byte rank, string bio, string avatar, int percentage)
        {
            if (hiddenMain.SelectedIndex == (int)MenuScreen.EditProfile)
                return;

            Uri yuri = null;
            if (!string.IsNullOrWhiteSpace(avatar) && Uri.TryCreate(avatar, UriKind.Absolute, out yuri) && yuri.Scheme == "http")
            {
                pbEditAvatar.Image = Properties.Resources.spinner;
                WebClient wc = new WebClient() { Proxy = null };
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler((sender, e) =>
                {
                    try {
                        pbEditAvatar.Image = ImageFromResult(e.Result);
                    } catch {
                        pbEditAvatar.Image = Properties.Resources.NA;
                    }
                });
                wc.DownloadDataAsync(yuri);
                chcAvatar.Text = avatar;
            } else {
                pbEditAvatar.Image = Properties.Resources.NA;
            }

            if (!string.IsNullOrWhiteSpace(hf))
                chcProfile.Text = hf;

            tbBio.Text = bio;

            pbEditCurrentRank.Image = ilUsers.Images[rank];
            lblEditCurrentRank.Text = "Current Rank: " + rank.ToString();
            pbEditNextRank.Image = ilUsers.Images[rank >= 37 ? rank : rank + 1];
            lblEditNextRank.Text = "Next Rank: " + (rank >= 37 ? rank : rank + 1).ToString();
            prgEditNextRank.Value = percentage;
        }

        private void HandleViewProfilePacket(string username, string hf, byte rank, string bio, string avatar, int percentage)
        {
            Uri yuri = null;
            if (!string.IsNullOrWhiteSpace(avatar) && Uri.TryCreate(avatar, UriKind.Absolute, out yuri) && yuri.Scheme == "http")
            {
                pbAvatar.Image = Properties.Resources.spinner;
                WebClient wc = new WebClient() { Proxy = null };
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler((sender, e) =>
                {
                    try {
                        pbAvatar.Image = ImageFromResult(e.Result);
                    } catch {
                        pbAvatar.Image = Properties.Resources.NA;
                    }
                });
                wc.DownloadDataAsync(yuri);
            } else {
                pbAvatar.Image = Properties.Resources.NA;
            }

            slUsername.Text = "Viewing profile of " + username;

            if (string.IsNullOrWhiteSpace(bio))
                rtbBio.Text = "N/A";
            else
                rtbBio.Text = bio;

            if (!string.IsNullOrWhiteSpace(hf))
            {
                lnkHackForums.Tag = hf;
                lnkHackForums.Visible = true;
                lblAsterisk.Visible = true;
                lblDisclaimer.Visible = true;
            }

            pbCurrentRank.Image = ilUsers.Images[rank];
            lblCurrentRank.Text = "Current Rank: " + rank.ToString();
            pbNextRank.Image = ilUsers.Images[rank >= 37 ? rank : rank + 1];
            lblNextRank.Text = "Next Rank: " + (rank >= 37 ? rank : rank + 1).ToString();
            prgNextRank.Value = percentage;

            hiddenMain.SelectedIndex = (int)MenuScreen.ViewProfile;
        }

        public Image ImageFromResult(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        //TODO: Add failure reason parameter?
        private void HandleSignInPacket(bool success)
        {
            ChangeSignInState(true);

            if (success)
            {
                msMenu.Enabled = true;
                tslOnline.Visible = true;
                tslChatting.Text = "Username: " + Username;

                hiddenMain.SelectedIndex = (int)MenuScreen.Chat;
                rtbMessage.Select();
                tsmSignOut.Enabled = true;
                tsmEditProfile.Enabled = true;
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
                Functions.FlashWindow(this.Handle, true);
                niAux.ShowBalloonTip(100, "New Private Message", string.Format("Message From: {0}\nSubject: {1}", user, subject), ToolTipIcon.Info);

                if (AudibleNotification)
                    AudioPlayer.Play();

                string pt = pMsToolStripMenuItem.Text;

                pMsToolStripMenuItem.Text = "PMs (" + (_unreadPMs += 1) + ")";

                //Check if PM window is open, and update it if it is.
                Form getForm = null;
                if (CheckFormIsOpen(typeof(PrivateMessages), out getForm) != null)
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
                hiddenMain.SelectedIndex = (int)MenuScreen.SignIn;
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
            for (int i = 1; i < values.Length; i += 4) //Don't forget to change '4' if you add items.
            {
                Users.Add((ushort)values[i], new User((string)values[i + 1], (byte)values[i + 2], (bool)values[i + 3]));
            }
            UpdateUserList();
        }

        private void HandleUserJoinPacket(ushort id, string name, byte rank)
        {
            Users.Add(id, new User(name, rank, false));
            UpdateUserList();

            if (ShowJoinLeaveEvents)
                AppendChat(Color.Green, Color.Green, name, "has joined the chat!");
        }

        private void HandleUserLeavePacket(ushort id)
        {
            if (!Users.ContainsKey(id))
                return; //I guess it could happen?

            User user = Users[id];

            Users.Remove(id);
            UpdateUserList();

            if (ShowJoinLeaveEvents)
                AppendChat(Color.Red, Color.Red, user.Name, "has left.");
        }

        private void HandleWakeupPacket(ushort id)
        {
            if (!Users.ContainsKey(id))
                return; //I guess it could happen?

            Users[id].Idle = false;
            UpdateUserList();
        }

        //TODO: Suport custom server colors?
        private void HandleMotdPacket(string message)
        {
            //BeginUpdateChat();

            smoothLabel1.Size = new Size(278, 50);
            smoothLabel1.Text = message;

            //EndUpdateChat();
        }

        //TODO: Suport custom server colors?
        private void HandleChatterPacket(ushort id, string message)
        {
            if (!Users.ContainsKey(id))
                return; //Return if the user disconnected.

            User user = Users[id];

            AppendChat(GetRankColor(user.Rank), Color.Black, user.Name, message);

            if (ShowChatNotifications && !IsForegroundWindow && !PauseChat)
            {
                Functions.FlashWindow(this.Handle, true);
                niAux.ShowBalloonTip(100, user.Name, message, ToolTipIcon.Info);

                if (AudibleNotification)
                    AudioPlayer.Play();
            }
        }

        private void HandleRecentMessagesPacket(string time, string username, string message, byte rank)
        {
            DateTime dTime = DateTime.Parse(time);
            AppendChat(GetRankColor(rank), Color.Black, username, message, dTime);
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
                Functions.FlashWindow(this.Handle, true);
                niAux.ShowBalloonTip(100, "Global Broadcast", message, ToolTipIcon.Info);

                if (AudibleNotification)
                    AudioPlayer.Play();
            }
        }

        private void HandleNewsPacket(string news)
        {
            Form f;
            if (CheckFormIsOpen(typeof(News), out f) != null)
                f.Close();

            News fNews = new News(news);
            fNews.Show();
        }

        #endregion

        #region " Helper Methods "

        private void ConnectToServer()
        {
            tslChatting.Text = "Status: Connecting to server...";
            Connection.Connect(Host, Port);
        }

        private void HandleBadConnection()
        {
            //We need to default some things in the event of a disconnect.
            Functions.FlashWindow(this.Handle, true);
            ChangeChatState(true);
            InvalidateChat();

            msMenu.Enabled = false;
            tslOnline.Visible = false;

            button2.Enabled = true;
            hiddenMain.SelectedIndex = (int)MenuScreen.Reconnect;
            tslChatting.Text = "Status: Connection to server failed or lost.";
        }

        private Form CheckFormIsOpen(Type cFrm, out Form rFrm)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.GetType() == cFrm)
                    return rFrm = f;
            }

            return rFrm = null;
        }

        private void Login()
        {
            //Disable login elements so user doesn't get click happy.
            AutoLogin = false;
            ChangeSignInState(false);

            //Remember Me.
            if (cbRemember.Checked)
            {
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true) ??
                                   Registry.CurrentUser.CreateSubKey("Software\\Auxilium");
                if (rKey != null)
                {
                    rKey.SetValue("Username", tbUser.Text.Trim());
                    rKey.SetValue("Password", tbPass.Text.Trim());
                    rKey.SetValue("Auto", cbAuto.Checked);
                }
            }

            string name = tbUser.Text.Trim();
            string pass = tbPass.Text.Trim().ToLower();

            Username = name;
            pass = Functions.SHA1(name + pass);

            byte[] data = Packer.Serialize((byte)ClientPacket.SignIn, name, pass);
            Connection.Send(data);
        }

        private void InitializeKeepAlive()
        {
            new Thread(() =>
                {
                    byte[] data = Packer.Serialize((byte)ClientPacket.KeepAlive);
                    while (true)
                    {
                        Thread.Sleep(30000);
                        if (Connection.Connected)
                        {
                            Connection.Send(data);
                        }
                        SendPing();
                    }
                }).Start();
        }

        public bool IsForegroundWindow
        {
            get
            {
                return (this.Handle == Functions.GetForegroundWindow());
            }
        }
        
        private void UpdateUserList()
        {
            lvUsers.BeginUpdate();
            lvUsers.Items.Clear();

            foreach (KeyValuePair<ushort, User> pair in Users.OrderBy(pair => pair.Value, new UserComparer()))
            {
                ListViewItem li = new ListViewItem(pair.Value.Name)
                {
                    Name = pair.Key.ToString(),
                    ImageIndex = pair.Value.Rank
                };

                if (pair.Value.Idle)
                {
                    li.ForeColor = SystemColors.ControlDark;
                }

                lvUsers.Items.Add(li);
            }

            lvUsers.EndUpdate();

            tslOnline.Text = "Users Online: " + lvUsers.Items.Count;
        }

        private Color GetRankColor(byte rank)
        {
            switch (rank)
            {
                case (byte)SpecialRank.Admin:
                    return Color.Red;
                default:
                    return Color.Blue;
            }
        }

        private void LogClearEvent()
        {
            if (!WriteMessageToFile)
                return;

            chatLogger.WriteLine();
            chatLogger.Write("**** Chat cleared at: " + DateTime.UtcNow.ToLongTimeString() + " ****");
            chatLogger.WriteLine();
        }

        private void LoadSettings()
        {
            File.Delete("settings.xml"); //Delete old settings file as it is no longer used.

            RegistryKey rKey = Registry.CurrentUser.OpenSubKey("Software\\Auxilium");

            if (rKey != null)
            {
                SpaceOutMessages = (bool) rKey.GetValue("SpaceMessages", SpaceOutMessages);
                AudibleNotification = (bool) rKey.GetValue("AudioNotification", AudibleNotification);
                ShowChatNotifications = (bool) rKey.GetValue("ChatNotifications", ShowChatNotifications);
                ShowJoinLeaveEvents = (bool) rKey.GetValue("JoinLeaveEvents", ShowJoinLeaveEvents);
                MinimizeToTray = (bool) rKey.GetValue("MinimizeToTray", MinimizeToTray);
                ShowTimestamps = (bool) rKey.GetValue("Timestamps", ShowTimestamps);
                WriteMessageToFile = (bool) rKey.GetValue("WriteMessages", WriteMessageToFile);
            }
            else
            {
                SaveSettings();
            }
        }
        private void SaveSettings()
        {
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true) ??
                                   Registry.CurrentUser.CreateSubKey("Software\\Auxilium");
            if (rKey != null)
            {
                rKey.SetValue("SpaceMessages", SpaceOutMessages);
                rKey.SetValue("AudioNotification", AudibleNotification);
                rKey.SetValue("ChatNotifications", ShowChatNotifications);
                rKey.SetValue("JoinLeaveEvents", ShowJoinLeaveEvents);
                rKey.SetValue("MinimizeToTray", MinimizeToTray);
                rKey.SetValue("Timestamps", ShowTimestamps);
                rKey.SetValue("WriteMessages", WriteMessageToFile);
            }
        }

        private void SendPing()
        {
            Ping ping = new Ping();
            ping.PingCompleted += (sender, e) =>
            {
                tsmPing.Text = "Ping: " + e.Reply.RoundtripTime.ToString() + " ms";
            };
            ping.SendAsync(IPAddress.Parse(Host), null);
        }
        #endregion

        #region " UI Events "

        private void btnRegister_Click(object sender, EventArgs e)
        {
            hiddenMain.SelectedIndex = (int)MenuScreen.Register;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            hiddenMain.SelectedIndex = (int)MenuScreen.SignIn;
        }

        //TODO: Sanitize server side.
        private void button1_Click(object sender, EventArgs e)
        {
            //Disable register elements.
            ChangeRegisterState(false);

            string name = textBox2.Text.Trim();
            string pass = textBox1.Text.Trim();

            pass = Functions.SHA1(name + pass);

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
            _unreadPMs = 0;
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

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rtbChat.Copy();
        }

        private void niAux_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            niAux.Visible = false;
            niAux.Dispose();
            Environment.Exit(0);
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
            if (!e.Shift && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string message = rtbMessage.Text.Trim();

                if (!string.IsNullOrEmpty(message))
                {
                    //Send the chat message to the server.
                    byte[] data = Packer.Serialize((byte)ClientPacket.ChatMessage, message);
                    Connection.Send(data);

                    //Show message locally. May want to wait for verification from server.
                    AppendChat(Color.ForestGreen, Color.DimGray, Username, message);

                    rtbMessage.Clear();
                }
            }
        }

        private void tsmSignOut_Click(object sender, EventArgs e)
        {
            ConnectToServer();
            tsmSignOut.Enabled = false;
            tsmEditProfile.Enabled = false;
            hiddenMain.SelectedIndex = (int)MenuScreen.SignIn;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            lvUsers.Columns[0].Width = lvUsers.Width - 5;
            if (WindowState == FormWindowState.Minimized && MinimizeToTray)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (hiddenMain.SelectedIndex == (int)MenuScreen.Chat)
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

        private void tsmNews_Click(object sender, EventArgs e)
        {
            byte[] data = Packer.Serialize((byte)ClientPacket.News);
            Connection.Send(data);
        }

        private void tsmGetSource_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/BanksyHF/Auxilium");
        }

        private void tsmViewProfile_Click(object sender, EventArgs e)
        {
            if (lvUsers.SelectedItems.Count == 1)
            {
                byte[] data = Packer.Serialize((byte)ClientPacket.ViewProfile, lvUsers.SelectedItems[0].Text);
                Connection.Send(data);
            }
        }

        private void lnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            hiddenMain.SelectedIndex = (int)MenuScreen.Chat;
        }

        private void lnkHackForums_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(lnkHackForums.Tag.ToString());
        }

        private void tsmEditProfile_Click(object sender, EventArgs e)
        {
            hiddenMain.SelectedIndex = (int)MenuScreen.EditProfile;
        }

        private void lnkCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            hiddenMain.SelectedIndex = (int)MenuScreen.Chat;
        }

        private void lnkSaveProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
            string avatar = chcAvatar.Text;
            string profile = chcProfile.Text;
            if (!(avatar.Contains("http://") || avatar.Contains("https://")) && !string.IsNullOrWhiteSpace(avatar))
                avatar = avatar.Insert(0, "http://");
            if (!(profile.Contains("http://") || profile.Contains("https://")) && !string.IsNullOrWhiteSpace(avatar))
                profile = profile.Insert(0, "http://");

            byte[] data = Packer.Serialize((byte)ClientPacket.EditProfile, avatar, tbBio.Text, profile);
            Connection.Send(data);
        }

        private void chcAvatar_TextChanged(object sender, EventArgs e)
        {
            if (chcAvatar.Text == string.Empty)
                return;

            Uri yuri = null;
            string url = chcAvatar.Text;
            if (!(url.Contains("http://") || url.Contains("https://")))
                url = url.Insert(0, "http://");

            if (Uri.TryCreate(url, UriKind.Absolute, out yuri) && yuri.Scheme == "http") //Make sure url is valid url before trying to download it.
            {
                pbEditAvatar.Image = Properties.Resources.spinner;
                WebClient wc = new WebClient() { Proxy = null };
                wc.DownloadDataCompleted += (x, i) =>
                                                {
                                                    try {
                                                        pbEditAvatar.Image = ImageFromResult(i.Result);
                                                    } catch {
                                                        pbEditAvatar.Image = Properties.Resources.NA;
                                                    }
                                                };
                wc.DownloadDataAsync(yuri);
            } else {
                pbEditAvatar.Image = Properties.Resources.NA;
            }
        }

        private void tbBio_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                tbBio.SelectAll();
            }
        }

        private void tsmClearChat_Click(object sender, EventArgs e)
        {
            rtbChat.Text = string.Empty;
        }

        #endregion

        #region " Options "

        private void tsmWriteMessages_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();
            if (WriteMessageToFile)
            {
                chatLogger = new StreamWriter("chat-" + DateTime.Now.ToFileTimeUtc() + ".log");
                chatLogger.AutoFlush = true;
            }
            else
            {
                chatLogger.Close();
                chatLogger.Dispose();
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            SaveSettings();
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

            lock (PauseBuffer)
                PauseBuffer.Clear();

            LogClearEvent();

            rtbChat.Font = fd.Font;
            rtbMessage.Font = fd.Font;
        }

        private void pauseChatToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (pauseChatToolStripMenuItem.Checked)
            {
                PauseChat = true;
                pauseChatToolStripMenuItem.Text = "Unpause chat";
            }
            else
            {
                PauseChat = false;

                lock (PauseBuffer)
                {
                    foreach (ChatMessage m in PauseBuffer)
                    {
                        if (m.Color != Color.Empty)
                        {
                            AppendText(m.Color, m.Value);
                        }
                        else
                        {
                            AppendLine();
                        }
                    }

                    PauseBuffer.Clear();
                }

                ScrollChat();
                pauseChatToolStripMenuItem.Text = "Pause chat";
            }
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
            tsmEditProfile.Enabled = enable;
            tsmSignOut.Enabled = enable;
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

            lock (PauseBuffer)
                PauseBuffer.Clear();

            LogClearEvent();
        }

        private void ScrollChat()
        {
            if (PauseChat)
                return;

            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.ScrollToCaret();
        }

        private void AppendChat(Color nameColor, Color msgColor, string name, string message, DateTime time = default(DateTime))
        {
            string sender = string.Format("{0}: ", name);

            if (ShowTimestamps)
                sender = string.Format("[{0}] {1}", (time == default(DateTime) ? DateTime.Now.ToShortTimeString() : time.ToShortTimeString()), sender);

            AppendText(nameColor, sender);
            AppendText(msgColor, message);
            AppendLine();

            if (SpaceOutMessages)
                AppendLine();

            ScrollChat();
        }

        private void AppendText(Color c, string text)
        {
            if (PauseChat)
            {
                lock (PauseBuffer)
                    PauseBuffer.Add(new ChatMessage(c, text));
            }
            else
            {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = c;

                rtbChat.AppendText(text);
                rtbChat.SelectionColor = rtbChat.ForeColor;

                if (WriteMessageToFile)
                    chatLogger.Write(text);
            }       
        }

        private void AppendLine()
        {
            if (PauseChat)
            {
                lock (PauseBuffer)
                    PauseBuffer.Add(new ChatMessage(Color.Empty, string.Empty));
            }
            else
            {
                rtbChat.AppendText(Environment.NewLine);

                if (WriteMessageToFile)
                    chatLogger.WriteLine();
            }

        }

        
        #endregion

    }
}
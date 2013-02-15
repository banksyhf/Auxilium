using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading;
using MySql.Data.MySqlClient;
using Auxilium_Server.Classes;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Auxilium_Server
{
    class Program
    {
        #region " Declarations "

        static ushort Port = 3357;
        static ushort RunningID = 0;

        static string[] Channels;

        static List<ChatMessage> RecentMessages = new List<ChatMessage>();

        static string MOTD;

        static int Reconnect;

        static Pack Packer;
        static Server Listener;

        static DateTime LastBackup;

        static System.Threading.Timer ChatMonitor;

        //Done, Still Testing: Have a pool of SQL Connections to randomly select from when querying the DB.
        static MySqlConnection[] SQLQueue;
        static Random RandomSQL;

        static Random _random = new Random();

        static List<PrivateMessage> privateMessages = new List<PrivateMessage>();  

        static MySqlConnection SQL {
            get {
                return SQLQueue[RandomSQL.Next(0, SQLQueue.Length)];
            }
        }

        private const string EmailUsername = "email@website.com";
        private const string EmailPassword = "password";

        private static SmtpClient EmailClient { get; set; }
        #endregion

        #region " Initialization "

        static void Main(string[] args)
        {
            SQLQueue = new MySqlConnection[10];
            RandomSQL = new Random(new Guid().GetHashCode());

            string connectionString = "server=localhost;uid=auxilium;pwd=123456;database=auxilium";

            for (int i = 0; i < SQLQueue.Length; i++)
            {
                MySqlConnection msc = new MySqlConnection();
                SQLQueue[i] = msc;
                try
                {
                    msc.ConnectionString = connectionString;
                    msc.Open();
                }
                catch (Exception)
                {
                    Console.Write("An error occured connecting to the database.\nPlease enter your credentials.\n\nUsername: ");

                    string username = Console.ReadLine();
                    Console.Write("Password: ");

                    string password = Console.ReadLine();
                    msc.ConnectionString = (connectionString = string.Format("server=localhost;uid={0};pwd={1};database=auxilium", username, password));
                    msc.Open();
                }
            }

            EmailClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            LetsDoThis();

            Channels = new string[] { "Lounge" };

            MOTD = GetMOTD();

            Packer = new Pack();
            Listener = new Server {BufferSize = 2048, MaxConnections = 1000};

            Listener.ClientRead += ClientRead;
            Listener.ClientState += ClientState;
            Listener.ServerState += ServerState;
            Listener.Listen(Port);

            //Create the chat monitor timer.
            ChatMonitor = new Timer(Monitor, null, 1000, 240000); //240,000 = 4 Minutes
            LastBackup = DateTime.Now;

            string console = string.Empty;

            while (true)
            {
                char c = Console.ReadKey().KeyChar;
                if (c == Convert.ToChar(ConsoleKey.Escape)) {
                    break;
                } if (c == Convert.ToChar(ConsoleKey.Enter)) {
                    CheckCommand(null, console);
                    console = string.Empty;
                } else {
                    console += c;
                }
            }
        }

        static void LetsDoThis()
        {
            //Console.WriteLine(await SendEmail("xbanksyxhf@gmail.com", "Banksy"));
        }

        #endregion

        #region " Monitor "

        static void Monitor(object state)
        {
            if (!Listener.Listening)
                return;

            UpdateAndSaveUsers(false);
        }

        static void AwardPoints(Client c)
        {
            if ((DateTime.Now - c.Value.LastAction).TotalMinutes >= 3)
            {
                c.Value.Idle = true;
            }

            //2812000 at 4.65 points per second should require about 1 week of active time to max rank.
            double Points = (DateTime.Now - c.Value.LastPayout).TotalSeconds * 4.65;
            c.Value.LastPayout = DateTime.Now;

            if (c.Value.Idle)
            {
                if (c.Value.Rank < 29)
                    c.Value.AddPoints((int)(Points * 1.0));
            }
            else
            {
                c.Value.AddPoints((int)Points);
            }
        }

        static void UpdateAndSaveUsers(bool shutDown)
        {
            bool doBackup = false;
            if ((DateTime.Now - LastBackup).TotalMinutes >= 20)
            {
                doBackup = true;
                LastBackup = DateTime.Now;
            }

            foreach (Client c in Listener.Clients)
            {
                AwardPoints(c);

                if (!shutDown)
                {
                    FullUserListUpdate();
                    SendProfile(c);
                }

                if (doBackup || shutDown)
                {

                    MySqlCommand q = new MySqlCommand(string.Empty, SQL);
                    q.CommandText = "UPDATE users SET Points=@Points,Rank=@Rank,Mute=@Mute WHERE Username=@Username;";
                    q.Parameters.AddWithValue("@Points", c.Value.Points);
                    q.Parameters.AddWithValue("@Rank", c.Value.Rank);
                    q.Parameters.AddWithValue("@Mute", c.Value.Mute);
                    q.Parameters.AddWithValue("@Username", c.Value.Username);
                    q.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region " Send Methods "

        static void Broadcast(byte channel, byte[] data)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Verified && c.Value.Channel == channel)
                {
                    c.Send(data);
                }
            }
        }

        static void BroadcastExclusive(ushort userID, byte channel, byte[] data, Client client = null)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Verified && c.Value.Channel == channel && c.Value.UserId != userID)
                {
                    if (client != null && !client.Value.Mute.Contains(c) && !c.Value.Mute.Contains(client))
                        c.Send(data);
                }
            }
        }

        static void GlobalBroadcast(byte[] data)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Verified)
                    c.Send(data);
            }
        }

        #endregion

        #region " Socket Events "

        static void ServerState(Server s, bool open)
        {
            if (open)
            {
                Reconnect = 0;
                Console.WriteLine("Server listening.");
            }
            else
            {
                //TODO: Clean up server state.

                if (Reconnect == 4) //Try reconnecting 3 times then exit.
                    Environment.Exit(0);

                Console.WriteLine("Server disconnected. Reconnecting in 20 seconds.");
                Thread.Sleep(20000);

                Reconnect += 1;
                Listener.Listen(Port);
            }
        }

        static void ClientState(Server s, Client c, bool open)
        {
            if (open)
            {
                c.Value = new UserState();
                if (!string.IsNullOrEmpty(MOTD))
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.MOTD, MOTD);
                    c.Send(data);
                }
            }
            else
            {
                if (c.Value.Authenticated)
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserId);
                    Broadcast(c.Value.Channel, data);

                    AwardPoints(c);

                    //Let's save the users data.
                    MySqlCommand q = new MySqlCommand(string.Empty, SQL);
                    q.CommandText = "UPDATE users SET Points=@Points,Rank=@Rank,Mute=@Mute WHERE Username=@Username;";
                    q.Parameters.AddWithValue("@Points", c.Value.Points);
                    q.Parameters.AddWithValue("@Rank", c.Value.Rank);
                    q.Parameters.AddWithValue("@Mute", c.Value.Mute);
                    q.Parameters.AddWithValue("@Username", c.Value.Username);

                    //If it fails there isn't much we can do about it.
                    q.ExecuteNonQuery();
                }
            }
        }

        static void ClientRead(Server s, Client c, byte[] e)
        {
            try
            {
                //Anti-flood measures.
                if (c.Value.IsFlooding())
                {
                    c.Disconnect();
                    return;
                }

                object[] values = Packer.Deserialize(e);
                if (values == null)
                    return;

                ClientPacket packet = (ClientPacket)values[0];

                if (c.Value.Authenticated && c.Value.Verified)
                {
                    switch (packet)
                    {
                        case ClientPacket.Channel:
                            HandleWakeup(c, true); //Supress the packet send here since we are changing rooms anyways.
                            HandleChannelPacket(c, (byte)values[1]);
                            break;
                        case ClientPacket.ChatMessage:
                            HandleWakeup(c);
                            HandleChatPacket(c, (string)values[1]);
                            break;
                        case ClientPacket.PM:
                            HandleWakeup(c);
                            HandlePMPacket(c, (string)values[1], (string)values[2], (string)values[3], values.Length == 5 ? (ushort)values[4] : default(ushort));
                            break;
                        case ClientPacket.KeepAlive:
                            HandleKeepAlivePacket(c);
                            break;
                        case ClientPacket.News:
                            HandleNewsPacket(c);
                            break;
                        case ClientPacket.ViewProfile:
                            HandleViewProfilePacket(c, (string)values[1]);
                            break;
                        case ClientPacket.EditProfile:
                            HandleEditProfilePacket(c, (string)values[1], (string)values[2], (string)values[3]);
                            break;
                        case ClientPacket.AuthCode:
                            HandleAuthCodePacket(c, (string) values[1]);
                            break;
                    }
                }
                else
                {
                    switch (packet)
                    {
                        case ClientPacket.SignIn:
                            HandleSignInPacket(c, (string)values[1], (string)values[2]);
                            break;
                        case ClientPacket.Register:
                            HandleRegisterPacket(c, (string)values[1], (string)values[2], (string)values[3]);
                            break;
                        case ClientPacket.ResendVerification:
                            HandleResendVerificationPacket(c);
                            break;
                        case ClientPacket.AuthCode:
                            HandleAuthCodePacket(c, (string) values[1]);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                c.Disconnect();
            }
        }

        #endregion

        #region " Packet Handlers "

        static void HandleAuthCodePacket(Client c, string authCode)
        {
            MySqlCommand q = new MySqlCommand("SELECT AuthType, Value FROM authcodes WHERE (User=@Username) AND Value=@Value;", SQL);
            q.Parameters.AddWithValue("@Username", c.Value.Username);
            q.Parameters.AddWithValue("@Value", authCode);

            MySqlDataReader r = q.ExecuteReader();
            bool read = r.Read() && r.HasRows;

            if (read)
            {
                AuthType authType = (AuthType) r.GetByte("AuthType");

                r.Close();

                q.CommandText = "DELETE FROM `authcodes` WHERE `Value`=@Value AND `User`=@Username LIMIT 1;";

                bool success = q.ExecuteNonQuery() != 0;

                byte[] data = Packer.Serialize((byte) ServerPacket.AuthResponse,(byte)authType, success);
                c.Send(data);
            }
            else
            {
                byte[] data = Packer.Serialize((byte) ServerPacket.AuthResponse, (byte)AuthType.Unknown, false);
                c.Send(data);
            }

        }

        //TODO: Don't disconnect people, instead return an error code.
        static void HandleSignInPacket(Client c, string name, string pass)
        {
            string n = name.Trim();

            if (n.Length == 0 || n.Length > 16 || pass.Length != 40 || !IsValidName(n))
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.SignIn, false, false);
                c.Send(fail);
                return;
            }


            MySqlCommand q = new MySqlCommand("SELECT Points, Username, Rank, Ban, Mute, Email FROM users WHERE (Username=@Username OR Email=@Username) AND Password=@Password;", SQL);
            q.Parameters.AddWithValue("@Username", n);
            q.Parameters.AddWithValue("@Password", pass);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read();

            if (success)
            {
                int points = r.GetInt32("Points");
                byte rank = r.GetByte("Rank");
                bool ban = r.GetBoolean("Ban");
                string email = r.GetString("Email");
                string username = r.GetString("Username");

                r.Close(); //TODO: Restructure this.

                r.Dispose();

                if (ban)
                {
                    c.Disconnect();
                    return;
                }

                //Second ban check, checks ip table.
                q = new MySqlCommand(string.Empty, SQL) {CommandText = "SELECT * FROM ipbans WHERE ip=@ip;"};
                q.Parameters.AddWithValue("@ip", c.EndPoint.Address.ToString());
                r = q.ExecuteReader();
                bool sCheck = r.Read();

                r.Close();

                if (sCheck)
                {
                    c.Disconnect();
                    return;
                }

                q = new MySqlCommand("SELECT * FROM authcodes WHERE User=@User;", SQL);
                q.Parameters.AddWithValue("@User", username);
                r = q.ExecuteReader();
                //Pending email verification
                bool unverified = r.Read() && r.GetByte("AuthType") == (byte)AuthType.AccountVerification;

                r.Close();

                if (unverified)
                {
                    byte[] notVerified = Packer.Serialize((byte)ServerPacket.NotVerified);
                    c.Send(notVerified);
                }

                byte[] data = Packer.Serialize((byte)ServerPacket.SignIn, true, !unverified);
                c.Send(data);


                //If this user is already logged in from somewhere else then disconnect them.
                Client existing = ClientFromUsername(n);
                if (existing != null && existing != c)
                {
                    existing.Disconnect();
                }

                c.Value.UserId = RunningID++;
                c.Value.Username = username;

                c.Value.Points = points;
                c.Value.Rank = rank;

                c.Value.Mute = new List<Client>();

                c.Value.LastPayout = DateTime.Now;
                c.Value.LastAction = DateTime.Now;
                c.Value.Email = email;

                c.Value.Authenticated = true;
                c.Value.Verified = !unverified;

                if (!unverified)
                {
                    SendProfile(c);
                    SendLoginBarrage(c);
                }
            }
            else
            {
                r.Close();//TODO: Restructure this.
                byte[] data1 = Packer.Serialize((byte)ServerPacket.SignIn, false, false);
                c.Send(data1);
            }
        }

        static void HandleRegisterPacket(Client c, string name, string pass, string email)
        {
            string n = name.Trim();

            if (n.Length == 0 || n.Length > 16 || pass.Length != 40 || !IsValidName(n) || email.Length == 0 || email.Length > 256)
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.Register, false);
                c.Send(fail);
                return;
            }

            MySqlCommand q = new MySqlCommand("SELECT Count(*) FROM users WHERE Username=@Username", SQL);
            q.Parameters.AddWithValue("@Username", n);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read() && (r.GetInt16(0) == 0);
            r.Close();

            if (available)
            {
                MySqlCommand q2 = new MySqlCommand(string.Empty, SQL);
                q2.CommandText = "INSERT INTO users VALUES (@Username,@Password,@Email,0,0,0,0,\"\",\"\",\"\");";
                q2.Parameters.AddWithValue("@Username", n);
                q2.Parameters.AddWithValue("@Password", pass);
                q2.Parameters.AddWithValue("@Email", email);

                //If registration fails, this will return false.
                bool success = (q2.ExecuteNonQuery() != 0);

                if (success)
                {
                    c.Value.Username = name;
                    c.Value.Email = email;
                    SendEmail(email, n);
                }

                byte[] data = Packer.Serialize((byte)ServerPacket.Register, success);
                c.Send(data);
            }
            else
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.Register, false);
                c.Send(data);
            }

        }

        static void HandleResendVerificationPacket(Client c)
        {
            MySqlCommand q = new MySqlCommand("SELECT * FROM authcodes WHERE User=@User AND AuthType=1", SQL);
            q.Parameters.AddWithValue("@User", c.Value.Username);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read();

            if (!available) return;

            string authCode = r.GetString("Value");

            SendEmail(c.Value.Email, c.Value.Username, authCode);
        }

        static void HandleChannelPacket(Client c, byte channel)
        {
            if (channel < Channels.Length)
            {
                //Let everyone in the old channel know this guy left.
                byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserId);
                Broadcast(c.Value.Channel, data);

                c.Value.Channel = channel;
                SendUserListUpdates(c);
            }
            else
            {
                c.Disconnect();
                return;
            }
        }

        static void HandlePMPacket(Client c, string recipient, string message, string subject, ushort id = default(ushort))
        {
            if (!(IsValidName(recipient) && IsValidData(message) && IsValidData(subject)))
            {
                c.Disconnect();
                return;
            }

            Client u = ClientFromUsername(recipient);
            if (u == null)
                return;

            PrivateMessage pm = null;

            if (id == default(ushort) && !privateMessages.Exists(x => x.Id == id))
            {
                pm = new PrivateMessage(id = (ushort)_random.Next(ushort.MinValue, ushort.MaxValue), subject, c.Value.Username, DateTime.Now, message);
                privateMessages.Add(pm);

                byte[] confirm = Packer.Serialize((byte)ServerPacket.PMConfirm, id, u.Value.Username, message, subject, DateTime.Now);
                c.Send(confirm);
            }
            else
            {
                pm = privateMessages.Find(x => x.Id == id);
                pm.Message.Add(message);
            }

            byte[] data = Packer.Serialize((byte)ServerPacket.PM, id, c.Value.Username, u.Value.Username, message, subject, DateTime.Now);
            u.Send(data);

        }

        static void HandleChatPacket(Client c, string message)
        {
            if (!IsValidData(message))
            {
                c.Disconnect();
                return;
            }

            if (CheckCommand(c, message))
            {
                //byte[] confirmCommand = Packer.Serialize((byte)ServerPacket.)
                return;
            }

            c.Value.AddPoints(5); //AWARD 5 POINTS FOR ACTIVITY***

            if (RecentMessages.Count == 10)
                RecentMessages.RemoveAt(0);


            ChatMessage msg = new ChatMessage(DateTime.Now.ToLocalTime().ToString(), message, c.Value.Username, c.Value.Rank);
            RecentMessages.Add(msg);

            byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserId, message);
            BroadcastExclusive(c.Value.UserId, c.Value.Channel, data, c);
        }

        static void HandleKeepAlivePacket(Client c)
        {
            byte[] data = Packer.Serialize((byte)ServerPacket.KeepAlive);
            c.Send(data);
        }

        static void HandleNewsPacket(Client c)
        {
            byte[] data = Packer.Serialize((byte)ServerPacket.News, GetNews());
            c.Send(data);
        }

        static void HandleEditProfilePacket(Client c, string avatar, string bio, string profile)
        {
            if (!(avatar.Contains("http://") || avatar.Contains("https://")))
                avatar = avatar.Insert(0, "http://");
            if (!(profile.Contains("http://") || profile.Contains("https://")))
                profile = profile.Insert(0, "http://");

            bool validProfile = Regex.IsMatch(profile, @"^(http:\/\/|https:\/\/)*(www\.)*hackforums\.net\/member\.php\?action=profile&uid=\d{1,7}$");

            MySqlCommand q = new MySqlCommand("UPDATE users SET Avatar=@Avatar, Bio=@Bio, ProfileLink=@ProfileLink WHERE Username=@Username;", SQL);
            q.Parameters.AddWithValue("@Username", c.Value.Username);
            q.Parameters.AddWithValue("@Bio", bio);
            q.Parameters.AddWithValue("@Avatar", avatar);
            q.Parameters.AddWithValue("@ProfileLink", validProfile ? profile : "");

            bool success = q.ExecuteNonQuery() == 0;

            byte[] data = Packer.Serialize((byte)ServerPacket.EditProfile, success);
            c.Send(data);
        }

        static void HandleViewProfilePacket(Client c, string username)
        {
            Client to = ClientFromUsername(username);
            if (to != null)
                SendProfileTo(c, to);
        }

        #endregion

        #region " Helper Methods "

        static bool SendEmail(string email, string username, string authCode = "", byte authType = 1)
        {
            try
            {
                bool isNull = string.IsNullOrWhiteSpace(authCode);
                if (isNull)
                    authCode = GenerateAuthCode(email);

                MySqlCommand q = new MySqlCommand("SELECT Count(*) FROM authcodes WHERE User=@User AND AuthType=" + authType, SQL);
                q.Parameters.AddWithValue("@User", username);

                MySqlDataReader r = q.ExecuteReader();
                bool noAuth = r.Read() && r.GetInt16(0) == 0;
                r.Close();

                if (noAuth && isNull)
                {
                    q.CommandText = "INSERT INTO authcodes VALUES (@AuthType,@Value,@User);";
                    q.Parameters.AddWithValue("@AuthType", authType);
                    q.Parameters.AddWithValue("@Value", authCode);
                    q.ExecuteNonQuery();
                }

                MailAddress from = new MailAddress(EmailUsername, "Auxilium");

                MailMessage mail = new MailMessage
                {
                    From = from,
                    Subject = "Registration code for Auxilium",
                    Body = "Your registration code for auxilium is: " + authCode
                };
                mail.To.Add(email);

                EmailClient.Credentials = new NetworkCredential(from.Address, EmailPassword);
                EmailClient.SendAsync(mail, null);

                return true;
            } catch {
                return false;
            }
        }

        private static string GenerateAuthCode(string text)
        {
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider(text);
            byte[] auth = new byte[32];
            crypto.GetNonZeroBytes(auth);
            return auth.GetHashCode().ToString();
        }

        static void HandleWakeup(Client c, bool suppress = false)
        {
            c.Value.LastAction = DateTime.Now;

            if (!c.Value.Idle) return;

            c.Value.Idle = false;

            if (suppress) return;

            byte[] data = Packer.Serialize((byte)ServerPacket.WakeUp, c.Value.UserId);
            Broadcast(c.Value.Channel, data);
        }

        static void SendLoginBarrage(Client c)
        {
            SendChannelList(c);
            SendUserListUpdates(c);
            foreach (ChatMessage cm in RecentMessages)
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.RecentMessages, cm.Time, cm.Username, cm.Value, cm.Rank);
                c.Send(data);
            }
        }

        static string GetMOTD()
        {
            string motd = string.Empty;

            MySqlCommand q = new MySqlCommand("SELECT motd FROM settings;", SQL);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read();
            if (available)
                motd = r.GetString("motd").Replace("\n", Environment.NewLine); ;
            r.Close();

            return motd;
        }

        static string GetNews()
        {
            string news = string.Empty;

            MySqlCommand q = new MySqlCommand("SELECT news FROM settings;", SQL);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read();
            if (available)
                news = r.GetString("news").Replace("\n", Environment.NewLine);
            r.Close();

            return news;
        }

        static void SendUserListUpdates(Client c)
        {
            //Let everyone know the life of the party has just arrived.
            byte[] data1 = Packer.Serialize((byte)ServerPacket.UserJoin, c.Value.UserId, c.Value.Username, c.Value.Rank);
            BroadcastExclusive(c.Value.UserId, c.Value.Channel, data1, c);

            //Our guy will probably need to know who he's chatting with, right?
            List<object> cValues = new List<object> {(byte) ServerPacket.UserList};

            foreach (Client t in Listener.Clients)
            {
                if (t.Value.Authenticated && t.Value.Channel == c.Value.Channel)
                {
                    cValues.AddRange(new object[] { t.Value.UserId, t.Value.Username, t.Value.Rank, t.Value.Idle });
                }
            }

            byte[] data2 = Packer.Serialize(cValues.ToArray());
            c.Send(data2);
        }

        static void SendProfile(Client c)
        {
            if (c.Value.Authenticated)
            {
                byte[] profile = Packer.Serialize(GetProfile(ServerPacket.Profile, c.Value.Username));
                c.Send(profile);
            }
        }

        static void SendProfileTo(Client fromClient, Client toClient)
        {
            if (fromClient.Value.Authenticated)
            {
                byte[] profile = Packer.Serialize(GetProfile(ServerPacket.ViewProfile, toClient.Value.Username));
                fromClient.Send(profile);
            }
        }

        static void FullUserListUpdate()
        {
            for (int i = 0; i < Channels.Length; i++)
            {
                List<object> values = new List<object> {(byte) ServerPacket.UserList};

                //It would make more sense to loop through the connections only once and build up a few arrays to send out at the end.
                foreach (Client c in Listener.Clients)
                {
                    if (c.Value.Authenticated && c.Value.Channel == i)
                    {
                        values.AddRange(new object[] { c.Value.UserId, c.Value.Username, c.Value.Rank, c.Value.Idle });
                    }
                }

                byte[] data = Packer.Serialize(values.ToArray());
                Broadcast((byte)i, data);
            }
        }

        static void SendChannelList(Client c)
        {
            List<object> values = new List<object>();
            values.Add((byte)ServerPacket.ChannelList);
            values.AddRange(Channels);

            byte[] data = Packer.Serialize(values.ToArray());
            c.Send(data);
        }

        static Client ClientFromUsername(string name)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Username.ToLower() == name.ToLower())
                    return c;
            }

            return null;
        }

        static bool IsValidName(string name)
        {
            string n = name.ToUpper();

            for (int i = 0; i < name.Length; i++)
            {
                if (!(n[i] == 45 || n[i] == 95 || char.IsNumber(n[i]) || n[i] > 64 && n[i] < 91))
                    return false;
            }

            return true;
        }

        static bool IsValidData(string data)
        {
            //Removed for now because of complications with the euro and pound signs. TODO: properly map these (and other) characters.

            /*for (int i = 0; i < data.Length; i++)
            {
                if (!(data[i] == 10 || data[i] == 13 || data[i] > 31 && data[i] < 127))
                    return false;
            }*/

            return true;
        }

        static object[] GetProfile(ServerPacket header, string username)
        {
            List<object> profile = new List<object> {(byte) header};

            MySqlCommand q = new MySqlCommand("SELECT * FROM users WHERE Username=@Username;", SQL);
            q.Parameters.AddWithValue("@Username", username);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read();

            if (success)
            {

                string link = r.GetString("ProfileLink");
                int points = r.GetInt32("Points");
                byte rank = r.GetByte("Rank");
                string bio = r.GetString("Bio");
                string avatar = r.GetString("Avatar");

                UserState state = new UserState()
                {
                    Points = points,
                    Rank = rank
                };

                state.AddPoints(0);

                profile.AddRange(new object[] { username, link, rank, bio, avatar, state.Percentage });

                r.Close();
                r.Dispose();
            }
            else
            {
                profile.AddRange(new object[] { username, "", 0, "", "", 0 });
                r.Close();
                r.Dispose();
            }

            return profile.ToArray();
        }

        #endregion

        #region " Commands "

        static bool KickUser(string name)
        {
            Client c = ClientFromUsername(name);

            if (c == null) return false;

            c.Disconnect();
            Console.WriteLine(name + " has been kicked.");
            return true;
        }

        static bool BanUser(string name, int ipBan = 0)
        {
            try
            {
                MySqlCommand q = new MySqlCommand("UPDATE users SET Ban=1 WHERE Username=@Username;", SQL);
                q.Parameters.AddWithValue("@Username", name);
                q.ExecuteNonQuery();

                Client c = ClientFromUsername(name);
                if (ipBan == 1)
                {

                    MySqlCommand q2 = new MySqlCommand(string.Empty, SQL);
                    q2.CommandText = "INSERT INTO ipbans VALUES (@ip);";
                    q2.Parameters.AddWithValue("@ip", c.EndPoint.Address.ToString());
                    q2.ExecuteNonQuery();
                }

                if (c != null)
                    c.Disconnect();

                Console.WriteLine(name + " has been dealt with.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static bool UnbanUser(string name)
        {
            try
            {
                MySqlCommand q = new MySqlCommand("UPDATE users SET Ban=0 WHERE Username=@Username;", SQL);
                q.Parameters.AddWithValue("@Username", name);
                if (q.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine(name + " has been unbanned.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        static void MuteUser(Client c, string name, bool global)
        {
            Client muted = ClientFromUsername(name);

            if (muted != null)
            {
                if (global)
                {
                    foreach (Client x in Listener.Clients)
                    {
                        if (x != muted)
                            x.Value.Mute.Add(muted);
                        return;
                    }
                }

                c.Value.Mute.Add(muted);
                Console.WriteLine(name + " has been muted by " + c.Value.Username);
            }
        }

        static void UnmuteUser(Client c, string name, bool global)
        {
            Client muted = ClientFromUsername(name);

            if (muted != null)
            {
                if (global)
                {
                    foreach (Client x in Listener.Clients)
                    {
                        if (x != muted)
                            x.Value.Mute.Remove(muted);
                        return;
                    }
                }

                c.Value.Mute.Remove(muted);
            }

            Console.WriteLine(name + " has been unmuted.");
        }

        static bool SetUserRank(string name, string rank)
        {
            try
            {
                MySqlCommand q = new MySqlCommand("UPDATE users SET Rank=@Rank WHERE Username=@Username;", SQL);
                q.Parameters.AddWithValue("@Rank", byte.Parse(rank));
                q.Parameters.AddWithValue("@Username", name);
                q.ExecuteNonQuery();

                //Quick and easy.
                Client c = ClientFromUsername(name);
                if (c != null)
                    c.Disconnect();

                Console.WriteLine(name + " has been set to: " + rank);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void ClearChat()
        {
            byte[] data = Packer.Serialize((byte)ServerPacket.ClearChat);
            foreach(Client c in Listener.Clients)
            {
                c.Send(data);
            }
        }

        static readonly string[] UserCommands = new[] { "mute", "unmute" };
        static readonly string[] AdminCommands = new[] { "globalmute", "kick", "ban", "unban", "globalmsg", "setlevel", "updatemotd", "shutdown", "cls" };
        static bool CheckCommand(Client c, string query)
        {
            string[] args = ParseArguments(query);
            string command = args[0].ToLower();
            if (command[0] == '/')
            {
                command = command.Substring(1, command.Length - 1);
                if (AdminCommands.Contains(command) && (c == null /*This is only null if typed directly into the console.*/ || c.Value.Rank == (byte)SpecialRank.Admin))
                {
                    //TODO: Find a better way to do this.
                    switch (command)
                    {
                        case "kick":
                            if (args.Length == 2)
                            {
                                if (KickUser(args[1]))
                                    return true;
                            }
                            break;
                        case "ban":
                            if (BanUser(args[1], args.Length == 3 && args[2] == 1.ToString() ? 1 : 0))
                                return true;
                            break;
                        case "unban":
                            if (args.Length == 2)
                            {
                                if (UnbanUser(args[1]))
                                    return true;
                            }
                            break;
                        case "globalmsg":
                            if (args.Length == 2)
                            {
                                byte[] data = Packer.Serialize((byte)ServerPacket.GlobalMsg, args[1]);
                                GlobalBroadcast(data);
                                return true;
                            }
                            break;
                        case "setlevel":
                            if (args.Length == 3)
                            {
                                if (SetUserRank(args[1], args[2]))
                                    return true;
                            }
                            break;
                        case "globalmute":
                            if (args.Length == 2 && args[1] != c.Value.Username)
                            {
                                MuteUser(c, args[1], true);
                                return true;
                            }
                            break;
                        case "globalunmute":
                            if (args.Length == 2 && args[1] != c.Value.Username)
                            {
                                UnmuteUser(c, args[1], true);
                                return true;
                            }
                            break;
                        case "updatemotd":
                            MOTD = GetMOTD();
                            return true;
                        case "shutdown":
                            UpdateAndSaveUsers(true);
                            Environment.Exit(0);
                            break;
                        case "cls":
                            ClearChat();
                            break;
                    }
                }

                if (UserCommands.Contains(command))
                {
                    switch (command)
                    {
                        case "mute":
                            if (args.Length == 2 && args[1] != c.Value.Username)
                            {
                                MuteUser(c, args[1], false);
                                return true;
                            }
                            break;
                        case "unmute":
                            if (args.Length == 2 && args[1] != c.Value.Username)
                            {
                                UnmuteUser(c, args[1], false);
                                return true;
                            }
                            break;
                    }
                }
            }
            return false;
        }
        static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars).Replace("\"", "")).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region " Custom Types "

        public enum SpecialRank : byte
        {
            Admin = 41
        }

        public enum ServerPacket : byte
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
            BanList,
            PM,
            PMConfirm,
            KeepAlive,
            WakeUp,
            RecentMessages,
            News,
            ViewProfile,
            Profile,
            EditProfile,
            ClearChat,
            NotVerified,
            AuthResponse
        }

        public enum ClientPacket : byte
        {
            SignIn,
            Register,
            Channel,
            ChatMessage,
            PM,
            KeepAlive,
            News,
            ViewProfile,
            EditProfile,
            ResendVerification,
            AuthCode
        }

        public enum AuthType : byte
        {
            Unknown,
            AccountVerification
        }

        class ChatMessage
        {
            public readonly string Time;
            public readonly string Username;
            public readonly string Value;
            public readonly byte Rank;

            public ChatMessage(string time, string value, string username, byte rank)
            {
                Time = time;
                Value = value;
                Username = username;
                Rank = rank;
            }
        }

        #endregion
    }
}

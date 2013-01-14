using System;
using System.Linq;
using System.Threading;
using Auxilium_Server.Classes.Connection;
using MySql.Data.MySqlClient;
using Auxilium_Server.Classes;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Auxilium_Server
{
    class Program
    {
        #region " Declarations "

        private const ushort Port = 3357;
        static ushort RunningId = 0;

        static string[] Channels;

        static List<ChatMessage> RecentMessages = new List<ChatMessage>();

        static string Motd;

        static int Reconnect;

        static Pack Packer;
        static Server Listener;

        static DateTime LastBackup;

        static Timer ChatMonitor;

        //Done, Still Testing: Have a pool of SQL Connections to randomly select from when querying the DB.
        static MySqlConnection[] _sqlQueue;
        static Random _randomSql;

        static MySqlConnection Sql
        {
            get
            {
                return _sqlQueue[_randomSql.Next(0, _sqlQueue.Length)];
            }
        }

        #endregion

        #region " Initialization "

        static void Main()
        {
            _sqlQueue = new MySqlConnection[10];
            _randomSql = new Random(new Guid().GetHashCode());

            string connectionString = "server=localhost;uid=auxilium;pwd=123456;database=auxilium";
            for (int i = 0; i < _sqlQueue.Length; i++)
            {
                MySqlConnection msc = new MySqlConnection();
                _sqlQueue[i] = msc;
                try
                {
                    msc.ConnectionString = connectionString;
                    msc.Open();
                }
                catch (MySqlException)
                {
                    Console.Write("An error occured connecting to the database.\nPlease enter your credentials.\n\nUsername: ");

                    string username = Console.ReadLine();
                    Console.Write("Password: ");

                    string password = Console.ReadLine();
                    msc.ConnectionString = (connectionString = string.Format("server=localhost;uid={0};pwd={1};database=auxilium", username, password));
                    msc.Open();
                }
            }

            Channels = new string[] { "Lounge" };

            Motd = GetMOTD();

            Packer = new Pack();
            Listener = new Server();

            Listener.BufferSize = 2048;
            Listener.Client_Read += Client_Read;
            Listener.MaxConnections = 200;
            Listener.Client_State += Client_State;
            Listener.Server_State += Server_State;
            Listener.Listen(Port);

            //Create the chat monitor timer.
            ChatMonitor = new Timer(Monitor, null, 1000, 240000); //240,000 = 4 Minutes
            LastBackup = DateTime.Now;

            while (true)
            {
                string str = string.Empty;
                if (!string.IsNullOrWhiteSpace(str = Console.ReadLine()))
                    CheckCommand(null, str);
                else if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
            }
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

                    MySqlCommand q = new MySqlCommand(string.Empty, Sql);
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
                if (c.Value.Authenticated && c.Value.Channel == channel)
                {
                    c.Send(data);
                }
            }
        }

        static void BroadcastExclusive(ushort userID, byte channel, byte[] data, Client client = null)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Channel == channel && c.Value.UserID != userID)
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
                if (c.Value.Authenticated)
                    c.Send(data);
            }
        }

        #endregion

        #region " Socket Events "

        static void Server_State(Server s, bool open)
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
                System.Threading.Thread.Sleep(20000);

                Reconnect += 1;
                Listener.Listen(Port);
            }
        }

        static void Client_State(Server s, Client c, bool open)
        {
            if (open)
            {
                c.Value = new UserState();
                if (!string.IsNullOrEmpty(Motd))
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.Motd, Motd);
                    c.Send(data);
                }
            }
            else
            {
                if (c.Value.Authenticated)
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserID);
                    Broadcast(c.Value.Channel, data);

                    AwardPoints(c);

                    //Let's save the users data.
                    MySqlCommand q = new MySqlCommand(string.Empty, Sql);
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

        static void Client_Read(Server s, Client c, byte[] e)
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

                if (c.Value.Authenticated)
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
                            HandlePMPacket(c, (string)values[1], (string)values[2], (string)values[3]);
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
                            HandleRegisterPacket(c, (string)values[1], (string)values[2]);
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

        //TODO: Don't disconnect people, instead return an error code.
        static void HandleSignInPacket(Client c, string name, string pass)
        {
            string n = name.Trim();

            if (n.Length == 0 || n.Length > 16 || pass.Length != 40 || !IsValidName(n))
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.SignIn, false);
                c.Send(fail);
                return;
            }


            MySqlCommand q = new MySqlCommand("SELECT Points, Rank, Ban, Mute FROM users WHERE Username=@Username AND Password=@Password;", Sql);
            q.Parameters.AddWithValue("@Username", n);
            q.Parameters.AddWithValue("@Password", pass);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read();

            if (success)
            {
                int points = r.GetInt32("Points");
                byte rank = r.GetByte("Rank");
                bool ban = r.GetBoolean("Ban");
                bool mute = r.GetBoolean("Mute");
                r.Close();//TODO: Restructure this.

                r.Dispose();

                if (ban)
                {
                    c.Disconnect();
                    return;
                }

                //Second ban check, checks ip table.
                q = new MySqlCommand(string.Empty, Sql);
                q.CommandText = "SELECT * FROM ipbans WHERE ip=@ip;";
                q.Parameters.AddWithValue("@ip", c.EndPoint.Address.ToString());
                r = q.ExecuteReader();
                bool sCheck = r.Read();

                r.Close();

                if (sCheck)
                {
                    c.Disconnect();
                    return;
                }

                byte[] data = Packer.Serialize((byte)ServerPacket.SignIn, true);
                c.Send(data);


                //If this user is already logged in from somewhere else then disconnect them.
                Client existing = ClientFromUsername(n);
                if (existing != null)
                {
                    existing.Disconnect();
                }

                c.Value.UserID = RunningId++;
                c.Value.Username = n;

                c.Value.Points = points;
                c.Value.Rank = rank;

                c.Value.Mute = new List<Client>();

                c.Value.LastPayout = DateTime.Now;
                c.Value.LastAction = DateTime.Now;

                c.Value.Authenticated = true;
                SendProfile(c);
                SendLoginBarrage(c);
            }
            else
            {
                r.Close();//TODO: Restructure this.
                byte[] data1 = Packer.Serialize((byte)ServerPacket.SignIn, false);
                c.Send(data1);
            }
        }

        static void HandleRegisterPacket(Client c, string name, string pass)
        {
            string n = name.Trim();

            if (n.Length == 0 || n.Length > 16 || pass.Length != 40 || !IsValidName(n))
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.Register, false);
                c.Send(fail);
                return;
            }

            MySqlCommand q = new MySqlCommand("SELECT Count(*) FROM users WHERE Username=@Username", Sql);
            q.Parameters.AddWithValue("@Username", n);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read() && (r.GetInt16(0) == 0);
            r.Close();

            if (available)
            {
                MySqlCommand q2 = new MySqlCommand(string.Empty, Sql);
                q2.CommandText = "INSERT INTO users VALUES (@Username,@Password,0,0,0,0,\"\",\"\",\"\");";
                q2.Parameters.AddWithValue("@Username", n);
                q2.Parameters.AddWithValue("@Password", pass);

                //If registration fails, this will return false.
                bool success = (q2.ExecuteNonQuery() != 0);

                byte[] data = Packer.Serialize((byte)ServerPacket.Register, success);
                c.Send(data);
            }
            else
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.Register, false);
                c.Send(data);
            }

        }

        static void HandleChannelPacket(Client c, byte channel)
        {
            if (channel < Channels.Length)
            {
                //Let everyone in the old channel know this guy left.
                byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserID);
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

        static void HandlePMPacket(Client c, string username, string message, string subject)
        {
            if (!(IsValidName(username) && IsValidData(message) && IsValidData(subject)))
            {
                c.Disconnect();
                return;
            }

            Client u = ClientFromUsername(username);
            if (u == null)
                return;

            byte[] data = Packer.Serialize((byte)ServerPacket.Pm, c.Value.Username, message, subject);
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
                return;

            c.Value.AddPoints(5); //AWARD 5 POINTS FOR ACTIVITY***

            if (RecentMessages.Count == 10)
                RecentMessages.RemoveAt(0);

            DateTimeOffset dt = DateTimeOffset.Now;

            dt.ToLocalTime().ToString();

            ChatMessage msg = new ChatMessage(dt.ToLocalTime().ToString(), message, c.Value.Username, c.Value.Rank);
            RecentMessages.Add(msg);

            byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserID, message);
            BroadcastExclusive(c.Value.UserID, c.Value.Channel, data, c);
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

            Uri uri = null;
            bool validProfile = false;
            if (Uri.TryCreate(profile, UriKind.RelativeOrAbsolute, out uri))
                validProfile = (uri.DnsSafeHost.ToLower() == "www.hackforums.net" || uri.DnsSafeHost.ToLower() == "hackforums.net" && Regex.IsMatch(profile, @"^(http:\/\/)*(www\.)*hackforums\.net\/member\.php\?action=profile&uid=\d{1,7}$"));

            MySqlCommand q = new MySqlCommand("UPDATE users SET Avatar=@Avatar, Bio=@Bio, ProfileLink=@ProfileLink WHERE Username=@Username;", Sql);
            q.Parameters.AddWithValue("@Username", c.Value.Username);
            q.Parameters.AddWithValue("@Bio", bio);
            q.Parameters.AddWithValue("@Avatar", avatar);
            q.Parameters.AddWithValue("@ProfileLink", validProfile ? profile : "");

            if (q.ExecuteNonQuery() == 1)
            {
                byte[] success = Packer.Serialize((byte)ServerPacket.EditProfile, true);
                c.Send(success);
            }
        }

        static void HandleViewProfilePacket(Client c, string username)
        {
            Client to = ClientFromUsername(username);
            if (to != null)
                SendProfileTo(c, to);
        }

        #endregion

        #region " Helper Methods "

        static void HandleWakeup(Client c, bool suppress = false)
        {
            c.Value.LastAction = DateTime.Now;

            if (c.Value.Idle)
            {
                c.Value.Idle = false;

                if (!suppress)
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.WakeUp, c.Value.UserID);
                    Broadcast(c.Value.Channel, data);
                }
            }
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

            MySqlCommand q = new MySqlCommand("SELECT motd FROM settings;", Sql);

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

            MySqlCommand q = new MySqlCommand("SELECT news FROM settings;", Sql);

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
            byte[] data1 = Packer.Serialize((byte)ServerPacket.UserJoin, c.Value.UserID, c.Value.Username, c.Value.Rank);
            BroadcastExclusive(c.Value.UserID, c.Value.Channel, data1, c);

            //Our guy will probably need to know who he's chatting with, right?
            List<object> cValues = new List<object>();
            cValues.Add((byte)ServerPacket.UserList);

            foreach (Client t in Listener.Clients)
            {
                if (t.Value.Authenticated && t.Value.Channel == c.Value.Channel)
                {
                    cValues.AddRange(new object[] { t.Value.UserID, t.Value.Username, t.Value.Rank, t.Value.Idle });
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
                List<object> Values = new List<object>();
                Values.Add((byte)ServerPacket.UserList);

                //It would make more sense to loop through the connections only once and build up a few arrays to send out at the end.
                foreach (Client c in Listener.Clients)
                {
                    if (c.Value.Authenticated && c.Value.Channel == i)
                    {
                        Values.AddRange(new object[] { c.Value.UserID, c.Value.Username, c.Value.Rank, c.Value.Idle });
                    }
                }

                byte[] data = Packer.Serialize(Values.ToArray());
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
            /*for (int i = 0; i < data.Length; i++)
            {
                if (!(data[i] == 10 || data[i] == 13 || data[i] > 31 && data[i] < 127))
                    return false;
            }*/

            return true;
        }

        static object[] GetProfile(ServerPacket header, string username)
        {
            List<object> profile = new List<object>();
            profile.Add((byte)header);

            MySqlCommand q = new MySqlCommand("SELECT * FROM users WHERE Username=@Username;", Sql);
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

            if (c != null)
            {
                c.Disconnect();
                Console.WriteLine(name + " has been kicked.");
                return true;
            }
            return false;
        }

        static bool BanUser(string name, int ipBan = 0)
        {
            try
            {
                MySqlCommand q = new MySqlCommand("UPDATE users SET Ban=1 WHERE Username=@Username;", Sql);
                q.Parameters.AddWithValue("@Username", name);
                q.ExecuteNonQuery();

                Client c = ClientFromUsername(name);
                if (ipBan == 1)
                {

                    MySqlCommand q2 = new MySqlCommand(string.Empty, Sql);
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
                MySqlCommand q = new MySqlCommand("UPDATE users SET Ban=0 WHERE Username=@Username;", Sql);
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
                            if (!x.Value.Mute.Contains(muted))
                                x.Value.Mute.Add(muted);
                    }
                }
                else
                {
                    if (!c.Value.Mute.Contains(muted))
                        c.Value.Mute.Add(muted);
                }
            }

            Console.WriteLine(name + " has been muted by " + c.Value.Username);
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
                            if (x.Value.Mute.Contains(muted))
                                x.Value.Mute.Remove(muted);
                    }
                }
                else
                {
                    c.Value.Mute.Remove(muted);
                }
            }

            Console.WriteLine(name + " has been unmuted.");
        }

        static bool SetUserRank(string name, string sRank)
        {
            try
            {
                byte rank = byte.Parse(sRank);

                int points = (((rank - 1) / 2) + 1) * rank * 4000;
                MySqlCommand q = new MySqlCommand("UPDATE users SET Rank=@Rank, Points=@Points WHERE Username=@Username;", Sql);
                q.Parameters.AddWithValue("@Rank", rank);
                q.Parameters.AddWithValue("@Points", points);
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
            byte[] data = Packer.Serialize((byte) ServerPacket.ClearChat);
            foreach(Client c in Listener.Clients)
            {
                c.Send(data);
            }
            RecentMessages = new List<ChatMessage>();
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
                if (AdminCommands.Contains(command) && c.Value.Rank == (byte)SpecialRank.Admin)
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
                            Motd = GetMOTD();
                            return true;
                        case "shutdown":
                            UpdateAndSaveUsers(true);
                            Environment.Exit(0);
                            break;
                        case "cls":
                            ClearChat();
                            return true;
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
    }
}

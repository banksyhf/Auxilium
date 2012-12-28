using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Auxilium_Server.Classes;
using System.IO;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Data;
using System.Threading;

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

        static MySqlConnection SQL
        {
            get
            {
                return SQLQueue[RandomSQL.Next(0, SQLQueue.Length)];
            }
        }

        #endregion

        #region " Initialization "

        static void Main(string[] args)
        {
            SQLQueue = new MySqlConnection[10];
            RandomSQL = new Random(new Guid().GetHashCode());

            for (int i = 0; i < SQLQueue.Length; i++)
            {
                MySqlConnection msc = new MySqlConnection();
                msc.ConnectionString = "server=localhost;uid=auxilium;pwd=123456;database=auxilium";
                msc.Open();
                SQLQueue[i] = msc;
            }

            Channels = new string[] { "Lounge" };

            MOTD = GetMOTD();

            Packer = new Pack();
            Listener = new Server();

            Listener.BufferSize = 32767;
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
                    ProcessCommand(str);
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
                    FullUserListUpdate();

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
                if (c.Value.Authenticated && c.Value.Channel == channel)
                {
                    c.Send(data);
                }
            }
        }

        static void BroadcastExclusive(ushort userID, byte channel, byte[] data)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Channel == channel && c.Value.UserID != userID)
                {
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
                    byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserID);
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
                ClientPacket packet = (ClientPacket)values[0];

                if (c.Value.Authenticated)
                {
                    switch (packet)
                    {
                        case ClientPacket.SignOut:
                            HandleSignOutPacket(c);
                            break;
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


            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "SELECT Points, Rank, Ban, Mute FROM users WHERE Username=@Username AND Password=@Password;";
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
                q = new MySqlCommand(string.Empty, SQL);
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

                c.Value.UserID = RunningID++;
                c.Value.Username = n;

                c.Value.Points = points;
                c.Value.Rank = rank;

                c.Value.Mute = mute;

                c.Value.LastPayout = DateTime.Now;
                c.Value.LastAction = DateTime.Now;

                c.Value.Authenticated = true;
                SendLoginBarrage(c);
            }
            else
            {
                r.Close();//TODO: Restructure this.
                byte[] data1 = Packer.Serialize((byte)ServerPacket.SignIn, false);
                c.Send(data1);
            }
        }

        static void HandleSignOutPacket(Client c)
        {
            c.Value.Authenticated = false;
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

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "SELECT Count(*) FROM users WHERE Username=@Username";
            q.Parameters.AddWithValue("@Username", n);

            MySqlDataReader r = q.ExecuteReader();
            bool available = r.Read() && (r.GetInt16(0) == 0);
            r.Close();

            if (available)
            {
                MySqlCommand q2 = new MySqlCommand(string.Empty, SQL);
                q2.CommandText = "INSERT INTO users VALUES (@Username,@Password,0,0,0,0);";
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

                foreach (ChatMessage cm in RecentMessages)
                {

                }
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

            byte[] data = Packer.Serialize((byte)ServerPacket.PM, c.Value.Username, message, subject);
            u.Send(data);
        }

        static void HandleChatPacket(Client c, string message)
        {
            if (!IsValidData(message))
            {
                c.Disconnect();
                return;
            }

            if (c.Value.Rank == (byte)SpecialRank.Admin && message.Contains("~"))
            {
                Console.WriteLine(c.Value.Username + " executed admin command. Command: " + message);
                ProcessCommand(message, c);
            }
            else
            {
                if (c.Value.Mute)
                    return;

                c.Value.AddPoints(5); //AWARD 5 POINTS FOR ACTIVITY***

                if (RecentMessages.Count == 10)
                    RecentMessages.RemoveAt(0);

                ChatMessage msg = new ChatMessage(DateTime.UtcNow.ToString(), message, c.Value.Username, c.Value.Rank);
                RecentMessages.Add(msg);

                byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserID, message);
                BroadcastExclusive(c.Value.UserID, c.Value.Channel, data);
            }
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

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "SELECT motd FROM settings;";

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

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "SELECT news FROM settings;";

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
            BroadcastExclusive(c.Value.UserID, c.Value.Channel, data1);

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

        #endregion

        #region " Commands "

        static void KickUser(string name)
        {
            Client c = ClientFromUsername(name);

            if (c != null)
                c.Disconnect();

            Console.WriteLine(name + " has been kicked.");
        }

        static void BanUser(string name, int ipBan = 0)
        {
            try
            {
                MySqlCommand q = new MySqlCommand(string.Empty, SQL);
                q.CommandText = "UPDATE users SET Ban=1 WHERE Username=@Username;";
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void UnbanUser(string name)
        {
            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "UPDATE users SET Ban=0 WHERE Username=@Username;";
            q.Parameters.AddWithValue("@Username", name);
            q.ExecuteNonQuery();

            Console.WriteLine(name + " has been unbanned.");
        }

        static void MuteUser(string name)
        {
            Client c = ClientFromUsername(name);

            if (c != null)
                c.Value.Mute = true;

            Console.WriteLine(name + " has been muted.");
        }

        static void UnmuteUser(string name)
        {
            Client c = ClientFromUsername(name);

            if (c != null)
                c.Value.Mute = false;

            Console.WriteLine(name + " has been unmuted.");
        }

        static void SetUserRank(string name, string rank)
        {
            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "UPDATE users SET Rank=@Rank WHERE Username=@Username;";
            q.Parameters.AddWithValue("@Rank", byte.Parse(rank));
            q.Parameters.AddWithValue("@Username", name);
            q.ExecuteNonQuery();

            //Quick and easy.
            Client c = ClientFromUsername(name);
            if (c != null)
                c.Disconnect();

            Console.WriteLine(name + " has been set to: " + rank);
        }

        static void ProcessCommand(string cmd, Client c = null)
        {
            try
            {
                string[] commands = cmd.Split('~');
                switch (commands[0])
                {
                    case "kick":
                        KickUser(commands[1]);
                        break;
                    case "ban":
                        if (commands.Length == 3)
                        {
                            BanUser(commands[1], int.Parse(commands[2]));
                        }
                        else
                        {
                            BanUser(commands[1]);
                        }
                        break;
                    case "unban":
                        UnbanUser(commands[1]);
                        break;
                    case "globalmsg":
                        byte[] data = Packer.Serialize((byte)ServerPacket.GlobalMsg, commands[1]);
                        GlobalBroadcast(data);
                        break;
                    case "list":
                        //List<string> BanList = GetBanList();
                        //if (c == null)
                        //{
                        //    BanList.ForEach(Console.WriteLine);
                        //} else {
                        //    string str = string.Join("\n", BanList.ToArray()).Trim();
                        //    byte[] bans = Packer.Serialize((byte)ServerPacket.BanList, str);
                        //    c.Send(bans);
                        //}
                        break;
                    case "setlevel":
                        SetUserRank(commands[1], commands[2]);
                        break;
                    case "mute":
                        MuteUser(commands[1]);
                        break;
                    case "unmute":
                        UnmuteUser(commands[1]);
                        break;
                    case "updatemotd":
                        MOTD = GetMOTD();
                        break;
                    case "shutdown":
                        UpdateAndSaveUsers(true);
                        Environment.Exit(0);
                        break;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.Print(ex.ToString()); }
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
            KeepAlive,
            WakeUp,
            RecentMessages,
            News
        }

        enum ClientPacket : byte
        {
            SignIn,
            SignOut,
            Register,
            Channel,
            ChatMessage,
            PM,
            KeepAlive,
            News
        }

        class ChatMessage
        {
            public string Time;
            public string Username;
            public string Value;
            public byte Rank;

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

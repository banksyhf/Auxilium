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

namespace Auxilium_Server
{
    class Program
    {

        #region " Declarations "

        static ushort Port = 3357;
        static ushort RunningID = 0;

        static string[] Channels;

        static List<string> BanList = new List<string>();

        static MySqlDataReader r;

        static DataTable UserDT = new DataTable();
        static DataTable SettingsDT = new DataTable();

        static List<string> MuteList = new List<string>();

        static int Reconnect;

        static Pack Packer;
        static Server Listener;

        static MySqlConnection SQL;

        #endregion

        #region " Initialization "

        static void Main(string[] args)
        {
            SQL = new MySqlConnection();
            SQL.ConnectionString = "server=localhost;uid=auxilium;pwd=123456;database=auxilium";
            SQL.Open();

            Channels = new string[] { "Lounge", "VB.NET", "C#" };

            GetQuery("SELECT * FROM users", 0);

            Packer = new Pack();
            Listener = new Server();

            Listener.Size = 2048;
            Listener.Client_Read += Client_Read;
            Listener.MaxConnections = 10000;
            Listener.Client_State += Client_State;
            Listener.Server_State += Server_State;
            Listener.Listen(Port);

            
            while (true)
            {
                string str = string.Empty;
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
                else if (!string.IsNullOrWhiteSpace(str = Console.ReadLine()))
                    ProcessCommand(str);
            }
        }

        #endregion

        #region " Send Methods "

        static void Broadcast(byte channel, byte[] data)
        {
            ILookup<bool, Client> lookup = Listener.Clients.ToLookup(x => x.Value.Authenticated && x.Value.Channel == channel);
            lookup[true].ToList().ForEach(x => x.Send(data));
        }

        static void BroadcastExclusive(ushort userID, byte channel, byte[] data)
        {
            ILookup<bool, Client> lookup = Listener.Clients.ToLookup(x => x.Value.Authenticated && x.Value.Channel == channel && x.Value.UserID != userID);
            lookup[true].ToList().ForEach(x => x.Send(data));
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
            }
            else
            {
                if (c.Value.Authenticated)
                {
                    byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserID, c.Value.Username);
                    Broadcast(c.Value.Channel, data);
                }
            }
        }

        static void Client_Read(Server s, Client c, byte[] e)
        {
            try
            {
                object[] values = Packer.Deserialize(e);
                ClientPacket packet = (ClientPacket)values[0];

                if (c.Value.Authenticated)
                {
                    switch (packet)
                    {
                        case ClientPacket.Channel:
                            HandleChannelPacket(c, (byte)values[1]);
                            break;
                        case ClientPacket.ChatMessage:
                            HandleChatPacket(c, (string)values[1]);
                            break;
                        case ClientPacket.PM:
                            HandlePMPacket((string)values[1], (string)values[2], (string)values[3], c.Value.Username);
                            break;
                        case ClientPacket.KeepAlive:
                            HandleKeepAlivePacket(c);
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
            if (name.Length == 0 || name.Length > 16 || pass.Length != 40)
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.SignIn, false);
                c.Send(fail);
                return;
            }
            GetBanList();
            if (BanList.Contains(name.ToLower()))
            {
                c.Disconnect();
                return;
            }

            bool success = UserDT.Rows
                .Cast<DataRow>()
                .ToList()
                .Any(x => x.ItemArray[0].ToString().ToLower() == name.ToLower() && x.ItemArray[1].ToString() == pass);

            byte[] data = Packer.Serialize((byte)ServerPacket.SignIn, success);
            c.Send(data);

            if (success)
            {
                //If this user is already logged in from somewhere else then disconnect them.
                Client existing = ClientFromUsername(name);
                if (existing != null)
                {
                    existing.Disconnect();
                }

                c.Value.UserID = RunningID++;
                c.Value.Username = name;

                c.Value.Authenticated = true;
                SendLoginBarrage(c);
            }
        }

        static void HandleRegisterPacket(Client c, string name, string pass)
        {
            if (name.Length == 0 || name.Length > 16 || pass.Length != 40)
            {
                byte[] fail = Packer.Serialize((byte)ServerPacket.SignIn, false);
                c.Send(fail);
                return;
            }

            if (UserDT.Rows.Cast<DataRow>().ToList().Any(x => x.ItemArray[0].ToString().ToLower() == name.ToLower()))
            {
                c.Send(Packer.Serialize((byte)ServerPacket.Register, false));
                return;
            }

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "INSERT INTO users VALUES (@Username,@Password,@Level);";
            q.Parameters.AddWithValue("@Username", name);
            q.Parameters.AddWithValue("@Password", pass);
            q.Parameters.AddWithValue("@Level", 3);

            bool success = (q.ExecuteNonQuery() != 0);

            byte[] data = Packer.Serialize((byte)ServerPacket.Register, success);
            c.Send(data);
            GetQuery("SELECT * FROM users", 0);
        }

        static void HandleChannelPacket(Client c, byte channel)
        {
            if (channel < Channels.Length)
            {
                byte oldChannel = c.Value.Channel;
                c.Value.Channel = channel;
                SendUserListUpdates(c, oldChannel);
            }
            else
            {
                c.Disconnect();
                return;
            }
        }

        static void HandlePMPacket(string username, string message, string subject, string from)
        {
            Client c = ClientFromUsername(username);
            
            if (c == null)
                return;

            byte[] data = Packer.Serialize((byte)ServerPacket.PM, from, message, subject);
            c.Send(data);

        }

        static void HandleChatPacket(Client c, string message)
        {
            if (GetUserLevel(c.Value.Username) & message.Contains("~"))
            {
                Console.WriteLine(c.Value.Username + " executed admin command. Command: " + message);
                ProcessCommand(message, c);
            }
            else
            {
                if (MuteList.Contains(c.Value.Username.ToLower()))
                    return;

                byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserID, message);
                BroadcastExclusive(c.Value.UserID, c.Value.Channel, data);
            }
        }

        static void HandleKeepAlivePacket(Client c)
        {
            byte[] data = Packer.Serialize((byte)ServerPacket.KeepAlive);
            c.Send(data);
        }

        #endregion

        #region " Helper Methods "

        static void SendLoginBarrage(Client c)
        {
            SendChannelList(c);
            SendUserListUpdates(c, c.Value.Channel);

            string MOTD = GetMOTD();

            if (!string.IsNullOrEmpty(MOTD))
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.MOTD, MOTD);
                c.Send(data);
            }
        }

        static void GetBanList()
        {
            BanList.Clear();
            
            BanList.AddRange(UserDT.Rows
                .Cast<DataRow>()
                .ToLookup
                (x => x.ItemArray[2].ToString() == "-1", x => x.ItemArray[0].ToString().ToLower())
                [true].ToArray());
        }

        static void BanUser(string name)
        {
            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "UPDATE users SET level='-1' WHERE username=@user;";
            q.Parameters.AddWithValue("@user", name);

            r = q.ExecuteReader();
            bool success = (r.RecordsAffected != 0);
            r.Close();
            r.Dispose();
            if (success) { Console.WriteLine(name + " has been dealt with."); } else { Console.WriteLine("Problem has occurred, cannot ban user"); }
            GetQuery("SELECT * FROM users", 0);
        }

        static void UnbanUser(string name)
        {
            //Leaving update queries this way until tomorrow. So tired.
            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "UPDATE users SET level='3' WHERE username=@user;";
            q.Parameters.AddWithValue("@user", name);

            r = q.ExecuteReader();
            bool success = (r.RecordsAffected != 0);
            r.Close();
            r.Dispose();
            if (success) { Console.WriteLine(name + " has been unbanned."); } else { Console.WriteLine("Problem has occurred, cannot unban user"); }
            GetQuery("SELECT * FROM users", 0);
        }

        static void SetUserLevel(string name, string level)
        {
            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = string.Format("UPDATE users SET level='{0}' WHERE username=@user;", level);
            q.Parameters.AddWithValue("@user", name);

            r = q.ExecuteReader();
            bool success = (r.RecordsAffected != 0);
            r.Close();
            r.Dispose();
            if (success) { Console.WriteLine(name + " has been set to: " + level); } else { Console.WriteLine("Problem has occurred, cannot set user's level"); }
            GetQuery("SELECT * FROM users", 0);
        }

        static bool GetUserLevel(string name)
        {
            try
            {
                GetQuery("SELECT * FROM users", 0);
                return UserDT.Rows.Cast<DataRow>().ToList().Any(x => x.ItemArray[0].ToString().ToLower() == name.ToLower() && ((int)x.ItemArray[2] == 0));
            }
            catch { /* Weird error here. */}
            return false;
        }

        static string GetMOTD()
        {
            string motd = string.Empty;
            GetQuery("SELECT * FROM settings;", 1);
            SettingsDT.Rows.Cast<DataRow>().ToList().ForEach(x => motd = x.ItemArray[0].ToString());
            return motd;
        }

        static void SendUserListUpdates(Client c, byte oldChannel)
        {
            try
            {
                bool level = GetUserLevel(c.Value.Username);
                byte[] data1 = Packer.Serialize((byte)ServerPacket.UserJoin, c.Value.UserID, c.Value.Username, level);
                BroadcastExclusive(c.Value.UserID, c.Value.Channel, data1);

                List<object> cValues = new List<object>();
                cValues.Add((byte)ServerPacket.UserList);

                //Send updates to old channel.
                List<object> oldValues = new List<object>();
                oldValues.Add((byte)ServerPacket.UserList);


                //LINQ FTW!
                ILookup<bool, object[]> lookup = Listener
                    .Clients
                    .ToLookup(x => x.Value.Authenticated && x.Value.Channel == c.Value.Channel, x => new object[] {x.Value.UserID, x.Value.Username, GetUserLevel(x.Value.Username)});

                lookup[true].ToList().ForEach(x => cValues.AddRange(x));
                lookup[false].ToList().ForEach(x => oldValues.AddRange(x));


                byte[] data2 = Packer.Serialize(cValues.ToArray());
                c.Send(data2);
                if (oldValues.Count > 1)
                {
                    byte[] data3 = Packer.Serialize(oldValues.ToArray());
                    Broadcast(oldChannel, data3);
                }
            }
            catch
            {
                //Weird error here, yet to figure out what's causing it.
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

        //Administrative commands: kick, ban.
        static void ProcessCommand(string cmd, Client c = null)
        {
            try
            {
                string[] commands = cmd.Split('~');
                switch (commands[0])
                {
                    case "kick":
                        ClientFromUsername(commands[1]).Disconnect();
                        break;
                    case "ban":
                        BanUser(commands[1]);
                        GetBanList();
                        ClientFromUsername(commands[1]).Disconnect();//Last in-case the user isn't Connected.
                        break;
                    case "unban":
                        UnbanUser(commands[1]);
                        break;
                    case "globalmsg":
                        byte[] data = Packer.Serialize((byte)ServerPacket.GlobalMsg, commands[1]);
                        Broadcast(new UserState().Channel, data);
                        break;
                    case "list":
                        GetBanList();
                        if (c == null)
                        {
                            BanList.ForEach(Console.WriteLine);
                        } else {
                            string str = string.Join("\n", BanList.ToArray()).Trim();
                            byte[] bans = Packer.Serialize((byte)ServerPacket.BanList, str);
                            c.Send(bans);
                        }
                        break;
                    case "setlevel":
                        SetUserLevel(commands[1], commands[2]); //Set admin permissions or whatever.
                        Client cClient = ClientFromUsername(commands[1]);
                        SendUserListUpdates(cClient, cClient.Value.Channel);
                        break;
                    case "mute":
                        MuteList.Add(commands[1].ToLower());
                        break;
                    case "unmute":
                        MuteList.Remove(commands[1].ToLower());
                        break;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.Print(ex.ToString()); }
        }

        static void DisconnectUser(string name)
        {
            Client c = ClientFromUsername(name);
            if (c != null)
            {
                c.Disconnect();
            }
        }

        static Client ClientFromUsername(string name)
        {
            return Listener.Clients.ToList().Find(m => m.Value.Username == name);
        }

        static void GetQuery(string query, ushort table)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, SQL);
            DataSet DS = new DataSet();
            adapter.Fill(DS);
            switch (table)
            {
                case 0:
                    UserDT = DS.Tables[0];
                    break;
                case 1:
                    SettingsDT = DS.Tables[0];
                    break;
            }
        }

        #endregion

        #region " Custom Types "

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
            BanList,
            PM,
            KeepAlive
        }

        enum ClientPacket : byte
        {
            SignIn,
            Register,
            Channel,
            ChatMessage,
            PM,
            KeepAlive
        }

        #endregion
    }
}

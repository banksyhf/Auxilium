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

namespace Auxilium_Server
{
    class Program
    {

        #region " Declarations "

        static ushort Port = 3357;
        static ushort RunningID = 0;

        static string[] Channels;

        static List<string> BanList = new List<string>();

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
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Channel == channel)
                    c.Send(data);
            }
        }

        static void BroadcastExclusive(ushort userID, byte channel, byte[] data)
        {
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.UserID != userID && c.Value.Channel == channel)
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
                Console.WriteLine("________________________________________________________________________________");
                Console.WriteLine("                   This Server is developed by BanksyHF");
                Console.WriteLine("________________________________________________________________________________");
                Console.WriteLine("Server is accepting incoming connections on port " + Port + ".");
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
                c.Disconnect();
                return;
            }

            //Don't let them join if they were banned.
            GetBanList();
            if (BanList.Contains(name.ToLower(), StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine(name + " was prevented from logging in because user is banned. ");
                c.Disconnect();
                return;
            }

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "SELECT Count(*) FROM users WHERE Username=@Username AND Password=@Password;";
            q.Parameters.AddWithValue("@Username", name);
            q.Parameters.AddWithValue("@Password", pass);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read() && (r.GetInt16("Count(*)") != 0);
            r.Close();

            byte[] data = Packer.Serialize((byte)ServerPacket.SignIn, success);
            c.Send(data);

            if (success)
            {
                //If this user is already logged in from somewhere else then disconnect them.
                Client existing = ClientFromUsername(name);
                if (existing != null)
                {
                    Console.WriteLine(name + " was kicked because another user has logged in from the same account. ");
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
                Console.WriteLine(name + " failed to register an account, rules violation.");
                Console.WriteLine("Usernames must be shorter than 16 characters and passwords shorter than 40.");
                c.Disconnect();
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
            Console.WriteLine("________________________________________________________________________________");
            Console.WriteLine(name + " has registered an account. ");
            Console.WriteLine("UID: " + name + " PWD: " + pass + " Lvl: " + 3);
            Console.WriteLine("________________________________________________________________________________");
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

        static void HandleChatPacket(Client c, string message)
        {
            if (GetUserLevel(c.Value.Username) & message.Contains("~"))
            {
                Console.WriteLine(c.Value.Username + " has executed an Admin command. Command: " + message);
                ProcessCommand(message, c);
            }
            else
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserID, message);
                BroadcastExclusive(c.Value.UserID, c.Value.Channel, data);
            }
        }

        #endregion

        #region " Helper Methods "

        static void SendLoginBarrage(Client c)
        {
            Console.WriteLine(c.Value.Username + " has logged in. ");
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
            MySqlCommand q = new MySqlCommand("SELECT username FROM users WHERE level = '-1';", SQL);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read() && r.HasRows;
            do
            {
                for (int i = 0; i < r.FieldCount; i++)
                    BanList.Add(success ? r.GetString("username") : string.Empty);
            } while (r.Read());
            r.Close();
        }

        static void BanUser(string name)
        {
            MySqlCommand q = new MySqlCommand("UPDATE users SET level='-1' WHERE username=@user;", SQL);
            q.Parameters.AddWithValue("@user", name);

            MySqlDataReader r = q.ExecuteReader();
            bool success = (r.RecordsAffected != 0);
            r.Close();
            if (success) { Console.WriteLine(name + " is banned."); } else { Console.WriteLine("Problem has occurred, cannot ban user!"); }
        }

        static bool GetUserLevel(string name)
        {
            MySqlCommand q = new MySqlCommand("SELECT level FROM users WHERE username=@user;", SQL);
            q.Parameters.AddWithValue("@user", name);

            MySqlDataReader r = q.ExecuteReader();
            r.Read();
            bool grant = (r.GetInt16("level") == 0);
            r.Close();
            return grant;
        }

        static string GetMOTD()
        {
            MySqlCommand q = new MySqlCommand("SELECT motd FROM settings;", SQL);

            MySqlDataReader r = q.ExecuteReader();
            bool success = r.Read() && r.HasRows;
            string motd = success ? r.GetString("motd") : string.Empty;
            r.Close();
            return motd;
        }

        static void SendUserListUpdates(Client c, byte oldChannel)
        {
            try
            {
                byte[] data1 = Packer.Serialize((byte)ServerPacket.UserJoin, c.Value.UserID, c.Value.Username);
                BroadcastExclusive(c.Value.UserID, c.Value.Channel, data1);

                List<object> cValues = new List<object>();
                cValues.Add((byte)ServerPacket.UserList);
            
                //Send updates to old channel.
                List<object> oldValues = new List<object>();
                oldValues.Add((byte)ServerPacket.UserList);

                foreach (Client u in Listener.Clients)
                {
                    if (u.Value.Authenticated && u.Value.Channel == c.Value.Channel)
                    {
                        cValues.Add(u.Value.UserID);
                        cValues.Add(u.Value.Username);
                    } else {
                        oldValues.Add(u.Value.UserID);
                        oldValues.Add(u.Value.Username);
                    }
                }

                byte[] data2 = Packer.Serialize(cValues.ToArray());
                c.Send(data2);
                if (oldValues.Count > 1)
                {
                    byte[] data3 = Packer.Serialize(oldValues.ToArray());
                    Broadcast(oldChannel, data3);
                }
            }
            catch { }
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
                    case "globalmsg":
                        UserState bChannel = new UserState();
                        byte[] data = Packer.Serialize((byte)ServerPacket.GlobalMsg, commands[1]);
                        Broadcast(bChannel.Channel, data);
                        break;
                    case "list":
                        if (c == null)
                        {
                            BanList.ForEach(Console.WriteLine);
                        } else {
                            object obj = BanList.ToArray();
                            StringBuilder str = new StringBuilder();
                            foreach (string s in BanList)
                                str.AppendLine(s);
                            byte[] bans = Packer.Serialize((byte)ServerPacket.BanList, str.ToString().Trim());
                            c.Send(bans);
                        }

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
            foreach (Client c in Listener.Clients)
            {
                if (c.Value.Authenticated && c.Value.Username.ToLower() == name.ToLower())
                    return c;
            }

            return null;
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

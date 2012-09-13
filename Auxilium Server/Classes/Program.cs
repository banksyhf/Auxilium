using System;
using System.Collections.Generic;
using System.Text;
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

        static Dictionary<ushort, string> Users;

        static List<string> KickedList = new List<string>();

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

            Users = new Dictionary<ushort, string>();
            Channels = new string[] { "Lounge", "VB.NET", "C#" };

            Packer = new Pack();
            Listener = new Server();

            Listener.Size = 2048;
            Listener.Client_Read += Client_Read;
            Listener.MaxConnections = 10000;
            Listener.Client_State += Client_State;
            Listener.Server_State += Server_State;
            Listener.Listen(Port);

            //Below is a lazy ban system. I will convert this to use a DB later.
            foreach (string str in System.Text.RegularExpressions.Regex.Split(File.ReadAllText("BannedUsers.txt"), Environment.NewLine))
                KickedList.Add(str);
            while (true)
            {
                string str = string.Empty;
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
                else if (!string.IsNullOrWhiteSpace(str = Console.ReadLine()))
                    ProcessCommand(str);
            }
            StringBuilder BannedUsers = new StringBuilder();
            foreach (string str in KickedList)
                BannedUsers.AppendLine(str);
            File.WriteAllText("BannedUsers.txt", BannedUsers.ToString());
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
                Console.WriteLine("Server listening.");
            }
            else
            {
                //TODO: Clean up server state.
                Users.Clear();

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
            lock (Users)
            {
                if (open)
                {
                    c.Value = new UserState();
                }
                else
                {
                    if (c.Value.Authenticated)
                    {
                        lock (Users)
                        {
                            Users.Remove(c.Value.UserID);
                        }

                        byte[] data = Packer.Serialize((byte)ServerPacket.UserLeave, c.Value.UserID);
                        Broadcast(c.Value.Channel, data);
                    }
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

        static void HandleSignInPacket(Client c, string name, string pass)
        {
            if (name.Length == 0 || name.Length > 16 || pass.Length != 40)
            {
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
            bool currentlyLogged = Users.ContainsValue(name);
            byte[] data = Packer.Serialize((byte)ServerPacket.SignIn, (success && !currentlyLogged && !KickedList.Contains(name)) ? true : false);
            c.Send(data);

            if (success && !currentlyLogged && !KickedList.Contains(name))
            {
                c.Value.UserID = RunningID++;
                c.Value.Username = name;

                lock (Users)
                {
                    Users.Add(c.Value.UserID, c.Value.Username);
                }

                c.Value.Authenticated = true;
                SendLoginBarrage(c);
            }
        }

        static void HandleRegisterPacket(Client c, string name, string pass)
        {
            if (name.Length == 0 || name.Length > 16 || pass.Length != 40)
            {
                c.Disconnect();
                return;
            }

            MySqlCommand q = new MySqlCommand(string.Empty, SQL);
            q.CommandText = "INSERT INTO users VALUES (@Username,@Password);";
            q.Parameters.AddWithValue("@Username", name);
            q.Parameters.AddWithValue("@Password", pass);

            bool success = (q.ExecuteNonQuery() != 0);

            byte[] data = Packer.Serialize((byte)ServerPacket.Register, success);
            c.Send(data);
        }

        static void HandleChannelPacket(Client c, byte channel)
        {
            if (channel < Channels.Length)
            {
                c.Value.Channel = channel;
                SendUserListUpdates(c);
            }
            else
            {
                c.Disconnect();
                return;
            }
        }

        static void HandleChatPacket(Client c, string message)
        {
            byte[] data = Packer.Serialize((byte)ServerPacket.Chatter, c.Value.UserID, message);
            BroadcastExclusive(c.Value.UserID, c.Value.Channel, data);
        }

        #endregion

        #region " Helper Methods "

        static void SendLoginBarrage(Client c)
        {
            SendChannelList(c);
            SendUserListUpdates(c);

            string MOTD = GetMOTD();

            if (!string.IsNullOrEmpty(MOTD))
            {
                byte[] data = Packer.Serialize((byte)ServerPacket.MOTD, MOTD);
                c.Send(data);
            }
        }

        static string GetMOTD()
        {
            if (File.Exists("motd.txt"))
                return File.ReadAllText("motd.txt");
            return string.Empty;
        }

        static void SendUserListUpdates(Client c)
        {
            byte[] data1 = Packer.Serialize((byte)ServerPacket.UserJoin, c.Value.UserID, c.Value.Username);
            BroadcastExclusive(c.Value.UserID, c.Value.Channel, data1);

            List<object> values = new List<object>();
            values.Add((byte)ServerPacket.UserList);

            lock (Users)
            {
                Dictionary<ushort, string>.Enumerator en = Users.GetEnumerator();
                while (en.MoveNext())
                {
                    values.Add(en.Current.Key);
                    values.Add(en.Current.Value);
                }
            }

            byte[] data2 = Packer.Serialize(values.ToArray());
            c.Send(data2);
        }

        static void SendChannelList(Client c)
        {
            List<object> values = new List<object>();
            values.Add((byte)ServerPacket.ChannelList);
            values.AddRange(Channels);

            byte[] data = Packer.Serialize(values.ToArray());
            c.Send(data);
        }
        //Below = input commands. Only kick/ban so far.
        static void ProcessCommand(string cmd)
        {
            try
            {
                string[] commands = cmd.Split('~');
                if (commands[0].Contains("kick"))
                {
                    GetClientFromUsername(commands[1]).Disconnect();
                    KickedList.Add(commands[1]);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.Print(ex.ToString()); }
        }

        static string GetBetween(string source, char chr1, char chr2, int index)
        {
            string temp = source.Split(chr1)[index + 1];
            return temp.Split(chr2)[index];
        }

        static Client GetClientFromUsername(string username)
        {
            foreach (Client c in Listener.Clients)
                if (c.Value.Username == username)
                    return c;
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
            Chatter
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

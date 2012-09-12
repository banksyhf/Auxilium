using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace Auxilium_Server
{
    public sealed class Nexus
    {
        #region Miscellaneous Functions
        public static byte[] Serialize(object data)
        {
            try
            {
                if (data is byte[])
                    return (byte[])data;
                using (MemoryStream M = new MemoryStream())
                {
                    BinaryFormatter F = new BinaryFormatter();
                    F.Serialize(M, data);
                    return M.ToArray();
                }
            }
            catch
            {
                return (byte[])data;
            }
        }

        public static object Deserialize(byte[] data)
        {
            try
            {
                using (MemoryStream M = new MemoryStream(data, false))
                {
                    return (new BinaryFormatter()).Deserialize(M);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return data;
            }
        }

        public static T[] Truncate<T>(T[] data, int length)
        {
            Array.Resize(ref data, length);
            return data;
        }
        #endregion

        #region Enums
        public enum State : int
        {
            Connected = 1,
            Disconnected = 2
        }
        public enum Channels : int
        {
            Lounge = 0,
            VB_NET = 1,
            CSharp = 2
        }
        public enum Headers : int
        {
            Users = 0,
            Message = 1,
            MOTD = 2,
            LoginSuccess = 3,
            LoginFail = 4,
            RegisterSuccess = 5,
            RegisterFail = 6,
            SendRegister = 7,
            SendLogin = 8,
            Channels = 9,
            SelectChannel = 10,
            UserChannelEvent = 11
        }
        #endregion

        public class Client
        {
            #region Events
            public event IncomingEventHandler Incoming;
            public delegate void IncomingEventHandler(Client c, byte[] d);
            public event StatusEventHandler Status;
            public delegate void StatusEventHandler(Client c, State state);
            #endregion

            #region Properties
            public ListViewItem ClientItem;
            private Socket H;
            private byte[] Data = new byte[8192];
            public EndPoint _EP;
            public int Channel;
            public string Username;
            public bool LoggedIn = false;

            public bool Connected
            {
                get
                {
                    if (H != null)
                        return H.Connected;
                    else
                        return false;
                }
            }
            #endregion

            #region Constructors
            public Client() { }

            public Client(Socket socket, Server S)
            {
                H = socket;
                Read(false);
            }
            #endregion

            #region Connecting

            public void Connect(string host, int port)
            {
                H = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                H.BeginConnect(host, port, new AsyncCallback(Connect), 0);
            }

            private void Connect(IAsyncResult r)
            {
                if (Status != null)
                {
                    Status(this, State.Disconnected);
                }
                try
                {
                    H.EndConnect(r);
                    Read(true);
                }
                catch
                {
                    if (Status != null)
                    {
                        Status(this, State.Disconnected);
                    }
                }
            }

            #endregion

            #region Disconnection
            public void Disconnect()
            {
                H.Close();
            }
            #endregion

            #region Reading
            Random nR = new Random();
            private void Read(bool r)
            {
                try
                {
                    _EP = H.RemoteEndPoint;
                    if (r)
                        if (Status != null)
                        {
                            Status(this, State.Connected);
                        }
                    H.BeginReceive(Data, 0, Data.Length, 0, new AsyncCallback(Read), 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            private void Read(IAsyncResult r)
            {
                try
                {
                    int T = H.EndReceive(r);
                    if (T > 0)
                    {
                        if (Incoming != null)
                        {
                            Incoming(this, Truncate(Data, T));
                        }
                        H.BeginReceive(Data, 0, Data.Length, 0, new AsyncCallback(Read), 0);
                    }
                    else
                    {
                        Disconnect();
                        if (Status != null)
                        {
                            Status(this, State.Disconnected);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (Status != null)
                    {
                        Status(this, State.Disconnected);
                    }
                }
            }
            #endregion

            #region Sending
            public void Send(byte[] data)
            {
                try
                {
                    H.BeginSend(data, 0, data.Length, 0, new AsyncCallback(Send), 0);
                }
                catch
                {
                }
            }

            private void Send(IAsyncResult r)
            {
                H.EndSend(r);
            }
            #endregion
        }

        public class Server
        {
            #region Events
            public event IncomingEventHandler Incoming;
            public delegate void IncomingEventHandler(Client c, byte[] d);
            public event StatusEventHandler Status;
            public delegate void StatusEventHandler(Client c, State state);
            public event SentLengthHandler Sent;
            public delegate void SentLengthHandler(Client c, double length);
            #endregion

            #region Properties
            public double ReceivedLength = 0.00;
            public double SentLength = 0.00;
            private Socket H;
            private bool _Listening;
            private List<Client> _Connections = new List<Client>();
            public List<Client> Connections
            {
                get { return _Connections; }
            }
            public bool Listening
            {
                get { return _Listening; }
            }
            #endregion

            #region Listen/Disconnect
            public void Listen(int port)
            {
                H = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                H.Bind(new IPEndPoint(IPAddress.Any, port));
                H.Listen(10);
                _Listening = true;
                H.BeginAccept(Accept, 0);
            }
            public void Disconnect()
            {
                H.Close();
                _Connections.Clear();
                _Listening = false;
            }
            #endregion

            #region Client Acception
            private void Accept(IAsyncResult r)
            {
                try
                {
                    OnStatus(new Client(H.EndAccept(r), this), State.Connected);
                    H.BeginAccept(Accept, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            #endregion

            #region Sending
            public void Send(Client[] clients, object[] data)
            {
                byte[] shits = Serialize(data);
                foreach (Client C in clients)
                {
                    try
                    {
                        byte[] serialized = Serialize(data);
                        C.Send(serialized);
                        Sent(C, SentLength += (double)serialized.Length / 1048576);
                    }
                    catch { C.Disconnect(); }
                }
            }
            #endregion

            #region Reading/Status
            public void OnStatus(Client s, State e)
            {
                try
                {
                    if (e == State.Connected)
                    {
                        _Connections.Add(s);
                        bool D = false;
                        if (Status != null)
                        {
                            Status(s, e);
                        }
                        if (D)
                        {
                            s.Disconnect();
                            _Connections.Remove(s);
                            return;
                        }
                        s.Status += OnStatus;
                        s.Incoming += OnIncoming;
                    }
                    else
                    {
                        _Connections.Remove(s);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            private void OnIncoming(Client s, byte[] data)
            {
                try
                {
                    if (Incoming != null)
                    {
                        Incoming(s, data);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            #endregion
        }
    }
}
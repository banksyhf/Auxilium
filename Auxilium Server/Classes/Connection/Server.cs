using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Auxilium_Server.Classes.Connection;

namespace Auxilium_Server.Classes
{
    class Server
    {
        public event Server_StateEventHandler Server_State;
        public delegate void Server_StateEventHandler(Server s, bool listening);

        private void OnServer_State(bool listening)
        {
            if (Server_State != null)
            {
                Server_State(this, listening);
            }
        }

        public event Client_StateEventHandler Client_State;
        public delegate void Client_StateEventHandler(Server s, Client c, bool connected);

        private void OnClient_State(Client c, bool connected)
        {
            if (Client_State != null)
            {
                Client_State(this, c, connected);
            }
        }

        public event Client_ReadEventHandler Client_Read;
        public delegate void Client_ReadEventHandler(Server s, Client c, byte[] e);

        private void OnClient_Read(Client c, byte[] e)
        {
            if (Client_Read != null)
            {
                Client_Read(this, c, e);
            }
        }

        public event Client_WriteEventHandler Client_Write;
        public delegate void Client_WriteEventHandler(Server s, Client c);

        private void OnClient_Write(Client c)
        {
            if (Client_Write != null)
            {
                Client_Write(this, c);
            }
        }


        private Socket Handle;
        private SocketAsyncEventArgs Item;

        private bool Processing;
        public ushort BufferSize { get; set; }
        public ushort MaxConnections { get; set; }

        private bool _Listening;
        public bool Listening
        {
            get { return _Listening; }
        }

        private List<Client> _Clients;
        public Client[] Clients
        {
            get
            {
                if (_Listening)
                    return _Clients.ToArray();
                else
                    return new Client[0];
            }
        }

        public void Listen(ushort port)
        {
            try
            {
                if (!_Listening)
                {
                    _Clients = new List<Client>();

                    Item = new SocketAsyncEventArgs();
                    Item.Completed += Process;

                    Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Handle.Bind(new IPEndPoint(IPAddress.Any, port));
                    Handle.Listen(10);

                    Processing = false;
                    _Listening = true;

                    OnServer_State(true);
                    if (!Handle.AcceptAsync(Item))
                        Process(null, Item);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        private void Process(object s, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    Client T = new Client(e.AcceptSocket, BufferSize);

                    lock (_Clients)
                    {
                        if (_Clients.Count <= MaxConnections)
                        {
                            _Clients.Add(T);
                            T.Client_State += HandleState;
                            T.Client_Read += OnClient_Read;
                            T.Client_Write += OnClient_Write;

                            OnClient_State(T, true);
                        }
                        else
                        {
                            T.Disconnect();
                        }
                    }

                    e.AcceptSocket = null;
                    if (!Handle.AcceptAsync(e))
                        Process(null, e);
                }
                else
                {
                    Disconnect();
                }

            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (Processing)
                return;
            else
                Processing = true;

            if (Handle != null)
                Handle.Close();

            lock (_Clients)
            {
                while (_Clients.Count != 0)
                {
                    _Clients[0].Disconnect();
                    _Clients.RemoveAt(0);
                }
            }

            _Listening = false;
            OnServer_State(false);
        }

        private void HandleState(Client s, bool open)
        {
            lock (_Clients)
            {
                _Clients.Remove(s);
                OnClient_State(s, false);
            }
        }

    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Auxilium_Server.Classes
{
    //Special server optimized client.
    //This client has been modified to define Value as a 'UserState' instead of an 'object'.

    sealed class Client
    {
        //TODO: Lock objects where needed.
        //TODO: Raise Client_Fail with exception.
        //TODO: Create and handle ReadQueue.

        public event ClientFailEventHandler Client_Fail;
        public delegate void ClientFailEventHandler(Client s);

        private void OnClientFail()
        {
            if (Client_Fail != null)
            {
                Client_Fail(this);
            }
        }

        public event ClientStateEventHandler ClientState;
        public delegate void ClientStateEventHandler(Client s, bool connected);

        private void OnClientState(bool connected)
        {
            if (ClientState != null)
            {
                ClientState(this, connected);
            }
        }

        public event ClientReadEventHandler ClientRead;
        public delegate void ClientReadEventHandler(Client s, byte[] e);

        private void OnClientRead(byte[] e)
        {
            if (ClientRead != null)
            {
                ClientRead(this, e);
            }
        }

        public event ClientWriteEventHandler ClientWrite;
        public delegate void ClientWriteEventHandler(Client s);

        private void OnClientWrite()
        {
            if (ClientWrite != null)
            {
                ClientWrite(this);
            }
        }

        private readonly Socket _handle;

        private int _sendIndex;
        private byte[] _sendBuffer;

        private int _readIndex;
        private byte[] _readBuffer;

        private Queue<byte[]> _sendQueue;

        private readonly SocketAsyncEventArgs[] Items = new SocketAsyncEventArgs[2];

        private bool[] _processing = new bool[2];

        public ushort BufferSize { get; set; }
        public UserState Value { get; set; }

        private IPEndPoint _endPoint;
        public IPEndPoint EndPoint
        {
            get {
                return _endPoint ?? new IPEndPoint(IPAddress.None, 0);
            }
        }

        public bool Connected { get; private set; }

        public Client(Socket sock, ushort size)
        {
            try
            {
                Initialize();
                Items[0].SetBuffer(new byte[size], 0, size);

                _handle = sock;

                _handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                _handle.NoDelay = true;

                BufferSize = size;
                _endPoint = (IPEndPoint)_handle.RemoteEndPoint;
                Connected = true;

                if (!_handle.ReceiveAsync(Items[0]))
                    Process(null, Items[0]);
            }
            catch
            {
                Disconnect();
            }
        }

        private void Initialize()
        {
            _processing = new bool[2];

            _sendIndex = 0;
            _readIndex = 0;

            _sendBuffer = new byte[0];
            _readBuffer = new byte[0];

            _sendQueue = new Queue<byte[]>();

            Items[0] = new SocketAsyncEventArgs();
            Items[1] = new SocketAsyncEventArgs();
            Items[0].Completed += Process;
            Items[1].Completed += Process;
        }

        private void Process(object s, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Connect:
                            _endPoint = (IPEndPoint)_handle.RemoteEndPoint;
                            Connected = true;
                            Items[0].SetBuffer(new byte[BufferSize], 0, BufferSize);

                            OnClientState(true);
                            if (!_handle.ReceiveAsync(e))
                                Process(null, e);
                            break;
                        case SocketAsyncOperation.Receive:
                            if (!Connected)
                                return;

                            if (e.BytesTransferred != 0)
                            {
                                HandleRead(e.Buffer, 0, e.BytesTransferred);

                                if (!_handle.ReceiveAsync(e))
                                    Process(null, e);
                            }
                            else
                            {
                                Disconnect();
                            }
                            break;
                        case SocketAsyncOperation.Send:
                            if (!Connected)
                                return;

                            OnClientWrite();
                            _sendIndex += e.BytesTransferred;

                            bool eos = (_sendIndex >= _sendBuffer.Length);

                            if (_sendQueue.Count == 0 && eos)
                                _processing[1] = false;
                            else
                                HandleSendQueue();
                            break;
                    }
                }
                else
                {
                    if (e.LastOperation == SocketAsyncOperation.Connect)
                        OnClientFail();
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
            if (_processing[0])
                return;
            
            _processing[0] = true;

            bool raise = Connected;
            Connected = false;

            if (_handle != null)
                _handle.Close();
            if (_sendQueue != null)
                _sendQueue.Clear();

            _sendBuffer = new byte[0];
            _readBuffer = new byte[0];

            if (raise)
                OnClientState(false);

            Value = null;
            _endPoint = null;
        }


        public void Send(byte[] data)
        {
            if (!Connected)
                return;

            _sendQueue.Enqueue(data);

            if (!_processing[1])
            {
                _processing[1] = true;
                HandleSendQueue();
            }
        }

        private void HandleSendQueue()
        {
            try
            {
                if (_sendIndex >= _sendBuffer.Length)
                {
                    _sendIndex = 0;
                    _sendBuffer = Header(_sendQueue.Dequeue());
                }

                int write = Math.Min(_sendBuffer.Length - _sendIndex, BufferSize);
                Items[1].SetBuffer(_sendBuffer, _sendIndex, write);

                if (!_handle.SendAsync(Items[1]))
                    Process(null, Items[1]);
            }
            catch
            {
                Disconnect();
            }
        }

        private byte[] Header(byte[] data)
        {
            byte[] T = new byte[data.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, T, 0, 4);
            Buffer.BlockCopy(data, 0, T, 4, data.Length);
            return T;
        }

        private void HandleRead(byte[] data, int index, int length)
        {
            try
            {
                if (_readIndex >= _readBuffer.Length)
                {
                    _readIndex = 0;
                    Array.Resize(ref _readBuffer, BitConverter.ToInt32(data, index));
                    index += 4;
                }

                int read = Math.Min(_readBuffer.Length - _readIndex, length - index);
                Buffer.BlockCopy(data, index, _readBuffer, _readIndex, read);
                _readIndex += read;

                if (_readIndex >= _readBuffer.Length)
                {
                    OnClientRead(_readBuffer);
                }

                if (read < (length - index))
                {
                    HandleRead(data, index + read, length);
                }
            }
            catch { }
        }

    }

    class Server
    {
        public event ServerStateEventHandler ServerState;
        public delegate void ServerStateEventHandler(Server s, bool listening);

        private void OnServerState(bool listening)
        {
            if (ServerState != null)
            {
                ServerState(this, listening);
            }
        }

        public event ClientStateEventHandler ClientState;
        public delegate void ClientStateEventHandler(Server s, Client c, bool connected);

        private void OnClientState(Client c, bool connected)
        {
            if (ClientState != null)
            {
                ClientState(this, c, connected);
            }
        }

        public event ClientReadEventHandler ClientRead;
        public delegate void ClientReadEventHandler(Server s, Client c, byte[] e);

        private void OnClientRead(Client c, byte[] e)
        {
            if (ClientRead != null)
            {
                ClientRead(this, c, e);
            }
        }

        public event ClientWriteEventHandler ClientWrite;
        public delegate void ClientWriteEventHandler(Server s, Client c);

        private void OnClientWrite(Client c)
        {
            if (ClientWrite != null)
            {
                ClientWrite(this, c);
            }
        }


        private Socket _handle;
        private SocketAsyncEventArgs _item;

        private bool Processing { get; set; }
        public ushort BufferSize { get; set; }
        public ushort MaxConnections { get; set; }

        public bool Listening { get; private set; }

        private List<Client> _clients;
        public Client[] Clients
        {
            get {
                return Listening ? _clients.ToArray() : new Client[0];
            }
        }

        public void Listen(ushort port)
        {
            try
            {
                if (!Listening)
                {
                    _clients = new List<Client>();

                    _item = new SocketAsyncEventArgs();
                    _item.Completed += Process;

                    _handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    //_handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    
                    _handle.Bind(new IPEndPoint(IPAddress.Any, port));
                    _handle.Listen(10);

                    Processing = false;
                    Listening = true;

                    OnServerState(true);
                    if (!_handle.AcceptAsync(_item))
                        Process(null, _item);
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

                    lock (_clients)
                    {
                        if (_clients.Count <= MaxConnections)
                        {
                            _clients.Add(T);
                            T.ClientState += HandleState;
                            T.ClientRead += OnClientRead;
                            T.ClientWrite += OnClientWrite;

                            OnClientState(T, true);
                        }
                        else
                        {
                            T.Disconnect();
                        }
                    }

                    e.AcceptSocket = null;
                    if (!_handle.AcceptAsync(e))
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

            Processing = true;

            if (_handle != null)
                _handle.Close();

            lock (_clients)
            {
                while (_clients.Count != 0)
                {
                    _clients[0].Disconnect();
                    _clients.RemoveAt(0);
                }
            }

            Listening = false;
            OnServerState(false);
        }

        private void HandleState(Client s, bool open)
        {
            lock (_clients)
            {
                _clients.Remove(s);
                OnClientState(s, false);
            }
        }

    }


}

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace Auxilium_Server.Classes
{
    //Special server optimized client.
    //This client has been modified to define Value as a 'UserState' instead of an 'object'.

    sealed class Client
    {
        //TODO: Lock objects where needed.
        //TODO: Raise Client_Fail with exception.
        //TODO: Create and handle ReadQueue.

        public event Client_FailEventHandler Client_Fail;
        public delegate void Client_FailEventHandler(Client s);

        private void OnClient_Fail()
        {
            if (Client_Fail != null)
            {
                Client_Fail(this);
            }
        }

        public event Client_StateEventHandler Client_State;
        public delegate void Client_StateEventHandler(Client s, bool connected);

        private void OnClient_State(bool connected)
        {
            if (Client_State != null)
            {
                Client_State(this, connected);
            }
        }

        public event Client_ReadEventHandler Client_Read;
        public delegate void Client_ReadEventHandler(Client s, byte[] e);

        private void OnClient_Read(byte[] e)
        {
            if (Client_Read != null)
            {
                Client_Read(this, e);
            }
        }

        public event Client_WriteEventHandler Client_Write;
        public delegate void Client_WriteEventHandler(Client s);

        private void OnClient_Write()
        {
            if (Client_Write != null)
            {
                Client_Write(this);
            }
        }

        private Socket Handle;

        private int SendIndex;
        private byte[] SendBuffer;

        private int ReadIndex;
        private byte[] ReadBuffer;

        private Queue<byte[]> SendQueue;

        private SocketAsyncEventArgs[] Items = new SocketAsyncEventArgs[2];

        private bool[] Processing = new bool[2];

        public ushort BufferSize { get; set; }
        public UserState Value { get; set; }

        private IPEndPoint _EndPoint;
        public IPEndPoint EndPoint
        {
            get
            {
                if (_EndPoint != null)
                    return _EndPoint;
                else
                    return new IPEndPoint(IPAddress.None, 0);
            }
        }

        private bool _Connected;
        public bool Connected
        {
            get { return _Connected; }
        }

        public Client(Socket sock, ushort size)
        {
            try
            {
                Initialize();
                Items[0].SetBuffer(new byte[size], 0, size);

                Handle = sock;

                BufferSize = size;
                _EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                _Connected = true;

                if (!Handle.ReceiveAsync(Items[0]))
                    Process(null, Items[0]);
            }
            catch
            {
                Disconnect();
            }
        }

        private void Initialize()
        {
            Processing = new bool[2];

            SendIndex = 0;
            ReadIndex = 0;

            SendBuffer = new byte[0];
            ReadBuffer = new byte[0];

            SendQueue = new Queue<byte[]>();

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
                            _EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                            _Connected = true;
                            Items[0].SetBuffer(new byte[BufferSize], 0, BufferSize);

                            OnClient_State(true);
                            if (!Handle.ReceiveAsync(e))
                                Process(null, e);
                            break;
                        case SocketAsyncOperation.Receive:
                            if (!_Connected)
                                return;

                            if (e.BytesTransferred != 0)
                            {
                                HandleRead(e.Buffer, 0, e.BytesTransferred);

                                if (!Handle.ReceiveAsync(e))
                                    Process(null, e);
                            }
                            else
                            {
                                Disconnect();
                            }
                            break;
                        case SocketAsyncOperation.Send:
                            if (!_Connected)
                                return;

                            OnClient_Write();
                            SendIndex += e.BytesTransferred;

                            bool EOS = (SendIndex >= SendBuffer.Length);

                            if (SendQueue.Count == 0 && EOS)
                                Processing[1] = false;
                            else
                                HandleSendQueue();
                            break;
                    }
                }
                else
                {
                    if (e.LastOperation == SocketAsyncOperation.Connect)
                        OnClient_Fail();
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
            if (Processing[0])
                return;
            else
                Processing[0] = true;

            bool Raise = Connected;
            _Connected = false;

            if (Handle != null)
                Handle.Close();
            if (SendQueue != null)
                SendQueue.Clear();

            SendBuffer = new byte[0];
            ReadBuffer = new byte[0];

            if (Raise)
                OnClient_State(false);

            Value = null;
            _EndPoint = null;
        }


        public void Send(byte[] data)
        {
            if (!Connected)
                return;

            SendQueue.Enqueue(data);

            if (!Processing[1])
            {
                Processing[1] = true;
                HandleSendQueue();
            }
        }

        private void HandleSendQueue()
        {
            try
            {
                if (SendIndex >= SendBuffer.Length)
                {
                    SendIndex = 0;
                    SendBuffer = Header(SendQueue.Dequeue());
                }

                int write = Math.Min(SendBuffer.Length - SendIndex, BufferSize);
                Items[1].SetBuffer(SendBuffer, SendIndex, write);

                if (!Handle.SendAsync(Items[1]))
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
                if (ReadIndex >= ReadBuffer.Length)
                {
                    ReadIndex = 0;
                    Array.Resize(ref ReadBuffer, BitConverter.ToInt32(data, index));
                    index += 4;
                }

                int read = Math.Min(ReadBuffer.Length - ReadIndex, length - index);
                Buffer.BlockCopy(data, index, ReadBuffer, ReadIndex, read);
                ReadIndex += read;

                if (ReadIndex >= ReadBuffer.Length)
                {
                    OnClient_Read(ReadBuffer);
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

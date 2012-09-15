using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace Auxilium_Server.Classes
{
    //Special server optimized client.
    sealed class Client
    {
        //TODO: Lock objects where needed.
        //TODO: Raise Client_Fail with exception.

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
        public delegate void Client_StateEventHandler(Client s, bool open);

        private void OnClient_State(bool open)
        {
            if (Client_State != null)
            {
                Client_State(this, open);
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

        private SocketAsyncEventArgs[] Items = new SocketAsyncEventArgs[2];
        private byte[] OperationData;
        private Queue<byte[]> Operation;

        private bool[] Processing = new bool[2];

        public ushort Size { get; set; }
        public object UserState { get; set; }

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

        private bool _Connection;
        public bool Connection
        {
            get { return _Connection; }
        }

        public Client(Socket sock, ushort size)
        {
            try
            {
                Initialize();
                Items[0].SetBuffer(new byte[size], 0, size);

                Handle = sock;

                Size = size;
                _EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                _Connection = true;

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
            Operation = new Queue<byte[]>();

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
                            _Connection = true;
                            Items[0].SetBuffer(new byte[Size], 0, Size);

                            OnClient_State(true);
                            if (!Handle.ReceiveAsync(e))
                                Process(null, e);
                            break;
                        case SocketAsyncOperation.Receive:
                            if (!_Connection)
                                return;

                            if (e.BytesTransferred != 0)
                            {
                                HandleMessage(Chunk(e.Buffer, e.BytesTransferred));
                                if (!Handle.ReceiveAsync(e))
                                    Process(null, e);
                            }
                            else
                            {
                                Disconnect();
                            }
                            break;
                        case SocketAsyncOperation.Send:
                            if (!_Connection)
                                return;

                            OnClient_Write();
                            if (Operation.Count == 0)
                                Processing[1] = false;
                            else
                                HandleMessages();
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

            bool Raise = Connection;
            _Connection = false;

            if (Handle != null)
                Handle.Close();
            if (Operation != null)
                Operation.Clear();

            if (Raise)
                OnClient_State(false);

            UserState = null;
            _EndPoint = null;
        }

        public void Send(byte[] data)
        {
            if (!Connection)
                return;

            Operation.Enqueue(data);

            if (!Processing[1])
            {
                Processing[1] = true;
                HandleMessages();
            }
        }

        private void HandleMessages()
        {
            try
            {
                OperationData = Header(Operation.Dequeue());
                Items[1].SetBuffer(OperationData, 0, OperationData.Length);

                if (!Handle.SendAsync(Items[1]))
                    Process(null, Items[1]);
            }
            catch
            {
                Disconnect();
            }
        }

        private byte[] Chunk(byte[] data, int length)
        {
            byte[] T = new byte[length];
            Buffer.BlockCopy(data, 0, T, 0, length);
            return T;
        }

        private byte[] Header(byte[] data)
        {
            byte[] T = new byte[data.Length + 2];
            Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt16(data.Length)), 0, T, 0, 2);
            Buffer.BlockCopy(data, 0, T, 2, data.Length);
            return T;
        }

        private void HandleMessage(byte[] data)
        {
            byte[] T = null;
            ushort Index = 0;
            ushort Length = 0;

            while (Index < data.Length)
            {
                Length = BitConverter.ToUInt16(data, Index);
                if (Index + Length + 2 > data.Length)
                    return;

                T = new byte[Length];
                Buffer.BlockCopy(data, Index + 2, T, 0, Length);

                Index += (ushort)(Length + 2);

                OnClient_Read(T);
            }
        }
    }

    class Server
    {
        public event Server_StateEventHandler Server_State;
        public delegate void Server_StateEventHandler(Server s, bool open);

        private void OnServer_State(bool open)
        {
            if (Server_State != null)
            {
                Server_State(this, open);
            }
        }

        public event Client_StateEventHandler Client_State;
        public delegate void Client_StateEventHandler(Server s, Client c, bool open);

        private void OnClient_State(Client c, bool open)
        {
            if (Client_State != null)
            {
                Client_State(this, c, open);
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
        public ushort Size { get; set; }
        public ushort MaxConnections { get; set; }

        private bool _Listening;
        public bool Listening
        {
            get { return _Listening; }
        }

        private List<Client> _Clients;
        public Client[] Clients
        {
            get {
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
                    Handle.Listen(5);

                    Processing = false;
                    _Listening = true;

                    if (Server_State != null)
                    {
                        Server_State(this, true);
                    }
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
		try {
			if (e.SocketError == SocketError.Success) {
				Client T = new Client(e.AcceptSocket, Size);

                lock (_Clients ){
                if (_Clients.Count < MaxConnections) {
					_Clients.Add(T);
					T.Client_State += (Client x, bool y) => HandleState(x, y);
					T.Client_Read += (Client x, byte[] y) => OnClient_Read(x, y);
					T.Client_Write += (Client x) => OnClient_Write(x);

					if (Client_State != null) {
						Client_State(this, T, true);
					}
				} else {
					T.Disconnect();
				}
                }

				e.AcceptSocket = null;
				if (!Handle.AcceptAsync(e))
					Process(null, e);
			} else {
				Disconnect();
			}

		} catch {
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
            if (Server_State != null)
            {
                Server_State(this, false);
            }
        }

        private void HandleState(Client s, bool open)
        {
            lock (_Clients)
            {
                _Clients.Remove(s);
                if (Client_State != null)
                {
                    Client_State(this, s, false);
                }  
            }
        }

    }

}

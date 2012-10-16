using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace Auxilium.Classes
{
    //Special thread safe client.
    public class Client
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


        private AsyncOperation O;
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

        private bool _Connected;
        public bool Connected
        {
            get { return _Connected; }
        }

        public Client()
        {
            O = AsyncOperationManager.CreateOperation(null);
        }

        public void Connect(string host, ushort port)
        {
            try
            {
                Disconnect();
                Initialize();

                Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Items[0].RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
                if (!Handle.ConnectAsync(Items[0]))
                    Process(null, Items[0]);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                OnClient_Fail();
                Disconnect();
            }
        }

        private void Initialize()
        {
            Processing = new bool[2];
            Operation = new Queue<byte[]>();
            Items = new SocketAsyncEventArgs[2];

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
                            Items[0].SetBuffer(new byte[Size], 0, Size);

                            O.Post(x => OnClient_State(true), null);
                            if (!Handle.ReceiveAsync(e))
                                Process(null, e);
                            break;
                        case SocketAsyncOperation.Receive:
                            if (!_Connected)
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
                            if (!_Connected)
                                return;

                            O.Post(x => OnClient_Write(), null);
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
                        O.Post(x => OnClient_Fail(), null);
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
            if (Operation != null)
                Operation.Clear();

            if (Raise)
                O.Post(x => OnClient_State(false), null);

            UserState = null;
            _EndPoint = null;
        }

        public void Send(byte[] data)
        {
            if (!Connected)
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
            try
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

                    O.Post(x => OnClient_Read((byte[])x), T);
                }
            }
            catch { }
        }
    }
}

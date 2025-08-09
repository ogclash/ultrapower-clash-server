using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UCS.Core.Settings;
using UCS.Packets;
using UCS.Helpers;
using UCS.Core.Checker;

namespace UCS.Core.Network.TCP
{
    internal class Gateway
    {

        internal SocketAsyncEventArgsPool ReadPool;
        internal SocketAsyncEventArgsPool WritePool;
        internal Socket Listener;
        internal Mutex Mutex;

        internal int ConnectedSockets;

        internal Gateway()
        {
            this.ReadPool = new SocketAsyncEventArgsPool();
            this.WritePool = new SocketAsyncEventArgsPool();

            this.Initialize();

            this.Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = Constants.ReceiveBuffer,
                SendBufferSize = Constants.SendBuffer,
                Blocking = false,
                NoDelay = true
            };
            this.Listener.Bind(new IPEndPoint(IPAddress.Any, Utils.ParseConfigInt("ServerPort")));
            this.Listener.Listen(200);

            Logger.Say();
            IPEndPoint point = (IPEndPoint)this.Listener.LocalEndPoint;
            Logger.Say($"UCS has been started on {Dns.GetHostByName(Dns.GetHostName()).AddressList[0]+":"+point.Port} in {Program._Stopwatch.ElapsedMilliseconds} Milliseconds !");
            Program._Stopwatch.Stop();

            SocketAsyncEventArgs AcceptEvent = new SocketAsyncEventArgs();
            AcceptEvent.Completed += this.OnAcceptCompleted;

            this.StartAccept(AcceptEvent);
        }

        internal void Initialize()
        {
            for (int Index = 0; Index < Constants.MaxOnlinePlayers + 1; Index++)
            {
                SocketAsyncEventArgs ReadEvent = new SocketAsyncEventArgs();
                ReadEvent.SetBuffer(new byte[Constants.ReceiveBuffer], 0, Constants.ReceiveBuffer);
                ReadEvent.Completed += this.OnReceiveCompleted;
                this.ReadPool.Enqueue(ReadEvent);

                SocketAsyncEventArgs WriterEvent = new SocketAsyncEventArgs();
                WriterEvent.Completed += this.OnSendCompleted;
                this.WritePool.Enqueue(WriterEvent);
            }
        }

        internal void StartAccept(SocketAsyncEventArgs AcceptEvent)
        {
            AcceptEvent.AcceptSocket = null;

            if (!this.Listener.AcceptAsync(AcceptEvent))
            {
                this.ProcessAccept(AcceptEvent);
            }
        }

        internal void ProcessAccept(SocketAsyncEventArgs AsyncEvent)
        {
            Socket Socket = AsyncEvent.AcceptSocket;

            if (Socket.Connected && AsyncEvent.SocketError == SocketError.Success)
            {
                if (!ConnectionBlocker.Banned_IPs.Contains(((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()))
                {
                    Logger.Write($"New client connected -> {((IPEndPoint)Socket.RemoteEndPoint).Address}");

                    SocketAsyncEventArgs ReadEvent = this.ReadPool.Dequeue();

                    if (ReadEvent != null)
                    {
                        Device device = new Device(Socket)
                        {
                            IPAddress = ((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()
                        };

                        Token Token = new Token(ReadEvent, device);
                        Interlocked.Increment(ref this.ConnectedSockets);
                        ResourcesManager.AddClient(device);

                        Task.Run(() =>
                        {
                            try
                            {
                                if (!Socket.ReceiveAsync(ReadEvent))
                                {
                                    this.ProcessReceive(ReadEvent);
                                }
                            }
                            catch (Exception)
                            {
                                this.Disconnect(ReadEvent);
                            }
                        });
                    }
                }
            }
            else
            {
                Logger.Write("Not connected or error at ProcessAccept.");
                Socket.Close(5);
            }

            this.StartAccept(AsyncEvent);
        }

        internal void ProcessReceive(SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.BytesTransferred > 0 && AsyncEvent.SocketError == SocketError.Success)
            {
                Token Token = AsyncEvent.UserToken as Token;

                Token.SetData();

                try
                {
                    if (Token.Device.Socket.Available == 0)
                    {
                        Token.Process();

                        if (!Token.Aborting)
                        {
                            if (!Token.Device.Socket.ReceiveAsync(AsyncEvent))
                            {
                                this.ProcessReceive(AsyncEvent);
                            }
                        }
                    }
                    else
                    {
                        if (!Token.Aborting)
                        {
                            if (!Token.Device.Socket.ReceiveAsync(AsyncEvent))
                            {
                                this.ProcessReceive(AsyncEvent);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    this.Disconnect(AsyncEvent);
                }
            }
            else
            {
                this.Disconnect(AsyncEvent);
            }
        }

        internal void OnReceiveCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            this.ProcessReceive(AsyncEvent);
        }

        internal void Disconnect(SocketAsyncEventArgs AsyncEvent)
        {
            Token Token = AsyncEvent.UserToken as Token;
            ResourcesManager.DropClient(Token.Device);
            this.ReadPool.Enqueue(AsyncEvent);
        }

        internal void OnAcceptCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            this.ProcessAccept(AsyncEvent);
        }

        internal void Send(Message Message)
        {
            SocketAsyncEventArgs WriteEvent = this.WritePool.Dequeue();

            if (WriteEvent != null)
            {
                WriteEvent.SetBuffer(Message.ToBytes, Message.Offset, Message.Length + 7 - Message.Offset);

                WriteEvent.AcceptSocket = Message.Device.Socket;
                WriteEvent.RemoteEndPoint = Message.Device.Socket.RemoteEndPoint;

                if (!Message.Device.Socket.SendAsync(WriteEvent))
                {
                    this.ProcessSend(Message, WriteEvent);
                }
            }
            else
            {
                WriteEvent = new SocketAsyncEventArgs();

                WriteEvent.SetBuffer(Message.ToBytes, Message.Offset, Message.Length + 7 - Message.Offset);

                WriteEvent.AcceptSocket = Message.Device.Socket;
                WriteEvent.RemoteEndPoint = Message.Device.Socket.RemoteEndPoint;

                if (!Message.Device.Socket.SendAsync(WriteEvent))
                {
                    this.ProcessSend(Message, WriteEvent);
                }
            }
        }

        internal void ProcessSend(Message Message, SocketAsyncEventArgs Args)
        {
            Message.Offset += Args.BytesTransferred;

            if (Message.Length + 7 > Message.Offset)
            {
                if (Message.Device.Connected)
                {
                    Args.SetBuffer(Message.Offset, Message.Length + 7 - Message.Offset);

                    if (!Message.Device.Socket.SendAsync(Args))
                    {
                        this.ProcessSend(Message, Args);
                    }
                }
            }
        }

        internal void OnSendCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            this.WritePool.Enqueue(AsyncEvent);
        }
    }
}

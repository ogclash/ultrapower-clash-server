using System.Net.Sockets;
using UCS.Logic;
using UCS.Helpers;
using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic.Enums;

namespace UCS.Packets
{
    internal class Device
    {
        internal Socket Socket;
        internal Level Player;
        internal Token Token;
        internal Crypto Keys;

        public Device(Socket so)
        {
            this.Socket = so;
            this.Keys = new Crypto();
            this.SocketHandle = so.Handle;
        }
        public Device(Socket so, Token token)
        {
            this.Socket = so;
            this.Keys = new Crypto();
            this.Token = token;
            this.SocketHandle = so.Handle;
        }


        internal State PlayerState = Logic.Enums.State.DISCONNECTED;

        internal IntPtr SocketHandle;

        internal string Interface;
        internal string AndroidID;
        internal string OpenUDID;
        internal string Model;
        internal string OSVersion;
        internal string MACAddress;
        internal string AdvertiseID;
        internal string VendorID;
        internal string IPAddress;
        internal string AttackInfo;
        internal bool NpcAttacked = false;
        internal int AttackedNpc;
        internal Level AttackVictim;
        internal bool ShieldInfo = false;

        internal uint ClientSeed;

        internal bool Connected => this.Socket.Connected && (!this.Socket.Poll(1000, SelectMode.SelectRead) || this.Socket.Available != 0);

        public bool IsClientSocketConnected()
        {
            try
            {
                return !((Socket.Poll(1000, SelectMode.SelectRead) && (Socket.Available == 0)) || !Socket.Connected);
            }
            catch
            {
                return false;
            }
        }

        internal void Process(byte[] Buffer)
        {
            if (Buffer.Length >= 7)
            {
                int[] _Header = new int[3];

                using (Reader Reader = new Reader(Buffer))
                {
                    _Header[0] = Reader.ReadUInt16(); // Message ID
                    Reader.Seek(1);
                    _Header[1] = Reader.ReadUInt16(); // Length
                    _Header[2] = Reader.ReadUInt16(); // Version

                    if (Buffer.Length - 7 >= _Header[1])
                    {
                        if (MessageFactory.Messages.ContainsKey(_Header[0]))
                        {
                            Message _Message =  Activator.CreateInstance(MessageFactory.Messages[_Header[0]], this, Reader) as Message;

                            _Message.Identifier = (ushort)_Header[0];
                            _Message.Length = (ushort)_Header[1];
                            _Message.Version = (ushort)_Header[2];

                            _Message.Reader = Reader;

                            try
                            {
                                Logger.Write($"Message { _Message.GetType().Name } ({_Header[0]}) is handled");

                                _Message.Decrypt();
                                _Message.Decode();
                                _Message.Process();
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            if (this.Model == null && this.OSVersion == null && this.Interface == null &&
                                Player == null)
                            {
                                Logger.Write("DDOS-Attempt");
                                try
                                {
                                    ResourcesManager.DropClient(this);
                                    this.Socket.Disconnect(true);
                                } catch (Exception){}
                                return;
                            }
                            Logger.Write($"Message { _Header[0] } is unhandled");
                            this.Keys.SNonce.Increment();
                        }

                        this.Token.Packet.RemoveRange(0, _Header[1] + 7);

                        if ((Buffer.Length - 7) - _Header[1] >= 7)
                        {
                            this.Process(Reader.ReadBytes((Buffer.Length - 7) - _Header[1]));
                        }
                        else
                        {
                           this.Token.Reset();
                        }
                    }
                }
            }
        }
    }
}

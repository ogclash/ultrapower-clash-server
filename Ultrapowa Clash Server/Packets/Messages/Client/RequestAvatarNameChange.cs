﻿using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14600
    internal class RequestAvatarNameChange : Message
    {
        public RequestAvatarNameChange(Device device, Reader reader) : base(device, reader)
        {
        }

        public string PlayerName { get; set; }

        public byte Unknown1 { get; set; }

        internal override void Decode()
        {
            this.PlayerName = this.Reader.ReadString();
        }

        internal override async void Process()
        {
            try
            {
                if (this.Device.Player.Avatar.SoftBan)
                {
                    new OwnHomeDataMessage(this.Device, this.Device.Player).Send();
                    return;
                }
                Level l = this.Device.Player;
                if (l != null)
                {
                    if (PlayerName.Length > 15)
                    {
                        ResourcesManager.DisconnectClient(Device);
                    }
                    else
                    {
                        l.Avatar.SetName(PlayerName);
                        AvatarNameChangeOkMessage p = new AvatarNameChangeOkMessage(l.Client) {AvatarName = PlayerName};
                        p.Send();
                    }
                }
            } catch (Exception) { }
        }
    }
}
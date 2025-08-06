using System;
using System.IO;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Logic.StreamEntry;
using UCS.Packets.Commands.Server;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14418
    internal class DeclineInviteMessage : Message
    {
        public DeclineInviteMessage(Device device, Reader reader) : base(device, reader)
        {
        }
        internal override void Decode()
        {
            var MessageID = this.Reader.ReadInt64();
            var userId    = this.Reader.ReadInt64();
            foreach (AvatarStreamEntry message in Device.Player.Avatar.messages)
            {
                if (message.ID == MessageID)
                {
                    Device.Player.Avatar.messages.Remove(message);
                    ResourcesManager.DisconnectClient(Device);
                    break;
                }
            }
        }
    }
}
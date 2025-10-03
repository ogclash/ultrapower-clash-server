using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 10105
    internal class AskForFriendListMessage : Message
    {
        public AskForFriendListMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            new FriendListMessage(this.Device).Send();
        }
    }
}
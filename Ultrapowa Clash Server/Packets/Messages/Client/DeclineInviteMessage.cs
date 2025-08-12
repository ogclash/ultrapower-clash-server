using UCS.Core;
using UCS.Helpers.Binary;
using UCS.Logic.AvatarStreamEntry;

namespace UCS.Packets.Messages.Client
{
    // Packet 14418
    internal class DeclineInviteMessage : Message
    {
        private long userId;
        private long MessageID;
        public DeclineInviteMessage(Device device, Reader reader) : base(device, reader)
        {
        }
        internal override void Decode()
        {
            this.MessageID = this.Reader.ReadInt64();
            this.userId    = this.Reader.ReadInt64();
        }

        internal override void Process()
        {
            foreach (AvatarStreamEntry message in Device.Player.Avatar.messages)
            {
                if (message.ID == this.MessageID)
                {
                    Device.Player.Avatar.messages.Remove(message);
                    ResourcesManager.DisconnectClient(Device);
                    break;
                }
            }
        }
    }
}
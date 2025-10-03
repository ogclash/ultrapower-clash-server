using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    internal class AddClashFriendMessage : Message
    {
        public AddClashFriendMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            this.FriendID = this.Reader.ReadInt64();
        }

        public long FriendID { get; set; }

        internal override void Process()
        {
            /*if (FriendID != null)
            {
                new Clash_Friend_Request_Sent_Message(this.Device, this.FriendID).Send();
            }
            else
            {*/
                new Friend_Request_Failed_Message(this.Device).Send();
            //}
        }
    }
}

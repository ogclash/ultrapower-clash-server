using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic.DataSlots;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14343
    internal class AddToBookmarkMessage : Message
    {
        public AddToBookmarkMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        private long id;

        internal override void Decode()
        {
            this.id = this.Reader.ReadInt64();
        }

        internal override void Process()
        {
            this.Device.Player.Avatar.BookmarkedClan.Add(new BookmarkSlot(id));;
            new BookmarkAddAllianceMessage(Device).Send();
        }
    }
}
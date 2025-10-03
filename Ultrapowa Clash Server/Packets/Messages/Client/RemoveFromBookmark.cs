using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic.DataSlots;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14344
    internal class RemoveFromBookmarkMessage : Message
    {
        public RemoveFromBookmarkMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        private long id;

        internal override void Decode()
        {
            this.id = this.Reader.ReadInt64();
        }

        internal override void Process()
        {
            BookmarkSlot al = this.Device.Player.Avatar.BookmarkedClan.Find(a => a.Value == id);
            if (al != null)
            {
                this.Device.Player.Avatar.BookmarkedClan.Remove(al);
            }
            new BookmarkRemoveAllianceMessage(Device).Send();
        } 
    }
}
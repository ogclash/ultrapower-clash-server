using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14341
    internal class AskForBookmarkMessage : Message
    {
        public AskForBookmarkMessage(Device device, Reader reader) : base(device, reader)
        {
        }


        internal override void Process()
        {
            new BookmarkListMessage(this.Device).Send();
        }
    }
}
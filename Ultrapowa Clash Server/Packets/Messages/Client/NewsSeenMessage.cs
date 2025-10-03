using UCS.Helpers.Binary;

namespace UCS.Packets.Messages.Client
{
    // Packet 10905
    internal class NewsSeenMessage : Message
    {
        public NewsSeenMessage(Device device, Reader reader) : base(device, reader)
        {

        }
    }
}

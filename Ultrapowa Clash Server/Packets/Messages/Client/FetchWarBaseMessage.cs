using UCS.Helpers.Binary;

namespace UCS.Packets.Messages.Client
{
    // Packet ?
    internal class FetchWarBaseMessage : Message
    {
        public FetchWarBaseMessage(Device device, Reader reader) : base(device, reader)
        {
        }
    }
}
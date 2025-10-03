using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14406
    internal class TopPreviousGlobalPlayersMessage : Message
    {
        public TopPreviousGlobalPlayersMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            new PreviousGlobalPlayersMessage(Device).Send();
        }
    }
}
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14403
    internal class TopGlobalPlayersMessage : Message
    {
        public TopGlobalPlayersMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            new GlobalPlayersMessage(this.Device).Send();
        }
    }
}

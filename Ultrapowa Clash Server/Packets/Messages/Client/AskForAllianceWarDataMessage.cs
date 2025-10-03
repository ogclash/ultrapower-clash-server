using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14331
    internal class AskForAllianceWarDataMessage : Message
    {
        public AskForAllianceWarDataMessage(Device client, Reader reader) : base(client, reader)
        {
        }


        internal override void Process()
        {
            new AllianceWarDataMessage(this.Device).Send();
        }
    }
}
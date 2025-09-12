using UCS.Packets.Messages.Client;

namespace UCS.Packets.Messages.Server
{
    // Packet 20108
    internal class KeepAliveOkMessage : Message
    {
        public KeepAliveOkMessage(Device client, KeepAliveMessage cka = null) : base(client)
        {
            this.Identifier = 20108;
        }
    }
}
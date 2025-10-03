using UCS.Helpers.List;

namespace UCS.Packets.Messages.Server
{
    // Packet 20161
    internal class ShutdownStartedMessage : Message
    {
        internal int Code;

        public ShutdownStartedMessage(Device client) : base(client)
        {
            this.Identifier = 20161;
        }

        internal override void Encode()
        {
            this.Data.AddInt(this.Code);
        }
    }
}

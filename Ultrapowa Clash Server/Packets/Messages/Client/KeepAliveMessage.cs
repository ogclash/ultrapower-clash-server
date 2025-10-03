using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 10108
    internal class KeepAliveMessage : Message
    {
        public KeepAliveMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
           new KeepAliveOkMessage(Device, this).Send();
        }
    }
}
using UCS.Core;
using UCS.Helpers.Binary;

namespace UCS.Packets.Messages.Client
{
    // Packet 15004
    internal class WatchAttackMessage : Message
    {
        public WatchAttackMessage(Device device, Reader reader) : base(device, reader)
        {
        }
        

        internal override void Decode()
        {
            ResourcesManager.DisconnectClient(this.Device);
        }

    }
}
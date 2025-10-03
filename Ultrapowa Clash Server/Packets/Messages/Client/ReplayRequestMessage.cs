using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14114
    internal class ReplayRequestMessage : Message
    {
        public ReplayRequestMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            this.Device.PlayerState = Logic.Enums.State.REPLAY;
            new ReplayData(this.Device).Send();
        }
    }
}
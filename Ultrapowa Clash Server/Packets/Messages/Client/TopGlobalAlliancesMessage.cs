using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14401
    internal class TopGlobalAlliancesMessage : Message
    {
        public int unknown { get; set; }

        public TopGlobalAlliancesMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            this.unknown = this.Reader.BaseStream.Length == 10 ? this.Reader.ReadBytes(10)[9] : this.Reader.Read();

        }

        internal override void Process()
        {
            if (unknown == 0)
                new GlobalAlliancesMessage(Device).Send();
            else
                new LocalAlliancesMessage(this.Device).Send();
        }
    }
}

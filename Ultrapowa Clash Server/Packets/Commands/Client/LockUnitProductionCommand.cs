using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    internal class LockUnitProductionCommand : Command
    {
        public int m_vBuildingId;
        public bool spells;

        public LockUnitProductionCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            var unknown1 = this.Reader.ReadInt32();
            var unknow2 = this.Reader.ReadInt32();
            var unknow3 = this.Reader.ReadUInt32();
            var unknow4 = this.Reader.ReadUInt32();
            var unknow5 = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
        }
    }
}
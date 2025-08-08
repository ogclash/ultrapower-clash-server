using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 572
    internal class ToggleHeroAttackModeCommand : Command
    {
        public ToggleHeroAttackModeCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            this.reader = reader;
        }

        internal override void Encode()
        {
            BuildingId = reader.ReadUInt32(); //buildingId - 0x1DCD6500;
            Unknown1 = reader.ReadByte();
            Unknown2 = reader.ReadUInt32();
            Unknown3 = reader.ReadUInt32();
        }

        internal override async void Process()
        {
            /*HeroData hd = (HeroData)herostate[i].Data;
            hd.AltModeFlying = true;
            hd.IsFlying = true;
            this.m_vHeroData.AltModeFlying = true;
            this.m_vHeroData.IsFlying = true;
            herostate[i].Data = hd;*/
        }

        private Reader reader;
        public uint BuildingId { get; set; }
        public byte Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
    }
}
using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 603
    internal class EndOfBattleCommand : Command
    {
        public EndOfBattleCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }
        
        internal override void Decode()
        {
            /*
            var unknown = this.Reader.ReadInt16();
            var unknown1 = this.Reader.ReadInt32();
            var unknown2 = this.Reader.ReadInt32();
            var unknown3 = this.Reader.ReadInt32();
            var unknown4 = this.Reader.ReadInt32();
            var unknown5 = this.Reader.ReadInt32();
            */

        }
        
    }
}
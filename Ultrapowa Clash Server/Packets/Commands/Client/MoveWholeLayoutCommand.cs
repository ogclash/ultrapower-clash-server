using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 501
    internal class MoveWholeLayoutCommand : Command
    {
        public MoveWholeLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
        }
        
        public int X;
        public int Y;
    }
}
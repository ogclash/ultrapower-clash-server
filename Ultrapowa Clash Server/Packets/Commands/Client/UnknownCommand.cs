using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 3072
    internal class UnknownCommand : Command
    {
        public static int Tick;
        public UnknownCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {

        }
        
        internal override void Decode()
        {
            Tick = Reader.ReadInt32();
            this.Reader.ReadData();
        }
    }
}
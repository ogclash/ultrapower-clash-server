using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 568
    internal class RemoveAllBuildingsFromLayoutCommand : Command
    {
        public int Tick;
        
        public RemoveAllBuildingsFromLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }
        
        internal override void Decode()
        {
            this.Reader.ReadData();
        }
    }
}
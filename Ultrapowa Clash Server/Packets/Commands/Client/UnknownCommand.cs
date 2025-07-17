using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 3072
    internal class UnknownCommand : Command
    {
        public UnknownCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {

        }

        public static int Tick;
        public static int Unknown1;
    }
}
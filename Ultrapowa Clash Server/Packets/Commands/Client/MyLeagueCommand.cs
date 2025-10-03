using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 538
    internal class MyLeagueCommand : Command
    {
        public MyLeagueCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {

        }

        internal override void Process()
        {
            //new LeaguePlayersMessage(level.Client).Send();
        }
    }
}

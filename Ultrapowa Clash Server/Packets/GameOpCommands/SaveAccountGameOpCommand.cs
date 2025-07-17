using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SaveAccountGameOpCommand : GameOpCommand
    {
        public SaveAccountGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(5);
        }

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                Resources.DatabaseManager.Save(level);
                var p = new GlobalChatLineMessage(level.Client)
                {
                    Message = "Game Successfuly Saved!",
                    HomeId = 0,
                    CurrentHomeId = 0,
                    LeagueId = 22,
                    PlayerName = "Server"
                };
                Processor.Send(p);
            }
            else
            {
                Resources.DatabaseManager.Save(level);
                var p = new GlobalChatLineMessage(level.Client)
                {
                    Message = "Game Successfuly Saved!",
                    HomeId = 0,
                    CurrentHomeId = 0,
                    LeagueId = 22,
                    PlayerName = "Server"
                };
                Processor.Send(p);
            }
        }

        readonly string[] m_vArgs;
    }
}

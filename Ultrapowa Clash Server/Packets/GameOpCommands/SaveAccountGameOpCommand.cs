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

        public override void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                ResourcesManager.DisconnectClient(level.Client);
                ResourcesManager.reloadPlayer(level);
            }
            else
            {
                ResourcesManager.DisconnectClient(level.Client);
                ResourcesManager.reloadPlayer(level);
            }
        }
        private void SendGlobalChatMessage(Level level, string message)
        {
            var p = new GlobalChatLineMessage(level.Client)
            {
                Message = message,
                HomeId = 0,
                CurrentHomeId = 0,
                LeagueId = 22,
                PlayerName = "Server"
            };
            p.Send();
        }

        readonly string[] m_vArgs;
    }
}

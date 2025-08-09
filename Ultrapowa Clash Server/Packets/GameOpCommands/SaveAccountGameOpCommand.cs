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
                /*
                if (m_vArgs.Length >= 2)
                {
                    long targetId = Convert.ToInt64(m_vArgs[1]);
                    var player = await ResourcesManager.GetPlayer(targetId);
                    if (player.Client != null)
                    {
                        ResourcesManager.DisconnectClient(level.Client);
                    }
                    ResourcesManager.reloadPlayer(level);
                    SendGlobalChatMessage(level, "Account was reloaded!");
                    return;
                }*/
                ResourcesManager.DisconnectClient(level.Client);
                ResourcesManager.reloadPlayer(level);
            }
            else
            {
                /*
                if (m_vArgs.Length >= 2)
                {
                    long targetId = Convert.ToInt64(m_vArgs[1]);
                    var player = await ResourcesManager.GetPlayer(targetId);
                    if (player.Client != null)
                    {
                        ResourcesManager.DisconnectClient(player.Client);
                    }
                    ResourcesManager.reloadPlayer(player);
                    SendGlobalChatMessage(level, "Account was reloaded!");
                    return;
                }*/
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

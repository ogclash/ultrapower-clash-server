using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class NewAccountGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public NewAccountGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            { 
                bool switched = level.Avatar.old_account != level.Avatar.UserId;
                if (m_vArgs.Length >= 1)
                {
                    if (switched)
                    {
                        Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                        oldplayer.Avatar.account_switch = 1;
                    }
                    else
                    {
                        level.Avatar.account_switch = 1;
                    }
                    ResourcesManager.DisconnectClient(level.Client);;
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
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
    }
}

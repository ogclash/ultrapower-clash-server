using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Core.Threading;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class ResetAccGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public ResetAccGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 1)
                {
                    if (level.Avatar.old_account == 0)
                    {
                        return;
                    }
                    Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                    Logger.Say("Account with id: "+oldplayer.Avatar.UserId+" was reset");
                    oldplayer.Avatar.account_switch = 0;
                    SendGlobalChatMessage(level, "Account with id: "+oldplayer.Avatar.UserId+" was reset!");
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

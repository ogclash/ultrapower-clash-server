using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Core.Threading;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SwichAccGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public SwichAccGameOpCommand(string[] args)
        {
            m_vArgs = args;
        }

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 3)
                {
                    long targetId = Convert.ToInt64(m_vArgs[1]);
                    var player = await ResourcesManager.GetPlayer(targetId);
                    if ((long)level.Avatar.old_account != level.Avatar.UserId)
                    {
                        SendGlobalChatMessage(level, "Account switching only works on main account, \ndo /resetacc first");
                        return;
                    }
                    if (targetId == level.Avatar.UserId)
                    {
                        SendGlobalChatMessage(level, "Cant switch to the same Account!");
                        return;
                    }
                    if (player.Avatar.account_password != "")
                    {
                        if (player.Avatar.account_password == m_vArgs[2])
                        {
                            level.Avatar.account_switch = (int)player.Avatar.UserId;
                            if (player.Client != null)
                                ResourcesManager.DisconnectClient(player.Client);
                            ResourcesManager.DisconnectClient(level.Client);
                        }
                        else
                            SendGlobalChatMessage(level, "Wrong password!");
                    }
                    else
                        SendGlobalChatMessage(level, "Account has no password!");
                }
                else if (m_vArgs.Length >= 1)
                {
                    SendGlobalChatMessage(level, "Usage: /switchacc <id> <password>");
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

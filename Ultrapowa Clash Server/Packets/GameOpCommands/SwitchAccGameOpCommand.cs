using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
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
            try {
                if (GetRequiredAccountPrivileges())
                {
                    bool switched = level.Avatar.old_account != level.Avatar.UserId;
                    if (m_vArgs.Length >= 3)
                    {
                        long targetId = Convert.ToInt64(m_vArgs[1]);
                        var player = await ResourcesManager.GetPlayer(targetId);
                        if (targetId == level.Avatar.UserId)
                        {
                            SendGlobalChatMessage(level, "Cant switch to the same Account!");
                            return;
                        }
                        if (player.Avatar.account_password != "")
                        {
                            if (player.Avatar.account_password == m_vArgs[2])
                            {
                                if (switched)
                                {
                                    Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                                    oldplayer.Avatar.account_switch = (int)player.Avatar.UserId;
                                }
                                else
                                {
                                    level.Avatar.account_switch = (int)player.Avatar.UserId;
                                }
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
                    else if (m_vArgs.Length >= 2)
                    {
                        int adminaccount=  0;
                        try {
                          adminaccount = Utils.ParseConfigInt("AdminAccount");
                        }catch (Exception){}
                        if (level.Avatar.UserId == adminaccount)
                        {
                            long targetId = Convert.ToInt64(m_vArgs[1]);
                            var player = await ResourcesManager.GetPlayer(targetId);
                            if (switched)
                            {
                                Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                                oldplayer.Avatar.account_switch = (int)player.Avatar.UserId;
                            }
                            else
                            {
                                level.Avatar.account_switch = (int)player.Avatar.UserId;
                            }
                            if (player.Client != null)
                                ResourcesManager.DisconnectClient(player.Client);
                            ResourcesManager.DisconnectClient(level.Client);
                        }
                        else
                            SendGlobalChatMessage(level, "Usage: /switchacc <id> <password>");
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
            } catch (Exception) {}
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

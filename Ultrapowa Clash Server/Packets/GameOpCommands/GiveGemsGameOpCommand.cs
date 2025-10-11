using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.GameOpCommands
{
    internal class GiveGemsOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public GiveGemsOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }
        
        

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() && level.Avatar.m_vTownHallLevel >= 5 && level.Avatar.m_vAccountCreationDate < DateTime.Now.AddMonths(-1))
            {
                try
                {
                    if (m_vArgs.Length >= 3)
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (l != null && l.Client != null)
                        {
                            if (level.Avatar.HasEnoughDiamonds(Convert.ToInt32(m_vArgs[2])))
                                level.Avatar.UseDiamonds(Convert.ToInt32(m_vArgs[2]));
                            else
                                return;
                            l.Avatar.AddDiamonds(Convert.ToInt32(m_vArgs[2]));
                            GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                            _MSG.PlayerName = "Server";
                            _MSG.LeagueId = 22;
                            _MSG.Message = "Gifted " + Convert.ToInt32(m_vArgs[2]) + " gem(s) to " + l.Avatar.AvatarName;
                            _MSG.Send();
                            
                            GlobalChatLineMessage MSG = new GlobalChatLineMessage(l.Client);
                            MSG.PlayerName = "Server";
                            MSG.LeagueId = 22;
                            MSG.Message = "You got gifted " + Convert.ToInt32(m_vArgs[2]) + " gem(s)! by " + level.Avatar.AvatarName;
                            MSG.Send();
                            if (l.Avatar.minorversion >= 551)
                                new OwnHomeDataMessage(l.Client, l).Send();
                            else
                                new OwnHomeDataForOldClients(l.Client, l).Send();
                            if (level.Avatar.minorversion >= 551)
                                new OwnHomeDataMessage(level.Client, level).Send();
                            else
                                new OwnHomeDataForOldClients(level.Client, level).Send();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client, "Requires Townhall 6 & Account to be atleast 1 month old");
            }
        }
    }
}
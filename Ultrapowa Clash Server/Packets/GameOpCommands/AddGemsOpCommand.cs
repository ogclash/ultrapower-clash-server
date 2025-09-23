using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.GameOpCommands
{
    internal class AddGemsOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public AddGemsOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }
        
        

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                try
                {
                    if (m_vArgs.Length >= 3)
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (l != null)
                        {
                            l.Avatar.AddDiamonds(Convert.ToInt32(m_vArgs[2]));
                            GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                            _MSG.PlayerName = "Server";
                            _MSG.LeagueId = 22;
                            _MSG.Message = "Added " + Convert.ToInt32(m_vArgs[2]) + " gem(s)";
                            _MSG.Send();
                        }
                    }
                    else
                    {
                        level.Avatar.AddDiamonds(Convert.ToInt32(m_vArgs[1]));
                        GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                        _MSG.PlayerName = "Server";
                        _MSG.LeagueId = 22;
                        _MSG.Message = "Added " + Convert.ToInt32(m_vArgs[1]) + " gem(s)";
                        _MSG.Send();
                    }
                    if (level.Avatar.minorversion >= 709)
                        new OwnHomeDataMessage(level.Client, level).Send();
                    else
                        new OwnHomeDataForOldClients(level.Client, level).Send();
                }
                catch (Exception)
                {
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
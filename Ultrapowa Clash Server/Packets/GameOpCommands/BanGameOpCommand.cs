using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class BanGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public BanGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(5);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                if (m_vArgs.Length >= 2)
                {
                    try
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (l != null)
                        {
                            if (l.Avatar.AccountPrivileges < level.Avatar.AccountPrivileges)
                            {
                                l.Avatar.AccountBanned = true;
                                l.Avatar.AccountPrivileges = 0;
                                if (ResourcesManager.IsPlayerOnline(l))
                                {
                                    Processor.Send(new OutOfSyncMessage(l.Client));
                                }
                            }
                        }
                    }
                    catch 
                    {
                    }
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}

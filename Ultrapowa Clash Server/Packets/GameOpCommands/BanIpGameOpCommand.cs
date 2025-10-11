using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Core.Settings;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class BanIpGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public BanIpGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
                if (m_vArgs.Length >= 1)
                    try
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (l != null)
                            if (l.Avatar.AccountPrivileges < level.Avatar.AccountPrivileges)
                            {
                                l.Avatar.SoftBan = true;
                                l.Avatar.AccountBanned = false;
                                l.Avatar.m_vNameChangingLeft = 0;
                            }
                    }
                    catch 
                    {
                    }
                else
                    SendCommandFailedMessage(level.Client);
        }
    }
}

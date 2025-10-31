using System;
using UCS.Core;
using UCS.Core.Settings;
using UCS.Logic;

namespace UCS.Packets.GameOpCommands
{
    internal class SoftBanGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public SoftBanGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Constants.SuperAdmin)
                if (m_vArgs.Length > 1)
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

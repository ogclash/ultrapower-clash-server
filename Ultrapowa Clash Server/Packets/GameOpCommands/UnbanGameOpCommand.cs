using System;
using UCS.Core;
using UCS.Helpers;
using UCS.Logic;

namespace UCS.Packets.GameOpCommands
{
    internal class UnbanGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public UnbanGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(2);
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
                            l.Avatar.AccountBanned = false;
                            l.Avatar.SoftBan = false;
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

using System.Collections.Generic;
using UCS.Core;
using UCS.Helpers;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;

namespace UCS.Packets.GameOpCommands
{
    internal class ClearInboxGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public ClearInboxGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                level.Avatar.messages = new List<AvatarStreamEntry>();
                ResourcesManager.DisconnectClient(level.Client);
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
using System.Collections.Generic;
using UCS.Core;
using UCS.Core.Settings;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;

namespace UCS.Packets.GameOpCommands
{
    internal class ClearInboxGameOpCommand : GameOpCommand
    {

        public ClearInboxGameOpCommand(string[] args)
        {
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Constants.SuperAdmin)
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
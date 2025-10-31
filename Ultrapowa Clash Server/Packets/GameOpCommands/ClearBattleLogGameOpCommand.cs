using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Core.Settings;
using UCS.Logic;

namespace UCS.Packets.GameOpCommands
{
    internal class ClearBattleLogGameOpCommand : GameOpCommand
    {
        public ClearBattleLogGameOpCommand(string[] args)
        {
            SetRequiredAccountPrivileges(1);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Constants.SuperAdmin)
            {
                level.Avatar.battles = new List<JObject>();
                ResourcesManager.DisconnectClient(level.Client);
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
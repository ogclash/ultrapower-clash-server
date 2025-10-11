using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Helpers;
using UCS.Logic;

namespace UCS.Packets.GameOpCommands
{
    internal class ClearBattleLogGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public ClearBattleLogGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
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
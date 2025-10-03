using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UCS.Core;
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
            if (GetRequiredAccountPrivileges())
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
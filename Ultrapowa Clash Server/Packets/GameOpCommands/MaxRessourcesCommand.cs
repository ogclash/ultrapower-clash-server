using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.GameOpCommands
{
    internal class MaxRessourcesCommand : GameOpCommand
    {
        public MaxRessourcesCommand(string[] Args)
        {
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                var p = level.Avatar;
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"), 999999999);
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"), 999999999);
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("DarkElixir"), 999999999);
                p.m_vCurrentGems = 999999999;
                if (level.Avatar.minorversion >= 709)
                    new OwnHomeDataMessage(level.Client, level).Send();
                else
                    new OwnHomeDataForOldClients(level.Client, level).Send();
            }
            else
                SendCommandFailedMessage(level.Client);
        }
    }
}

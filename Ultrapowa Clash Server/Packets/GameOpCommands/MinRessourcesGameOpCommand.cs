using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.GameOpCommands
{
    internal class MinRessourcesCommand : GameOpCommand
    {
        public MinRessourcesCommand(string[] Args)
        {
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                ClientAvatar p = level.Avatar;
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"), 1000);
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"), 1000);
                p.SetResourceCount(CSVManager.DataTables.GetResourceByName("DarkElixir"), 100);
                p.m_vCurrentGems = 200;
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

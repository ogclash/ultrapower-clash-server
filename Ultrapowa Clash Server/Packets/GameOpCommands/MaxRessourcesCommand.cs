using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

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
                Processor.Send(new OwnHomeDataMessage(level.Client, level));
            }
            else
                SendCommandFailedMessage(level.Client);
        }
    }
}

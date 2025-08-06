using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SaveAccountGameOpCommand : GameOpCommand
    {
        public SaveAccountGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(5);
        }

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                ResourcesManager.DisconnectClient(level.Client);
                ResourcesManager.reloadPlayer(level);
            }
            else
            {
                ResourcesManager.DisconnectClient(level.Client);
                ResourcesManager.reloadPlayer(level);
            }
        }

        readonly string[] m_vArgs;
    }
}

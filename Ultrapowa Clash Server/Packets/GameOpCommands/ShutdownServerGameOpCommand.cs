using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class ShutdownServerGameOpCommand : GameOpCommand
    {
        string[] m_vArgs;

        public ShutdownServerGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(10);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                foreach (var onlinePlayer in ResourcesManager.m_vOnlinePlayers)
                {
                    var p = new ShutdownStartedMessage(onlinePlayer.Client) {Code = 5};
                    p.Send();
                    //Task.Delay(300000).ContinueWith(_ => UCSControl.UCSRestart());
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}

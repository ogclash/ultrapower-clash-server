using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class GetIdGameopCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public GetIdGameopCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                if (m_vArgs.Length >= 1)
                {
                    GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                    _MSG.PlayerName = "Server";
                    _MSG.LeagueId = 22;
                    _MSG.Message = "Your ID: " + level.Avatar.UserId;
                    _MSG.Send();
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
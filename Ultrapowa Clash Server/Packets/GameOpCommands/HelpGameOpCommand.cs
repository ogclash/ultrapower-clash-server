using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class HelpGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public HelpGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 1)
                {
                    ClientAvatar avatar = level.Avatar;
                    GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                    _MSG.PlayerName = "Server";
                    _MSG.LeagueId = 22;
                    _MSG.Message = "Your ID: " + level.Avatar.UserId;
                    _MSG.Message = @"/help" +
                                   "\n/status" +
                                   "\n/visit" +
                                   "\n/min" +
                                   "\n/max" +
                                   "\n/accinfo" +
                                   "\n/rename " +
                                   "\n/resetacc" +
                                   "\n/clearbattles" +
                                   "\n/clearinbox" +
                                   "\n/setpassword <password>" +
                                   "\n/switchacc <id> <password>" +
                                   "\n/addgems <amount>";

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

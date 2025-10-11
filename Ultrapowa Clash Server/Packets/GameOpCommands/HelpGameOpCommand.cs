using UCS.Core.Network;
using UCS.Core.Settings;
using UCS.Helpers;
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
            bool superadmin = level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount");
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || superadmin)
            {
                if (m_vArgs.Length >= 1)
                {
                    GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                    _MSG.PlayerName = "Server";
                    _MSG.LeagueId = 22;
                    _MSG.Message = @"/help" +
                                   "\n/visit" +
                                   "\n/accinfo" +
                                   "\n/resetacc" +
                                   "\n/clearbattles" +
                                   "\n/clearinbox" +
                                   "\n/newaccount" +
                                   "\n/setpassword <password>" +
                                   "\n/switchacc <id> <password>";
                    if (Constants.DeveloperBuild)
                    {
                        _MSG.Message += "\n/addgems <amount>" +
                                        "\n/min" +
                                        "\n/max";
                    }
                    if (level.Avatar.AccountPrivileges >= 1 ||superadmin)
                    {
                        _MSG.Message += "\n/rename " +
                                        "\n/softban " +
                                        "\n/status";
                        if (Constants.DeveloperBuild)
                            _MSG.Message += "\n/reload ";
                    }
                    if (level.Avatar.AccountPrivileges >= 2 ||superadmin)
                    {
                        _MSG.Message += "\n/kick " +
                                        "\n/unban";
                    }
                    if (level.Avatar.AccountPrivileges >= 5||superadmin)
                    {
                        _MSG.Message += "\n/ban " +
                                        "\n/op";
                    }
                    if (Constants.DeveloperBuild && (level.Avatar.AccountPrivileges >= 10 ||superadmin))
                    {
                        _MSG.Message += "\n/sysmsg " +
                                        "\n/becomeleader";
                    }
                    _MSG.Message += "\n/givegems <id> <amount> [Requires Townhall 6 & Account to be atleast 1 month old]";
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

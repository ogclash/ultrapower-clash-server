using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SetPasswordGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public SetPasswordGameOpCommand(string[] args)
        {
            m_vArgs = args;
        }

        public override async void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 3)
                {
                    if (level.Avatar.account_password != "")
                    {
                        if (level.Avatar.account_password == m_vArgs[1])
                        {
                            level.Avatar.account_password = m_vArgs[2];
                            SendGlobalChatMessage(level, "Your Password was updated");
                        }
                        else
                            SendGlobalChatMessage(level, "Wrong password!");
                    }
                    else
                    {
                        SendGlobalChatMessage(level, "Account has no password!");
                        SendGlobalChatMessage(level, "Usage: /setpassword <password>");
                    }
                }
                else if (m_vArgs.Length >= 2)
                {
                    if (level.Avatar.account_password == "")
                    {
                        level.Avatar.account_password = m_vArgs[1];
                        SendGlobalChatMessage(level, "Your Password is set and ready for acc-switching!");
                    }
                    else
                    {
                        SendGlobalChatMessage(level, "There is already a password, you need to update it");
                        SendGlobalChatMessage(level, "Usage: /setpassword <oldpassword> <newpassword>");
                    }
                }
                else if (m_vArgs.Length >= 1)
                {
                    SendGlobalChatMessage(level, "Usage: /setpassword <password>");
                    SendGlobalChatMessage(level, "Usage: /setpassword <oldpassword> <newpassword>");
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
        
        private void SendGlobalChatMessage(Level level, string message)
        {
            var p = new GlobalChatLineMessage(level.Client)
            {
                Message = message,
                HomeId = 0,
                CurrentHomeId = 0,
                LeagueId = 22,
                PlayerName = "Server"
            };
            p.Send();
        }
    }
}
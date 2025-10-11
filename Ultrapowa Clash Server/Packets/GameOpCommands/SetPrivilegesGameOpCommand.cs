using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SetPrivilegesGameOpCommand : GameOpCommand
    {
        private readonly string[] m_vArgs;

        public SetPrivilegesGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(5); // Requires privilege level 4 to execute
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.UserId != Utils.ParseConfigInt("AdminAccount"))
            {
                if (level.Avatar.AccountPrivileges < GetRequiredAccountPrivileges())
                {
                    SendCommandFailedMessage(level.Client, "Insufficient privileges.");
                    return;
                }
            }

            if (m_vArgs.Length < 2) // If no arguments are provided, display the help menu
            {
                DisplayHelpMenu(level);
                return;
            }

            if (m_vArgs.Length < 3)
            {
                Logger.Write("SetPrivileges command failed: Insufficient arguments.");
                SendCommandFailedMessage(level.Client, "Insufficient arguments.");
                return;
            }

            try
            {
                long targetId = Convert.ToInt64(m_vArgs[1]);
                byte newPrivileges = Convert.ToByte(m_vArgs[2]);

                if (level.Avatar.UserId != Utils.ParseConfigInt("AdminAccount") && newPrivileges >= level.Avatar.AccountPrivileges)
                {
                    Logger.Write("SetPrivileges command failed: Cannot assign privileges equal to or higher than the executor's.");
                    SendCommandFailedMessage(level.Client, "Cannot assign privileges equal to or higher than your own.");
                    return;
                }

                var targetLevel = await ResourcesManager.GetPlayer(targetId);
                if (targetLevel != null)
                {
                    targetLevel.Avatar.AccountPrivileges = newPrivileges;
                    Logger.Write($"SetPrivileges command succeeded: User ID {targetId} privileges set to {newPrivileges}.");
                    SendGlobalChatMessage(level, $"SetPrivileges command succeeded: User ID {targetId} privileges set to {newPrivileges}.");
                }
                else
                {
                    Logger.Write($"SetPrivileges command failed: User ID {targetId} not found.");
                    SendCommandFailedMessage(level.Client, $"User ID {targetId} not found.");
                }
            }
            catch (FormatException)
            {
                Logger.Write("SetPrivileges command failed: Invalid argument format.");
                SendCommandFailedMessage(level.Client, "Invalid argument format.");
            }
            catch (Exception ex)
            {
                Logger.Write($"SetPrivileges command failed with error: {ex.Message}");
                SendCommandFailedMessage(level.Client, $"failed with error: {ex.Message}");
            }
        }

        private void DisplayHelpMenu(Level level)
        {
            string helpMessage = "Usage: /op <PlayerID> <Level>\n" +
                                 "Options:\n" +
                                 "  Level:    The new privilege level (0-5).\n" +
                                 "Example:\n" +
                                 "  /op 123456789 3";

            Logger.Write("Displaying help menu for SetPrivileges command.");
            var p = new GlobalChatLineMessage(level.Client)
            {
                Message = helpMessage,
                HomeId = 0,
                CurrentHomeId = 0,
                LeagueId = 22,
                PlayerName = "Server"
            };
            p.Send();
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

using System;
using UCS.Core;
using UCS.Core.Network;
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
            SetRequiredAccountPrivileges(4); // Requires privilege level 4 to execute
        }

        public override async void Execute(Level level)
        {
            if (!GetRequiredAccountPrivileges())
            {
                SendCommandFailedMessage(level.Client);
                SendGlobalChatMessage(level, "SetPrivileges command failed: Insufficient privileges.");
                return;
            }

            if (m_vArgs.Length < 2) // If no arguments are provided, display the help menu
            {
                DisplayHelpMenu(level);
                return;
            }

            if (m_vArgs.Length < 3)
            {
                Logger.Write("SetPrivileges command failed: Insufficient arguments.");
                SendCommandFailedMessage(level.Client);
                SendGlobalChatMessage(level, "SetPrivileges command failed: Insufficient arguments.");
                return;
            }

            try
            {
                long targetId = Convert.ToInt64(m_vArgs[1]);
                byte newPrivileges = Convert.ToByte(m_vArgs[2]);

                if (newPrivileges >= level.Avatar.AccountPrivileges)
                {
                    Logger.Write("SetPrivileges command failed: Cannot assign privileges equal to or higher than the executor's.");
                    SendCommandFailedMessage(level.Client);
                    SendGlobalChatMessage(level, "SetPrivileges command failed: Cannot assign privileges equal to or higher than your own.");
                    //return;
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
                    SendCommandFailedMessage(level.Client);
                    SendGlobalChatMessage(level, $"SetPrivileges command failed: User ID {targetId} not found.");
                }
            }
            catch (FormatException)
            {
                Logger.Write("SetPrivileges command failed: Invalid argument format.");
                SendCommandFailedMessage(level.Client);
                SendGlobalChatMessage(level, "SetPrivileges command failed: Invalid argument format.");
            }
            catch (Exception ex)
            {
                Logger.Write($"SetPrivileges command failed with error: {ex.Message}");
                SendCommandFailedMessage(level.Client);
                SendGlobalChatMessage(level, $"SetPrivileges command failed with error: {ex.Message}");
            }
        }

        private void DisplayHelpMenu(Level level)
        {
            string helpMessage = "Usage: /setprivileges <PlayerID> <PrivilegeLevel>\n" +
                                 "Options:\n" +
                                 "  <PlayerID>        The ID of the player whose privileges you want to set.\n" +
                                 "  <PrivilegeLevel>  The new privilege level (0-5).\n" +
                                 "Example:\n" +
                                 "  /setprivileges 123456789 3";

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
            var chatMessage = new GlobalChatLineMessage(level.Client)
            {
                Message = message,
                HomeId = level.Avatar.UserId,
                CurrentHomeId = level.Avatar.UserId,
                LeagueId = level.Avatar.m_vLeagueId,
                PlayerName = level.Avatar.AvatarName
            };
            chatMessage.Send();
        }
    }
}

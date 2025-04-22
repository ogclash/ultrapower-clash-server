using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class ToggleBroadcastGameOpCommand : GameOpCommand
    {
        private readonly string[] m_vArgs;

        public ToggleBroadcastGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(4); // Requires admin privileges
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 2)
                {
                    string option = m_vArgs[1].ToLower();
                    string message;

                    if (option == "on")
                    {
                        BroadcastManager.StartBroadcast(level);
                        Logger.Write("Broadcast system enabled.");
                        message = "Broadcast system has been successfully enabled.";
                    }
                    else if (option == "off")
                    {
                        BroadcastManager.StopBroadcast(level);
                        Logger.Write("Broadcast system disabled.");
                        message = "Broadcast system has been successfully disabled.";
                    }
                    else
                    {
                        message = "Invalid option. Use 'on', 'off', or 'h' for help.";
                    }

                    // Construct and send the mail message
                    var avatar = level.Avatar;
                    AllianceMailStreamEntry mail = new AllianceMailStreamEntry
                    {
                        SenderId = avatar.UserId,
                        m_vSenderName = avatar.AvatarName,
                        IsNew = 2,
                        AllianceId = 0,
                        AllianceBadgeData = 1526735450,
                        AllianceName = "Broadcast System Status",
                        Message = message,
                        m_vSenderLevel = avatar.m_vAvatarLevel,
                        m_vSenderLeagueId = avatar.m_vLeagueId
                    };

                    AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                    p.SetAvatarStreamEntry(mail);
                    Processor.Send(p);
                }
                else
                {
                    DisplayHelpMenu(level);
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);

                // Construct and send the failure mail message
                var avatar = level.Avatar;
                AllianceMailStreamEntry mail = new AllianceMailStreamEntry
                {
                    SenderId = avatar.UserId,
                    m_vSenderName = avatar.AvatarName,
                    IsNew = 2,
                    AllianceId = 0,
                    AllianceBadgeData = 1526735450,
                    AllianceName = "Broadcast Command Failed",
                    Message = "You do not have sufficient privileges to execute this command.",
                    m_vSenderLevel = avatar.m_vAvatarLevel,
                    m_vSenderLeagueId = avatar.m_vLeagueId
                };

                AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                p.SetAvatarStreamEntry(mail);
                Processor.Send(p);
            }
        }

        private void DisplayHelpMenu(Level level)
        {
            string helpMessage = "Usage: /togglebroadcast <on|off|h>\n" +
                                 "Options:\n" +
                                 "  on   - Enable the broadcast system.\n" +
                                 "  off  - Disable the broadcast system.\n" +
                                 "  h    - Display this help menu.\n" +
                                 "Example:\n" +
                                 "  /togglebroadcast on";

            Logger.Write("Displaying help menu for ToggleBroadcast command.");

            // Construct and send the help mail message
            ClientAvatar avatar = level.Avatar;
            var mail = new AllianceMailStreamEntry();
            mail.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            mail.SetSender(avatar);
            mail.IsNew = 2;
            mail.AllianceId = 0;
            mail.AllianceBadgeData = 1526735450;
            mail.AllianceName = "UCS Broadcast Commands Help";
            mail.Message = helpMessage;


            var p = new AvatarStreamEntryMessage(level.Client);
            p.SetAvatarStreamEntry(mail);
            Processor.Send(p);

        }
    }
}
/* var avatar = level.Avatar;
            AllianceMailStreamEntry mail = new AllianceMailStreamEntry
            {
                SenderId = avatar.UserId,
                m_vSenderName = avatar.AvatarName,
                IsNew = 2,
                AllianceId = 0,
                AllianceBadgeData = 1526735450,
                AllianceName = "Broadcast Command Help",
                Message = helpMessage,
                m_vSenderLevel = avatar.m_vAvatarLevel,
                m_vSenderLeagueId = avatar.m_vLeagueId
            };

            AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
            p.SetAvatarStreamEntry(mail);
            Processor.Send(p);
        }*/
using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SystemMessageGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public SystemMessageGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(10);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                if (m_vArgs.Length >= 1)
                {
                    string message = string.Join(" ", m_vArgs.Skip(1));
                    ClientAvatar avatar = level.Avatar;
                    var mail = new AllianceMailStreamEntry();
                    mail.ID = (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    mail.SetSender(avatar);
                    mail.IsNew = 2;
                    mail.AllianceId = 0;
                    mail.AllianceBadgeData = 1526735450;
                    mail.AllianceName = "Administrator";
                    mail.Message = message;

                    foreach (var onlinePlayer in ResourcesManager.m_vOnlinePlayers)
                    {
                        var p = new AvatarStreamEntryMessage(onlinePlayer.Client);
                        p.SetAvatarStreamEntry(mail);
                        Processor.Send(p);
                    }
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}

using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;
using UCS.Packets.Commands.Server;
using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 543
    internal class KickAllianceMemberCommand : Command
    {
        public KickAllianceMemberCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.m_vAvatarId = this.Reader.ReadInt64();
            this.Reader.ReadByte();
            this.m_vMessage = this.Reader.ReadString();
            this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                var targetAccount = await ResourcesManager.GetPlayer(m_vAvatarId);
                if (targetAccount != null)
                {
                    var targetAvatar = targetAccount.Avatar;
                    var targetAllianceId = targetAvatar.AllianceId;
                    var requesterAvatar = this.Device.Player.Avatar;
                    var requesterAllianceId = requesterAvatar.AllianceId;
                    if (requesterAllianceId > 0 && targetAllianceId == requesterAllianceId)
                    {
                        var alliance = ObjectManager.GetAlliance(requesterAllianceId);
                        var requesterMember = alliance.m_vAllianceMembers[requesterAvatar.UserId];
                        var targetMember = alliance.m_vAllianceMembers[m_vAvatarId];
                        if (targetMember.HasLowerRoleThan(requesterMember.Role))
                        {
                            targetAvatar.AllianceId = 0;
                            alliance.RemoveMember(m_vAvatarId);
                            if (ResourcesManager.IsPlayerOnline(targetAccount))
                            {
                                var leaveAllianceCommand = new LeavedAllianceCommand(this.Device);
                                leaveAllianceCommand.SetAlliance(alliance);
                                leaveAllianceCommand.SetReason(2); //Kick
                                new AvailableServerCommandMessage(targetAccount.Client, leaveAllianceCommand.Handle()).Send();

                                var kickOutStreamEntry = new AllianceKickOutStreamEntry();
                                kickOutStreamEntry.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                kickOutStreamEntry.SetSender(requesterAvatar);
                                kickOutStreamEntry.IsNew = 2;
                                kickOutStreamEntry.SetAllianceId(alliance.m_vAllianceId);
                                kickOutStreamEntry.SetAllianceBadgeData(alliance.m_vAllianceBadgeData);
                                kickOutStreamEntry.SetAllianceName(alliance.m_vAllianceName);
                                kickOutStreamEntry.SetMessage(m_vMessage);

                                var p = new AvatarStreamEntryMessage(targetAccount.Client);
                                p.SetTargetAcc(targetAccount);
                                p.SetAvatarStreamEntry(kickOutStreamEntry);
                                p.Send();
                            }

                            var eventStreamEntry = new AllianceEventStreamEntry();
                            eventStreamEntry.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                            eventStreamEntry.SetSender(targetAvatar);
                            eventStreamEntry.m_vAvatarName = this.Device.Player.Avatar.AvatarName;
                            eventStreamEntry.EventType = 1;
                            alliance.AddChatMessage(eventStreamEntry);

                            StreamEntry oldmessage = alliance.m_vChatMessages.Find(c => c.SenderID == m_vAvatarId && c.m_vType == 1);
                            alliance.bannedPlayers.Add(m_vAvatarId);
                            foreach (AllianceMemberEntry op in alliance.GetAllianceMembers())
                            {
                                Level alliancemembers = await ResourcesManager.GetPlayer(op.AvatarId);
                                if (alliancemembers.Client != null)
                                {
                                    if (oldmessage != null && oldmessage.m_vSenderName == targetAccount.Avatar.AvatarName)
                                    {
                                        new AllianceStreamEntryRemovedMessage(alliancemembers.Client, oldmessage.ID).Send();
                                    }
                                    new AllianceStreamEntryMessage(alliancemembers.Client) {StreamEntry = eventStreamEntry}.Send();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        internal long m_vAvatarId;
        internal string m_vMessage;
    }
}

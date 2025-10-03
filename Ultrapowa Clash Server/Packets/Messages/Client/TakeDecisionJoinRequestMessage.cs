using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Logic.StreamEntry;
using UCS.Packets.Commands.Server;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14321
    internal class TakeDecisionJoinRequestMessage : Message
    {
        public TakeDecisionJoinRequestMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public long MessageID { get; set; }

        public byte Choice { get; set; }

        internal override void Decode()
        {
            this.MessageID = this.Reader.ReadInt64();
            this.Choice    = this.Reader.ReadByte();
        }

        internal async override void Process()
        {
            try
            {
                Alliance a = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                StreamEntry message = a.m_vChatMessages.Find(c => c.ID == MessageID);
                Level requester = await ResourcesManager.GetPlayer(message.SenderID);
                if (Choice == 1)
                {
                    if (requester.Avatar.AllianceId != 0)
                    {
                        return;
                    }
                    if (!a.IsAllianceFull())
                    {
                        requester.Avatar.AllianceId = a.m_vAllianceId;

                        AllianceMemberEntry member = new AllianceMemberEntry(requester.Avatar.UserId);
                        member.Role = 1;
                        a.AddAllianceMember(member);

                        StreamEntry e = a.m_vChatMessages.Find(c => c.ID == MessageID);
                        e.SetJudgeName(this.Device.Player.Avatar.AvatarName);
                        e.SetState(2);

                        AllianceEventStreamEntry eventStreamEntry = new AllianceEventStreamEntry();
                        eventStreamEntry.ID = a.m_vChatMessages.Count > 0 ? a.m_vChatMessages.Last().ID + 1 : 1;
                        eventStreamEntry.SetSender(requester.Avatar);
                        eventStreamEntry.EventType = 2;
                        eventStreamEntry.m_vAvatarName = "Clan Member";
                        eventStreamEntry.m_vAvatarId = eventStreamEntry.SenderID;
                        a.AddChatMessage(eventStreamEntry);

                        foreach (AllianceMemberEntry op in a.GetAllianceMembers())
                        {
                            Level player = await ResourcesManager.GetPlayer(op.AvatarId);
                            if (player.Client != null)
                            {
                                new AllianceStreamEntryMessage(player.Client) { StreamEntry = eventStreamEntry }.Send();
                                new AllianceStreamEntryMessage(player.Client) { StreamEntry = e }.Send();
                            }
                        }
                        if (ResourcesManager.IsPlayerOnline(requester))
                        {
                            JoinedAllianceCommand joinAllianceCommand = new JoinedAllianceCommand(requester.Client);
                            joinAllianceCommand.SetAlliance(a);

                            new AvailableServerCommandMessage(requester.Client, joinAllianceCommand.Handle()).Send();

                            AllianceRoleUpdateCommand d = new AllianceRoleUpdateCommand(requester.Client);
                            d.SetAlliance(a);
                            d.SetRole(4);
                            d.Tick(requester);

                            new AvailableServerCommandMessage(requester.Client, d.Handle()).Send();

                            new AllianceStreamMessage(requester.Client, a).Send();
                            requester.Avatar.SendCLanMessagesToOldClient(requester.Client);
                        }
                    }
                }
                else
                {
                    StreamEntry e = a.m_vChatMessages.Find(c => c.ID == MessageID);
                    e.SetJudgeName(this.Device.Player.Avatar.AvatarName);
                    e.SetState(3);

                    foreach (AllianceMemberEntry op in a.GetAllianceMembers())
                    {
                        Level player = await ResourcesManager.GetPlayer(op.AvatarId);
                        if (player.Client != null)
                        {
                            new AllianceStreamEntryMessage(player.Client) { StreamEntry = e }.Send();
                        }
                    }
                    var alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                    var allianceDeclineMessage = new AllianceDeclineStreamEntry();
                    allianceDeclineMessage.SetSender(Device.Player.Avatar);
                    allianceDeclineMessage.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    allianceDeclineMessage.SenderId = Device.Player.Avatar.UserId;
                    allianceDeclineMessage.IsNew = 2;
                    allianceDeclineMessage.AllianceId = (alliance.m_vAllianceId);
                    allianceDeclineMessage.AllianceBadgeData = (alliance.m_vAllianceBadgeData);
                    allianceDeclineMessage.AllianceName = (alliance.m_vAllianceName);
                    var p = new AvatarStreamEntryMessage(requester.Client);
                    p.SetAvatarStreamEntry(allianceDeclineMessage);
                    p.SetTargetAcc(requester);
                    p.Send();
                }
            } catch (Exception) { }
        }

    }
}
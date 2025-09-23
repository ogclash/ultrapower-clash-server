using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Commands.Server;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14323
    internal class AcceptInviteMessage : Message
    {
        public AcceptInviteMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public long MessageID { get; set; }

        public long userId { get; set; }

        internal override void Decode()
        {
            this.MessageID = this.Reader.ReadInt64();
            this.userId    = this.Reader.ReadInt64();
        }

        internal async override void Process()
        {
            try
            {
                var castle = Device.Player.Avatar.GetAllianceCastleLevel();
                if (castle != -1)
                {
                    Level inviter = await ResourcesManager.GetPlayer(userId);
                    Alliance alliance = ObjectManager.GetAlliance(inviter.Avatar.AllianceId);
                    if (alliance != null && !alliance.IsAllianceFull())
                    {
                        this.Device.Player.Avatar.AllianceId = alliance.m_vAllianceId;
                        AllianceMemberEntry member = new AllianceMemberEntry(this.Device.Player.Avatar.UserId);
                        member.Role = 1;
                        alliance.AddAllianceMember(member);

                        JoinedAllianceCommand b = new JoinedAllianceCommand(this.Device);
                        b.SetAlliance(alliance);

                        AllianceRoleUpdateCommand c = new AllianceRoleUpdateCommand(this.Device);
                        c.SetAlliance(alliance);
                        c.SetRole(1);
                        c.Tick(this.Device.Player);

                        AllianceEventStreamEntry eventStreamEntry = new AllianceEventStreamEntry();
                        eventStreamEntry.ID = alliance.m_vChatMessages.Count > 0 ? alliance.m_vChatMessages.Last().ID + 1 : 1;
                        eventStreamEntry.SetSender(this.Device.Player.Avatar);
                        eventStreamEntry.EventType = 3;
                        alliance.AddChatMessage(eventStreamEntry);

                        new AvailableServerCommandMessage(this.Device, b.Handle()).Send();

                        new AvailableServerCommandMessage(this.Device, c.Handle()).Send();

                        new AllianceStreamMessage(Device, alliance).Send();
                        
                        this.Device.Player.Avatar.SendCLanMessagesToOldClient(this.Device);

                        foreach (AllianceMemberEntry a in alliance.GetAllianceMembers())
                        {
                            Level l = await ResourcesManager.GetPlayer(a.AvatarId);
                            new AllianceStreamEntryMessage(l.Client) { StreamEntry = eventStreamEntry }.Send();
                        }
                    }
                }
            } catch (Exception) { }
        }

    }
}
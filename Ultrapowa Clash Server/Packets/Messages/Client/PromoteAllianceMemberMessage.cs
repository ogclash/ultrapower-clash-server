using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Commands.Server;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Client
{
    // Packet 14306
    internal class PromoteAllianceMemberMessage : Message
    {
        public PromoteAllianceMemberMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public long m_vId;
        public int m_vRole;

        internal override void Decode()
        {
            this.m_vId   = this.Reader.ReadInt64();
            this.m_vRole = this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                Level target = await ResourcesManager.GetPlayer(m_vId);
                ClientAvatar player = this.Device.Player.Avatar;
                Alliance alliance = ObjectManager.GetAlliance(player.AllianceId);
                if (await player.GetAllianceRole() == 2 || await player.GetAllianceRole() == 4)
                    if (player.AllianceId == target.Avatar.AllianceId)
                    {
                        int oldrole = await target.Avatar.GetAllianceRole();
                        target.Avatar.SetAllianceRole(m_vRole);
                        if (m_vRole == 2)
                        {
                            player.SetAllianceRole(4);

                            AllianceEventStreamEntry demote = new AllianceEventStreamEntry();
                            demote.ID = alliance.m_vChatMessages.Count > 0 ? alliance.m_vChatMessages.Last().ID + 1 : 1;
                            demote.SetSender(player);
                            demote.EventType = 6;
                            demote.m_vAvatarId = player.UserId;
                            demote.m_vAvatarName = player.AvatarName;

                            alliance.AddChatMessage(demote);

                            AllianceEventStreamEntry promote = new AllianceEventStreamEntry();
                            promote.ID = alliance.m_vChatMessages.Count > 0 ? alliance.m_vChatMessages.Last().ID + 1 : 1;
                            promote.SetSender(target.Avatar);
                            promote.EventType = 5;
                            promote.m_vAvatarId = player.UserId;
                            promote.m_vAvatarName = player.AvatarName;

                            alliance.AddChatMessage(promote);

                            AllianceRoleUpdateCommand p = new AllianceRoleUpdateCommand(this.Device);
                            //AvailableServerCommandMessage pa = new AvailableServerCommandMessage(Device, p.Handle());

                            AllianceRoleUpdateCommand t = new AllianceRoleUpdateCommand(target.Client);
                            //AvailableServerCommandMessage ta = new AvailableServerCommandMessage(target.Client, t.Handle());

                            PromoteAllianceMemberOkMessage rup = new PromoteAllianceMemberOkMessage(Device)
                            {
                                Id = this.Device.Player.Avatar.UserId,
                                Role = 4
                            };
                            PromoteAllianceMemberOkMessage rub = new PromoteAllianceMemberOkMessage(target.Client)
                            {
                                Id = target.Avatar.UserId,
                                Role = 2
                            };

                            t.SetAlliance(alliance);
                            p.SetAlliance(alliance);
                            t.SetRole(2);
                            p.SetRole(4);
                            t.Tick(target);
                            p.Tick(this.Device.Player);

                            if (ResourcesManager.IsPlayerOnline(target))
                            {
                                //ta.Send();
                                rub.Send();
                            }
                            rup.Send();
                            //pa.Send();

                            foreach (AllianceMemberEntry op in alliance.GetAllianceMembers())
                            {
                                Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                                if (aplayer.Client != null)
                                {
                                    new AllianceStreamEntryMessage(aplayer.Client) { StreamEntry = demote }.Send();
                                    new AllianceStreamEntryMessage(aplayer.Client) { StreamEntry = promote }.Send();
                                }

                            }
                            if (this.Device.Player.Avatar.minorversion >= 709)
                                new OwnHomeDataMessage(this.Device, this.Device.Player).Send();
                            else
                                new OwnHomeDataForOldClients(this.Device, this.Device.Player).Send();
                        }
                        else
                        {
                            AllianceRoleUpdateCommand t = new AllianceRoleUpdateCommand(target.Client);
                            //AvailableServerCommandMessage ta = new AvailableServerCommandMessage(target.Client, t.Handle());

                            t.SetAlliance(alliance);
                            t.SetRole(m_vRole);
                            t.Tick(target);

                            PromoteAllianceMemberOkMessage ru = new PromoteAllianceMemberOkMessage(target.Client)
                            {
                                Id = target.Avatar.UserId,
                                Role =  m_vRole
                            };

                            if (ResourcesManager.IsPlayerOnline(target))
                            {
                                ru.Send();
                            }

                            AllianceEventStreamEntry stream = new AllianceEventStreamEntry();

                            stream.ID = alliance.m_vChatMessages.Count > 0 ? alliance.m_vChatMessages.Last().ID + 1 : 1;
                            stream.SetSender(target.Avatar);
                            stream.EventType = m_vRole > oldrole ? 5 : 6;
                            stream.m_vAvatarName = player.AvatarName;
                            stream.m_vAvatarId = player.UserId;
                            alliance.AddChatMessage(stream);

                            foreach (AllianceMemberEntry op in alliance.GetAllianceMembers())
                            {
                                Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                                if (aplayer.Client != null)
                                {
                                    new AllianceStreamEntryMessage(aplayer.Client) { StreamEntry = stream }.Send();
                                }
                            }
                        }
                    }
            } catch (Exception) { }
        }
    }
}

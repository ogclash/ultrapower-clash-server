using System;
using System.Collections.Generic;
using UCS.Core;
using UCS.Core.Network;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Commands.Server;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14310
    internal class DonateAllianceUnitMessage : Message
    {
        public CombatItemData Troop;
        public int MessageID;
        public byte BuyTroop;

        public DonateAllianceUnitMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.Troop = (CombatItemData) this.Reader.ReadDataReference();
            this.Reader.ReadInt32();
            this.MessageID = this.Reader.ReadInt32();
            this.BuyTroop = this.Reader.ReadByte();
            var unkown = "this.Reader.ReadByte()";
        }

        internal override async void Process()
        {
            try
            {
                if (this.BuyTroop >= 1)
                {     
                    this.Device.Player.Avatar.UseDiamonds(1);
                }
                else
                {
                    List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();

                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == Troop.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        if (_DataSlot.Value < 0)
                            _DataSlot.Value = 0;
                        else
                            _DataSlot.Value--;
                    }
                }

                Alliance a = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                StreamEntry _Stream = a.m_vChatMessages.Find(c => c.ID == MessageID);
                Level _Sender = await ResourcesManager.GetPlayer(_Stream.SenderID);
                int upcomingspace = _Stream.m_vDonatedTroop + Troop.GetHousingSpace();

                if (upcomingspace <= _Stream.m_vMaxTroop)
                {
                    
                    DonatedAllianceUnitCommand _Donated = new DonatedAllianceUnitCommand(this.Device);
                    _Donated.Tick(_Sender);
                    _Donated.MessageID = this.MessageID;
                    _Donated.TroopID = Troop.GetGlobalID();

                    new AvailableServerCommandMessage(this.Device, _Donated.Handle()).Send();

                    ReceivedAllianceUnitCommand _Received = new ReceivedAllianceUnitCommand(this.Device);
                    _Received.Donator_Name = this.Device.Player.Avatar.AvatarName;
                    _Received.TroopID = this.Troop.GetGlobalID();
                    _Received.Troop_Level = this.Device.Player.Avatar.GetUnitUpgradeLevel(Troop);

                    new AvailableServerCommandMessage(_Sender.Client, _Received).Send();

                    Level _PreviousPlayer = await ResourcesManager.GetPlayer(_Stream.SenderID);
                    ClientAvatar _PreviousPlayerAvatar = _PreviousPlayer.Avatar;
                    _Stream.AddDonatedTroop(this.Device.Player.Avatar.UserId, Troop.GetGlobalID(), 1, this.Device.Player.Avatar.GetUnitUpgradeLevel(Troop));
                    
                    int _Capicity = Troop.GetHousingSpace();
                    _Stream.AddUsedCapicity(_Capicity);
                    _PreviousPlayerAvatar.SetAllianceCastleUsedCapacity(_PreviousPlayerAvatar.GetAllianceCastleUsedCapacity() + _Capicity);
                    _PreviousPlayerAvatar.AddAllianceTroop(this.Device.Player.Avatar.UserId, Troop.GetGlobalID(), 1, this.Device.Player.Avatar.GetUnitUpgradeLevel(Troop));

                    this.Device.Player.Avatar.m_vDonated += Troop.GetHousingSpace();
                    _Sender.Avatar.m_vReceived += Troop.GetHousingSpace();
                    a.m_vAllianceExperienceInternal += Troop.GetHousingSpace();
                    a.m_vAllianceExperience = 
                        a.m_vAllianceExperienceInternal < 10 
                        ? 1 
                        : a.m_vAllianceExperienceInternal / 10;
                    var oldlevel = a.m_vAllianceLevel;
                    switch (a.m_vAllianceLevel)
                    {
                        case 1:
                            if (a.m_vAllianceExperience >= 500)
                                a.m_vAllianceLevel++;
                            break;
                        case 2:
                            if (a.m_vAllianceExperience >= 1200)
                                a.m_vAllianceLevel++;
                            break;
                        case 3:
                            if (a.m_vAllianceExperience >= 1900)
                                a.m_vAllianceLevel++;
                            break;
                        case 4:
                            if (a.m_vAllianceExperience >= 3100)
                                a.m_vAllianceLevel++;
                            break;
                        case 5:
                            if (a.m_vAllianceExperience >= 3800)
                                a.m_vAllianceLevel++;
                            break;
                        case 6:
                            if (a.m_vAllianceExperience >= 4500)
                                a.m_vAllianceLevel++;
                            break;
                        case 7:
                            if (a.m_vAllianceExperience >= 5200)
                                a.m_vAllianceLevel++;
                            break;
                        case 8:
                            if (a.m_vAllianceExperience >= 5900)
                                a.m_vAllianceLevel++;
                            break;
                        case 9:
                            if (a.m_vAllianceExperience >= 7900)
                                a.m_vAllianceLevel++;
                            break;
                        case 10:
                            if (a.m_vAllianceExperience >= 8600)
                                a.m_vAllianceLevel++;
                            break;
                        default:
                            var multiplier = a.m_vAllianceLevel-10; 
                            var xp = 700*multiplier;
                            if (a.m_vAllianceExperience >= xp)
                                a.m_vAllianceLevel++;
                            break;
                    }

                    if (oldlevel != a.m_vAllianceLevel)
                    {
                        a.m_vAllianceExperience = 0;
                    }


                    foreach (AllianceMemberEntry op in a.GetAllianceMembers())
                    {
                        Level player = await ResourcesManager.GetPlayer(op.AvatarId);
                        if (player.Client != null)
                        {
                            new AllianceStreamEntryMessage(player.Client) { StreamEntry = _Stream }.Send();
                        }
                    }
                }
                if (upcomingspace == _Stream.m_vMaxTroop)
                {
                    a.m_vChatMessages.RemoveAll(t => t == _Stream);
                    foreach (AllianceMemberEntry op in a.GetAllianceMembers())
                    {
                        Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                        if (aplayer.Client != null)
                        {
                            new AllianceStreamEntryRemovedMessage(aplayer.Client, _Stream.ID).Send();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

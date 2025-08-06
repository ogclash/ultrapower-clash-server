using System;
using System.Linq;
using System.Text.RegularExpressions;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Commands.Client
{
    // Packet 511
    internal class RequestAllianceUnitsCommand : Command
    {
        public RequestAllianceUnitsCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.FlagHasRequestMessage = this.Reader.ReadByte();
            this.Message = this.Reader.ReadString();
            this.Message2 = this.Reader.ReadString();
        }

        internal override async void Process()
        {
            try
            {
                ClientAvatar player = this.Device.Player.Avatar;
                string filteredMessage = Regex.Replace(this.Message, @"[^a-zA-Z0-9 ]", "");
                
                player.TroopRequestMessage = filteredMessage;
                Alliance all = ObjectManager.GetAlliance(player.AllianceId);
                TroopRequestStreamEntry cm = new TroopRequestStreamEntry();
                cm.SetSender(player);
                cm.Message = this.Message;
                cm.ID = all.m_vChatMessages.Count > 0 ? all.m_vChatMessages.Last().ID + 1 : 1;
                cm.SetType(1);
                cm.SetMaxTroop(player.GetAllianceCastleTotalCapacity());
                cm.m_vDonatedTroop = player.GetAllianceCastleUsedCapacity();
                
                if (player.GetAllianceCastleTotalCapacity() > player.GetAllianceCastleUsedCapacity())
                {
                    StreamEntry oldmessage = all.m_vChatMessages.Find(c => c.SenderID == this.Device.Player.Avatar.UserId && c.m_vType == 1);
                    all.m_vChatMessages.Remove(oldmessage);
                    all.AddChatMessage(cm);
                    foreach (AllianceMemberEntry op in all.GetAllianceMembers())
                    {
                        Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                        if (aplayer.Client != null)
                        {
                            if (oldmessage != null && oldmessage.m_vSenderName == player.AvatarName)
                            {
                                new AllianceStreamEntryRemovedMessage(aplayer.Client, oldmessage.ID).Send();
                            }
                            new AllianceStreamEntryMessage(aplayer.Client) { StreamEntry = cm }.Send();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public byte FlagHasRequestMessage;
        public string Message;
        public int MessageLength;
        public string Message2;
    }
}

using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    internal class ChallangeCancelMessage : Message
    {
        public ChallangeCancelMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override async void Process()
        {
            try
            {
                Alliance alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                StreamEntry oldmessage = alliance.m_vChatMessages.Find(c => c.GetStreamEntryType() == 12);
                alliance.m_vChatMessages.Remove(oldmessage);
                foreach (AllianceMemberEntry op in alliance.GetAllianceMembers())
                {
                    Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                    if (aplayer.Client != null)
                    {
                        if (oldmessage != null)
                        {
                            new AllianceStreamEntryRemovedMessage(aplayer.Client, oldmessage.ID).Send();
                        }
                    }
                }
            } catch (Exception) { }
        }

    }
}

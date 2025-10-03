using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Logic.StreamEntry;

namespace UCS.Packets.Messages.Server
{
    // Packet 24311
    internal class AllianceStreamMessage : Message
    {
        readonly Alliance m_vAlliance;

        public AllianceStreamMessage(Device client, Alliance alliance) : base(client)
        {
            this.Identifier = 24311;
            m_vAlliance = alliance;
        }

        internal override async void Encode()
        {
            StreamEntry oldmessage =  m_vAlliance.m_vChatMessages.Find(c => c.GetStreamEntryType() == 12);
            if (m_vAlliance.m_vChatMessages .Count() != 0 && oldmessage !=  m_vAlliance.m_vChatMessages.Last())
            {
                m_vAlliance.m_vChatMessages.Remove(oldmessage);
                foreach (AllianceMemberEntry op in m_vAlliance.GetAllianceMembers())
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
            }
            var chatMessages = m_vAlliance.m_vChatMessages.ToList();
            this.Data.AddInt(0);
            this.Data.AddInt(chatMessages.Count);
            int count = 0;
            foreach (StreamEntry chatMessage in chatMessages)
            {
                this.Data.AddRange(chatMessage.Encode());
                count++;
                if (count >= 150)
                {
                    break;
                }
            }
        }
    }
}

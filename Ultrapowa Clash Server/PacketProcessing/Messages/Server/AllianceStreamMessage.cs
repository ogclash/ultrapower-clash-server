using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Old.Helpers;
using UCS.Packets;

namespace UCS.PacketProcessing
{
    //Packet 24311
    class AllianceStreamMessage : Message
    {
        private Alliance m_vAlliance;

        public AllianceStreamMessage(Device client, Alliance alliance)
            : base(client)
        {
            SetMessageType(24311);
            m_vAlliance = alliance;
        }

        public override void Encode()
        {
            List<Byte> pack = new List<Byte>();
            

            pack.AddInt32(m_vAlliance.m_vChatMessages.Count);
            foreach (var chatMessage in  m_vAlliance.m_vChatMessages)
            {
                pack.AddRange(chatMessage.Encode());
            }
            
            SetData(pack.ToArray());
        }
    }
}

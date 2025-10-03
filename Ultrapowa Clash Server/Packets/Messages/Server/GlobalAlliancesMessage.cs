using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server
{
    // Packet 24401
    internal class GlobalAlliancesMessage : Message
    {
        public GlobalAlliancesMessage(Device client) : base(client)
        {
            this.Identifier = 24401;
        }

        internal override void Encode()
        {
            List<byte> packet1 = new List<byte>();
            int i = 0;

            foreach (Alliance alliance in ObjectManager.GetInMemoryAlliances().OrderByDescending(t => t.m_vScore))
            {
                try
                {
                    if (alliance.m_vAllianceMembers.Count() == 0)
                        continue;
                    if (i >= 100)
                        break;
                    packet1.AddLong(alliance.m_vAllianceId);
                    packet1.AddString(alliance.m_vAllianceName);
                    packet1.AddInt(i + 1);
                    packet1.AddInt(alliance.m_vScore);
                    packet1.AddInt(i + 1);
                    packet1.AddInt(alliance.m_vAllianceBadgeData);
                    packet1.AddInt(alliance.GetAllianceMembers().Count);
                    packet1.AddInt(0);
                    packet1.AddInt(alliance.m_vAllianceLevel);
                    i++;
                }
                catch (Exception) { }
            }

            this.Data.AddInt(i);
            this.Data.AddRange(packet1);

            this.Data.AddInt((int) TimeSpan.FromDays(1).TotalSeconds);
            this.Data.AddInt(3);
            this.Data.AddInt(50000);
            this.Data.AddInt(30000);
            this.Data.AddInt(15000);
        }
    }
}

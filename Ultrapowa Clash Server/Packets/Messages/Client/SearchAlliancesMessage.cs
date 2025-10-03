using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14324
    internal class SearchAlliancesMessage : Message
    {
        public SearchAlliancesMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        const int m_vAllianceLimit = 40;
        int m_vAllianceOrigin;
        int m_vAllianceScore;
        int m_vMaximumAllianceMembers;
        int m_vMinimumAllianceLevel;
        int m_vMinimumAllianceMembers;
        string m_vSearchString;
        byte m_vShowOnlyJoinableAlliances;
        int m_vWarFrequency;

        internal override void Decode()
        {
            m_vSearchString = Reader.ReadString();
            this.m_vWarFrequency = this.Reader.ReadInt32();
            this.m_vAllianceOrigin = this.Reader.ReadInt32();
            this.m_vMinimumAllianceMembers = this.Reader.ReadInt32();
            this.m_vMaximumAllianceMembers = this.Reader.ReadInt32();
            this.m_vAllianceScore = this.Reader.ReadInt32();
            this.m_vShowOnlyJoinableAlliances = this.Reader.ReadByte();
            var unknown = this.Reader.ReadInt32();
            this.m_vMinimumAllianceLevel = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            List<Alliance> joinableAlliances = new List<Alliance>();

            foreach (Alliance _Alliance in ResourcesManager.m_vInMemoryAlliances.Values)
            {
                if (_Alliance.m_vAllianceName.Contains(m_vSearchString, StringComparison.OrdinalIgnoreCase))
                {
                    if (_Alliance.m_vAllianceMembers.Count() == 0)
                        continue;
                    joinableAlliances.Add(_Alliance);
                }
            }

            AllianceListMessage p = new AllianceListMessage(Device);
            p.m_vAlliances = joinableAlliances;
            p.m_vSearchString = m_vSearchString;
            p.Send();
        }
    }
}
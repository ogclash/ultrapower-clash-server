using System;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server.Support
{
    internal class AllianceDataForOldClients : Message
    {
        readonly Alliance m_vAlliance;

        public AllianceDataForOldClients(Device client, Alliance alliance) : base(client)
        {
            this.Identifier = 24301;
            m_vAlliance = alliance;
        }
        
        internal override async void Encode()
        {
            try
            {
                var allianceMembers = m_vAlliance.GetAllianceMembers();

                this.Data.AddRange(m_vAlliance.EncodeFullEntry(true));
                this.Data.AddString(m_vAlliance.m_vAllianceDescription);
                this.Data.AddInt(0);
                this.Data.Add(0);

                this.Data.AddInt(1);

                foreach (AllianceMemberEntry m in allianceMembers)
                {
                    if (m.Role == 2)
                        this.Data.AddRange(await m.Encode());
                }
            }
            catch (Exception) { }
        }
    }
}
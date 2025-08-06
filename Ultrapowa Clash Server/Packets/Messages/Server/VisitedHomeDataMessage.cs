using System;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server
{
    // Packet 24113
    internal class VisitedHomeDataMessage : Message
    {
        private bool challenge;
        public VisitedHomeDataMessage(Device client, Level ownerLevel, Level visitorLevel, bool challenge = false) : base(client)
        {
            this.Identifier = 24113;
            m_vOwnerLevel = ownerLevel;
            m_vVisitorLevel = visitorLevel;
            this.Device.PlayerState = Logic.Enums.State.VISIT;
            this.challenge = challenge;
        }

        internal override async void Encode()
        {
            try
            {
                string village;
                if (this.challenge)
                     village = m_vOwnerLevel.SaveToJSONforChallange();
                else
                     village = m_vOwnerLevel.SaveToJSON();
                ClientHome ownerHome = new ClientHome
                {
                    Id = m_vOwnerLevel.Avatar.UserId,
                    ShieldTime = m_vOwnerLevel.Avatar.m_vShieldTime,
                    ProtectionTime = m_vOwnerLevel.Avatar.m_vProtectionTime,
                    Village = village
                };

                this.Data.AddInt(-1);
                this.Data.AddInt((int)TimeSpan.FromSeconds(100).TotalSeconds);
                this.Data.AddRange(ownerHome.Encode);
                this.Data.AddRange(await this.m_vOwnerLevel.Avatar.Encode());
                this.Data.AddInt(0);
                this.Data.Add(1);
                this.Data.AddRange(await this.m_vVisitorLevel.Avatar.Encode());
            }
            catch (Exception)
            {
            }
        }

        readonly Level m_vOwnerLevel;
        readonly Level m_vVisitorLevel;
    }
}

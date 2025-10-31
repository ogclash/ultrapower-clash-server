using System;
using System.Collections.Generic;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Server
{
    // Packet 24101
    internal class OwnHomeDataMessage : Message
    {
        public OwnHomeDataMessage(Device client, Level level) : base(client)
        {
            this.Identifier = 24101;
            this.Player = level;
            this.Player.Tick();
        }

        public Level Player;

        internal override async void Encode()
        {
            if (this.Device.Player.Avatar.minorversion < 551)
            {
                new OwnHomeDataForOldClients(this.Device, this.Player).Send();
                return;
            }
            try
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                int elapsed = (int)(now - this.Player.Avatar.mv_ShieldTimeStamp);
                int shieldTimeRemaining = Math.Max(0, this.Player.Avatar.m_vShieldTimeValue - elapsed);
                this.Device.Player.Avatar.m_vShieldTime = shieldTimeRemaining;
                
                elapsed = (int)(now - this.Player.Avatar.m_vProtectionTimeStamp);
                shieldTimeRemaining = Math.Max(0, this.Player.Avatar.m_vProtectionTimeValue - elapsed);
                this.Device.Player.Avatar.m_vProtectionTime = shieldTimeRemaining;
                this.Player.Avatar.matchedPlayers.Clear();
                this.Player.Avatar.setBuidlings(new List<int[]>());
                var _Home =
                    new ClientHome
                    {
                        Id = this.Player.Avatar.UserId,
                        ShieldTime = this.Device.Player.Avatar.m_vShieldTime,
                        ProtectionTime = this.Device.Player.Avatar.m_vProtectionTime,
                        TimeOut =  7200 - (int)(now - this.Device.Player.Avatar.time_out_timestamp),
                        Village = this.Player.SaveToJSONforPlayer()
                    };

                this.Data.AddInt(0);
                this.Data.AddInt(-1);
                this.Data.AddInt((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                this.Data.AddRange(_Home.Encode);
                this.Data.AddRange(await this.Player.Avatar.Encode());
                this.Data.AddInt(this.Device.PlayerState == State.WAR_EMODE ? 1 : 0);
                this.Data.AddInt(0);
                this.Data.AddLong(0);
                this.Data.AddLong(0);
                this.Data.AddLong(0);
            }
            catch (Exception)
            {
            }
            
        }
    }
}
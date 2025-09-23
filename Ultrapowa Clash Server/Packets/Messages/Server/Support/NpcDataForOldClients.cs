using System;
using System.Text;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Client;

namespace UCS.Packets.Messages.Server.Support
{
    internal class NpcDataForOldClients : Message
    {
        public NpcDataForOldClients(Device client, Level level, AttackNpcMessage cnam) : base(client)
        {
            this.Identifier = 24133;
            this.Player = level;
            this.LevelId = cnam.LevelId;
            this.JsonBase = ObjectManager.NpcLevels[LevelId];
            this.Device.PlayerState = State.IN_BATTLE;
        }
        
        internal override async void Encode()
        {
            try
            {
                this.Data.AddInt(0);
                this.Data.AddInt(JsonBase.Length);
                this.Data.AddRange(Encoding.ASCII.GetBytes(JsonBase));
                //this.Data.AddRange(new ClientHome { Id = Player.Avatar.UserId, ShieldTime = this.Player.Avatar.m_vShieldTime, ProtectionTime = this.Player.Avatar.m_vProtectionTime, Village = this.JsonBase }.Encode);
                this.Data.AddRange(await this.Player.Avatar.EncodeForOldVersion());
                this.Data.AddInt(0);
                this.Data.AddInt(this.LevelId);
            }
            catch (Exception)
            {
            }
        }

        public string JsonBase;
        public int LevelId;
        public Level Player;
    }
}
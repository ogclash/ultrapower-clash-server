using System;
using System.Threading.Tasks;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Commands.Client
{
    // Packet 700
    internal class SearchOpponentCommand : Command
    {
        public SearchOpponentCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            var unknown1 = this.Reader.ReadUInt32();
            var unknown2 = this.Reader.ReadInt32();
            var unknown3 = this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                if (this.Device.ShieldInfo)
                {
                    this.Device.ShieldInfo = false;
                    int shieldReduction = 3 * 60 * 60;
                    if (Device.Player.Avatar.m_vShieldTime > 0 && Device.Player.Avatar.mv_ShieldTimeStamp > 0)
                    {
                        Device.Player.Avatar.mv_ShieldTimeStamp -= shieldReduction;
                        
                        if (Device.Player.Avatar.mv_ShieldTimeStamp < 0)
                            Device.Player.Avatar.mv_ShieldTimeStamp = 0;
                    }
                }
                SearchEnemyAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task SearchEnemyAsync()
        {
            this.Device.PlayerState = State.SEARCH_BATTLE;

            Level defender = ObjectManager.GetRandomOfflinePlayer();

            // Search loop
            while (Device.Player.Avatar.UserId == defender.Avatar.UserId || defender.Avatar.GetScore() + 720 < Device.Player.Avatar.GetScore())
            {
                defender = ObjectManager.GetRandomOfflinePlayer();
                await Task.Delay(1); 
                if (this.Device.PlayerState != State.SEARCH_BATTLE)
                {
                    return;
                }
            }

            // Assign victim and start battle
            this.Device.AttackVictim = defender;
            defender.Tick();
            if (this.Device.Player.Avatar.minorversion >= 709)
                new EnemyHomeDataMessage(this.Device, defender, this.Device.Player).Send();
            else
                new EnemyHomeDataForOldClients(this.Device, defender, this.Device.Player).Send();
            // Mark search finished
            this.Device.PlayerState = State.IN_BATTLE; // Player is now officially in battle
        }
    }
}

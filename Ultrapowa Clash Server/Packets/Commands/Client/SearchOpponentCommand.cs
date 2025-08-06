using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;

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
                if (this.Device.PlayerState == State.IN_BATTLE)
                {
                    this.Device.PlayerState = State.LOGGED;
                }
                else
                {
                    // New Method
                    this.Device.PlayerState = State.SEARCH_BATTLE;
                    Level Defender = ObjectManager.GetRandomOfflinePlayer();
                    while (Device.Player == Defender || Defender.Avatar.GetScore()+1000 < Device.Player.Avatar.GetScore())
                    {
                        Defender = ObjectManager.GetRandomOfflinePlayer();
                    }
                    this.Device.AttackVictim = Defender;
                    Defender.Tick();
                    new EnemyHomeDataMessage(this.Device, Defender, this.Device.Player).Send();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

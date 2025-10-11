﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.DataSlots;

namespace UCS.Packets.Commands.Client
{
    internal class PlaceAllianceTroopsCommand : Command
    {
        public PlaceAllianceTroopsCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.Reader.ReadInt32();
            this.Reader.ReadInt32();
            this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            if (this.Device.AttackInfo == null)
            {
                this.Device.AttackInfo = "multiplayer";
            }
            if (this.Device.AttackInfo == "challenge")
            {
                return;
            }
            ClientAvatar _Player = this.Device.Player.Avatar;
            foreach (DonationSlot VARIABLE in this.Device.Player.Avatar.AllianceUnits)
            {
                JArray unitInfo = new JArray
                {
                    VARIABLE.ID,
                    VARIABLE.UnitLevel,
                    VARIABLE.Count
                };
                this.Device.Player.Avatar.battle.cc_units.Add(unitInfo);
            }

            if (_Player != null)
            {
                _Player.AllianceUnits.Clear();
                _Player.SetAllianceCastleUsedCapacity(0);
            }
        }
    }
}

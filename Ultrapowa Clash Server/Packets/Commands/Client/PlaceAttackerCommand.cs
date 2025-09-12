using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Core;

namespace UCS.Packets.Commands.Client
{
    // Packet 600
    internal class PlaceAttackerCommand : Command
    {
        public PlaceAttackerCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.UnitID = this.Reader.ReadInt32();
            this.Unit = (CombatItemData)CSVManager.DataTables.GetDataById(UnitID); ;
            this.Tick = this.Reader.ReadUInt32();
        }


        internal override void Process()
        {
            this.Device.PlayerState = State.IN_BATTLE;
            if (this.Device.AttackInfo == null)
            {
                this.Device.AttackInfo = "multiplayer";
            }

            if (this.Device.AttackInfo == "challenge")
            {
                return;
            }
            if (this.Device.AttackInfo == "npc")
            {
                this.Device.NpcAttacked = true;
            }

            bool found = false;

            foreach (JArray unit in this.Device.Player.Avatar.battle.units.ToList())
            {
                int currentUnitId = (int)unit[0];
                int currentCount = (int)unit[1];

                if (currentUnitId == UnitID)
                {
                    // increment unit count
                    unit[1] = currentCount + 1;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // add new unit if not found
                JArray unitInfo = new JArray
                {
                    UnitID,
                    1
                };
                this.Device.Player.Avatar.battle.units.Add(unitInfo);
                
                JArray unitLevel = new JArray
                {
                    UnitID,
                    this.Device.Player.Avatar.GetUnitUpgradeLevel(Unit)
                };
                this.Device.Player.Avatar.battle.levels.Add(unitLevel);
            }
            
            List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();

            DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == Unit.GetGlobalID());
            if (_DataSlot != null)
            {
                if (_DataSlot.Value < 0)
                    _DataSlot.Value = 0;
                else
                    _DataSlot.Value--;
            }
            
        }

        public CombatItemData Unit;
        public uint Tick;
        private int UnitID;
        public int X;
        public int Y;
    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands
{
    // Packet 604
    internal class CastSpellCommand : Command
    {
        public CastSpellCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.Spell = (SpellData) this.Reader.ReadDataReference();
            var unknown = this.Reader.ReadBoolean();
            var lvl = this.Reader.ReadInt32();
            var unknown1 = this.Reader.ReadInt32();
            base.Decode();
        }

        internal override void Process()
        {
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

            foreach (JArray unit in this.Device.Player.Avatar.battle.spells.ToList())
            {
                int currentUnitId = (int)unit[0];
                int currentCount = (int)unit[1];

                if (currentUnitId == Spell.GetGlobalID())
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
                    Spell.GetGlobalID(),
                    1
                };
                this.Device.Player.Avatar.battle.spells.Add(unitInfo);
                
                JArray unitLevel = new JArray
                {
                    Spell.GetGlobalID(),
                    this.Device.Player.Avatar.GetUnitUpgradeLevel(Spell)
                };
                this.Device.Player.Avatar.battle.levels.Add(unitLevel);
            }
            
            List<DataSlot> _PlayerSpells = this.Device.Player.Avatar.GetSpells();

            DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == Spell.GetGlobalID());
            if (_DataSlot != null)
            {
                if (_DataSlot.Value < 0)
                    _DataSlot.Value = 0;
                else
                    _DataSlot.Value--;
            }
        }

        public SpellData Spell;
        public int X;
        public int Y;
    }
}
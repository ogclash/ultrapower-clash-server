using System.Collections.Generic;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.DataSlots;

namespace UCS.Packets.Commands.Client
{
    // Packet 550
    internal class RemoveUnitsCommand : Command
    {
        public RemoveUnitsCommand(Reader reader, Device client, int id) : base(reader, client, id)
        { 
        }

        internal override void Decode()
        {
            UnitTypesCount = this.Reader.ReadUInt32();
            UnitsToRemove = new List<UnitToRemove>();
            for (int i = 0; i < UnitTypesCount; i++)
            {
                this.AllianceUnits = this.Reader.ReadInt32();
                this.Reader.ReadUInt32();
                int UnitType = this.Reader.ReadInt32();
                int count = this.Reader.ReadInt32();
                int level = this.Reader.ReadInt32();
                this.UnitsToRemove.Add(new UnitToRemove { Data = UnitType, Count = count, Level = level }); ;
            }
        }


        internal override void Process()
        {
            List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();
            List<DataSlot> _PlayerSpells = this.Device.Player.Avatar.GetSpells();
            
          
            foreach (UnitToRemove _Unit in UnitsToRemove)
            {
                if (_Unit.Data.ToString().StartsWith("400"))
                {
                    if (AllianceUnits == 1)
                    {
                        CombatItemData troop =  (CombatItemData)CSVManager.DataTables.GetDataById(_Unit.Data);
                        DonationSlot e = this.Device.Player.Avatar.AllianceUnits.Find(t => t.ID == _Unit.Data && t.UnitLevel == _Unit.Level);
                        this.Device.Player.Avatar.RemoveAllianceTroop(e, _Unit.Count);
                        int capacity = troop.GetHousingSpace() * _Unit.Count;
                        this.Device.Player.Avatar.SetAllianceCastleUsedCapacity(this.Device.Player.Avatar.GetAllianceCastleUsedCapacity() - capacity);
                        return;
                    }
                    CombatItemData _Troop = (CombatItemData)CSVManager.DataTables.GetDataById(_Unit.Data); ;
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == _Troop.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value = _DataSlot.Value - _Unit.Count;
                    }
                }
                else if (_Unit.Data.ToString().StartsWith("260"))
                {
                    SpellData _Spell = (SpellData)CSVManager.DataTables.GetDataById(_Unit.Data); ;
                    DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == _Spell.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value = _DataSlot.Value - _Unit.Count;
                    }
                }
            }
        }

        public List<UnitToRemove> UnitsToRemove;
        public uint UnitTypesCount;
        public int AllianceUnits;
    }

    internal class UnitToRemove
    {
        public int Data;
        public int Count;
        public int Level;
    }
}

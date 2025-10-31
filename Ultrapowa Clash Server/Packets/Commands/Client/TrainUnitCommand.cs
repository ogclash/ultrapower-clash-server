using System;
using System.Collections.Generic;
using UCS.Core;
using UCS.Core.Settings;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 508
    internal class TrainUnitCommand : Command
    {
        public TrainUnitCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            
        }

        internal override void Decode()
        {
            buildingId = this.Reader.ReadInt32();
            slotId2 = this.Reader.ReadUInt32();
            this.UnitType = this.Reader.ReadInt32();
            this.Count    = this.Reader.ReadInt32();
            this.slotId = this.Reader.ReadUInt32();
            if (this.Device.Player.Avatar.minorversion >= 551)
                Tick  = this.Reader.ReadInt32();
        }

        public uint slotId;
        public uint slotId2;
        public int buildingId;
        public int Count;
        public int UnitType;
        public int Tick;

        internal override void Process()
        {
            if (!Constants.DeveloperBuild)
                if ((UnitType == 4000030 || UnitType == 26000004) && DateTime.Now.Month != 12 && DateTime.Now.Month != 1)
                    return;
            ClientAvatar _Player = this.Device.Player.Avatar;
            if (UnitType.ToString().StartsWith("400"))
            {
                CombatItemData _TroopData = (CombatItemData)CSVManager.DataTables.GetDataById(UnitType);
                List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();
                ResourceData _TrainingResource = _TroopData.GetTrainingResource();
                if (this.Device.Player.Avatar.minorversion < 551)
                {
                    int troops = 0;
                    foreach (var unit in _PlayerUnits)
                    {
                        if (unit.Value < 0)
                            unit.Value = 0;
                        CharacterData unitData = (CharacterData) unit.Data;
                        int housingSpace = unitData.HousingSpace;
                        troops += unit.Value * housingSpace;
                    }
                    
                    troops += Count * ((CharacterData)_TroopData).HousingSpace;
                    if (troops <= this.Device.Player.GameObjectManager.GetComponentManager().GetTotalMaxHousing())
                    {
                        DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == _TroopData.GetGlobalID());
                        if (_DataSlot != null)
                            _DataSlot.Value += this.Count;
                        else
                        {
                            DataSlot ds = new DataSlot(_TroopData, this.Count);
                            _PlayerUnits.Add(ds);
                        }

                        _Player.SetResourceCount(_TrainingResource, _Player.GetResourceCount(_TrainingResource) - _TroopData.GetTrainingCost(_Player.GetUnitUpgradeLevel(_TroopData)));
                    }
                }
                else
                {
                    int unitLevel = this.Device.Player.Avatar.GetUnitUpgradeLevel(_TroopData);
                    this.Device.Player.Avatar.SetResourceCount(_TroopData.GetTrainingResource(), this.Device.Player.Avatar.GetResourceCount(_TroopData.GetTrainingResource())-_TroopData.GetTrainingCost(unitLevel));
                    UnitProductionComponent barrack = null;
                    
                    if (buildingId == 0)
                        barrack = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetMainProduction().GetComponent(3);
                    else
                        barrack = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetGameObjectByID(buildingId).GetComponent(3);
                    for (int i = 0; i < Count; i++)
                        barrack.AddUnitToProductionQueue(_TroopData, true);
                }
            }
            else if (UnitType.ToString().StartsWith("260"))
            {
                SpellData _SpellData = (SpellData)CSVManager.DataTables.GetDataById(UnitType);
                if (this.Device.Player.Avatar.minorversion < 551)
                {
                    List<DataSlot> _PlayerSpells = this.Device.Player.Avatar.GetSpells();
                    ResourceData _CastResource = _SpellData.GetTrainingResource();
                    
                    int spells = 0;
                    foreach (var unit in _PlayerSpells)
                    {
                        if (unit.Value < 0)
                            unit.Value = 0;
                        SpellData unitData = (SpellData) unit.Data;
                        int housingSpace = unitData.HousingSpace;
                        spells += unit.Value * housingSpace;
                    }
                    
                    spells += Count * _SpellData.HousingSpace;
                
                    if (spells < this.Device.Player.GameObjectManager.GetComponentManager().GetTotalMaxHousing(true))
                    {
                        DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == _SpellData.GetGlobalID());
                        if (_DataSlot != null)
                        {
                            _DataSlot.Value += this.Count;
                        }
                        else
                        {
                            DataSlot ds = new DataSlot(_SpellData, this.Count);
                            _PlayerSpells.Add(ds);
                        }

                        _Player.SetResourceCount(_CastResource, _Player.GetResourceCount(_CastResource) - _SpellData.GetTrainingCost(_Player.GetUnitUpgradeLevel(_SpellData)));
                    }
                }
                else
                {
                    int spelllevel = this.Device.Player.Avatar.GetUnitUpgradeLevel(_SpellData);
                    this.Device.Player.Avatar.SetResourceCount(_SpellData.GetTrainingResource(),  this.Device.Player.Avatar.GetResourceCount(_SpellData.GetTrainingResource())-_SpellData.GetTrainingCost(spelllevel));
                    UnitProductionComponent factory = null;
                    if (buildingId == 0)
                        factory = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetMainProduction(true).GetComponent(3);
                    else
                        factory = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetGameObjectByID(buildingId).GetComponent(3);
                    for (int i = 0; i < Count; i++)
                        factory.AddUnitToProductionQueue(_SpellData, true);
                }

            }
        }
    }
}


using System.Collections.Generic;
using UCS.Core;
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
            if (this.Device.Player.Avatar.minorversion >= 709)
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
            int id = (int)slotId;
            ClientAvatar _Player = this.Device.Player.Avatar;
            if (UnitType.ToString().StartsWith("400"))
            {
                CombatItemData _TroopData = (CombatItemData)CSVManager.DataTables.GetDataById(UnitType);
                List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();
                ResourceData _TrainingResource = _TroopData.GetTrainingResource();
                if (this.Device.Player.Avatar.minorversion < 709)
                {
                    var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
                    var troops = 0;
                    foreach (var unit in _PlayerUnits)
                    {
                        if (unit.Value < 0)
                            unit.Value = 0;
                        var unitData = (CharacterData) unit.Data;
                        var housingSpace = unitData.HousingSpace;
                        troops += unit.Value * housingSpace;
                    }

                    if (_TroopData != null)
                    {
                        var unitData = (CharacterData)_TroopData;
                        var housingSpace = unitData.HousingSpace;
                        troops += Count * housingSpace;
                    }
                    if (troops <= gameobjects.GetTotalMaxHousing())
                    {
                        DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == _TroopData.GetGlobalID());
                        if (_DataSlot != null)
                        {
                            _DataSlot.Value = _DataSlot.Value + this.Count;
                        }
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
                    if (buildingId == 0)
                        buildingId = 500000010;
                    UnitProductionComponent barrack = (UnitProductionComponent)this.Device.Player.GameObjectManager.GetGameObjectByID(buildingId).GetComponent(3, false);
                    for (int i = 0; i < Count; i++)
                        barrack.AddUnitToProductionQueue(_TroopData, true);
                }
            }
            else if (UnitType.ToString().StartsWith("260"))
            {
                SpellData _SpellData = (SpellData)CSVManager.DataTables.GetDataById(UnitType);
                if (this.Device.Player.Avatar.minorversion < 709)
                {
                    List<DataSlot> _PlayerSpells = this.Device.Player.Avatar.GetSpells();
                    ResourceData _CastResource = _SpellData.GetTrainingResource();

                    var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
                    var spells = 0;
                    foreach (var unit in _PlayerSpells)
                    {
                        if (unit.Value < 0)
                            unit.Value = 0;
                        var unitData = (UCS.Files.Logic.SpellData) unit.Data;
                        var housingSpace = unitData.HousingSpace;
                        spells += unit.Value * housingSpace;
                    }
                    if (_SpellData != null)
                    {
                        var spellData = (SpellData)_SpellData;
                        var housingSpace = spellData.HousingSpace;
                        spells += Count * housingSpace;
                    }
                
                    if (spells < gameobjects.GetTotalMaxHousing(true))
                    {
                        DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == _SpellData.GetGlobalID());
                        if (_DataSlot != null)
                        {
                            _DataSlot.Value = _DataSlot.Value + this.Count;
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
                    List<GameObject> buildings = this.Device.Player.GameObjectManager.GetAllGameObjects()[0];
                    List<GameObject> factories = new List<GameObject>();
                    foreach (GameObject gameObject in buildings)
                    {
                        if (gameObject.GetData().GetGlobalID() == 1000020)
                        {
                            factories.Add(gameObject);
                        }
                    }
                    UnitProductionComponent factory = (UnitProductionComponent)factories[0].GetComponent(3, false);
                    for (int i = 0; i < Count; i++)
                        factory.AddUnitToProductionQueue(_SpellData, true);
                }

            }
        }
    }
}


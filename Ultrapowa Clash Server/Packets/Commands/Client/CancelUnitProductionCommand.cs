using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using System.Collections.Generic;

namespace UCS.Packets.Commands.Client
{
    // Packet 509
    internal class CancelUnitProductionCommand : Command
    {
        public CancelUnitProductionCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.BuildingId = this.Reader.ReadInt32();
            this.Reader.ReadInt32();
            this.UnitType = this.Reader.ReadInt32();
            this.Count = this.Reader.ReadInt32();
            this.SlotId = this.Reader.ReadInt32();
            this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();
            List<DataSlot> _PlayerSpells = this.Device.Player.Avatar.GetSpells();
            if (UnitType.ToString().StartsWith("400"))
            {
                CombatItemData _Troop = (CombatItemData)CSVManager.DataTables.GetDataById(UnitType);
                if (this.Device.Player.Avatar.minorversion >= 551)
                {
                    int unitLevel = this.Device.Player.Avatar.GetUnitUpgradeLevel(_Troop);
                    this.Device.Player.Avatar.SetResourceCount(_Troop.GetTrainingResource(), this.Device.Player.Avatar.GetResourceCount(_Troop.GetTrainingResource())+_Troop.GetTrainingCost(unitLevel));
                    UnitProductionComponent barrack = (UnitProductionComponent)this.Device.Player.GameObjectManager.GetGameObjectByID(500000010).GetComponent(3, false);
                    for (int i = 0; i < Count; i++)
                        barrack.RemoveUnit(_Troop, SlotId);
                }
                else
                {
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == _Troop.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value = _DataSlot.Value - Count;
                    }
                }
                
            }
            else if (UnitType.ToString().StartsWith("260"))
            {
                SpellData _SpellData = (SpellData)CSVManager.DataTables.GetDataById(UnitType);
                if (this.Device.Player.Avatar.minorversion >= 551)
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
                    int spelllevel = this.Device.Player.Avatar.GetUnitUpgradeLevel(_SpellData);
                    this.Device.Player.Avatar.SetResourceCount(_SpellData.GetTrainingResource(),  this.Device.Player.Avatar.GetResourceCount(_SpellData.GetTrainingResource())+_SpellData.GetTrainingCost(spelllevel));
                    UnitProductionComponent factory = (UnitProductionComponent)factories[0].GetComponent(3, false);
                    for (int i = 0; i < Count; i++)
                        factory.RemoveUnit(_SpellData, SlotId);
                }
                else
                {
                    DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == _SpellData.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value = _DataSlot.Value - Count;
                    }
                }
            }
        }

        public int BuildingId;
        public int SlotId;
        public int Count;
        public int UnitType;
    }
}
using System.IO;
using UCS.Core;
using UCS.Core.Network;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14134
    internal class AttackNpcMessage : Message
    {
        public AttackNpcMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public int LevelId { get; set; }

        internal override void Decode()
        {
            this.LevelId = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            if (this.Device.PlayerState == State.IN_BATTLE)
            {
                this.Device.AttackInfo = "npc";
                ResourcesManager.DisconnectClient(Device);
            }
            else
            {
                this.Device.AttackInfo = "npc";
                if (LevelId > 0 || LevelId < 1000000)
                {
                    /*if (level.Avatar.GetUnits().Count < 10)
                    {
                        for (int i = 0; i < 31; i++)
                        {
                            Data unitData = CSVManager.DataTables.GetDataById(4000000 + i);
                            CharacterData combatData = (CharacterData)unitData;
                            int maxLevel = combatData.GetUpgradeLevelCount();
                            DataSlot unitSlot = new DataSlot(unitData, 1000);

                            level.Avatar.GetUnits().Add(unitSlot);
                            level.Avatar.SetUnitUpgradeLevel(combatData, maxLevel - 1);
                        }

                        for (int i = 0; i < 18; i++)
                        {
                            Data spellData = CSVManager.DataTables.GetDataById(26000000 + i);
                            SpellData combatData = (SpellData)spellData;
                            int maxLevel = combatData.GetUpgradeLevelCount();
                            DataSlot spellSlot = new DataSlot(spellData, 1000);

                            level.Avatar.GetSpells().Add(spellSlot);
                            level.Avatar.SetUnitUpgradeLevel(combatData, maxLevel - 1);
                        }
                    }*/
                    this.Device.PlayerState = State.SEARCH_BATTLE;
                    new NpcDataMessage(Device, this.Device.Player, this).Send();
                }
            }
        }
    }
}

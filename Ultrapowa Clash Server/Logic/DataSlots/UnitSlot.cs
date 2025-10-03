using UCS.Files.Logic;
using UCS.Helpers.Binary;

namespace UCS.Logic
{
    internal class UnitSlot
    {
        public int Count;
        public int Level;
        public CombatItemData UnitData;

        public UnitSlot(CombatItemData cd, int level, int count)
        {
            UnitData = cd;
            Level    = level;
            Count    = count;
        }

        public void Decode(Reader br)
        {
            UnitData = (CombatItemData) br.ReadDataReference();
            Level    = br.ReadInt32();
            Count    = br.ReadInt32();
        }
    }
}

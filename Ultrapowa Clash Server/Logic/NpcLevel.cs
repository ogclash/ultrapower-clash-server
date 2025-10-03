using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    internal class NpcLevel
    {
        int m_vType = 0x01036640;
        public int Id => m_vType + Index;
        public int Index { get; set; }
        public  int LootedElixir { get; set; }
        public int LootedGold { get; set; }
        public string Name { get; set; }
        public int Stars { get; set; }

        public NpcLevel(int index)
        {
            Index        = index;
            Stars        = 0;
            LootedGold   = 0;
            LootedElixir = 0;
        }
        
        public JObject Save(JObject jsonObject)
        {
            jsonObject.Add("Id", Id);
            jsonObject.Add("GlobalId", m_vType);
            jsonObject.Add("Index", Index);
            jsonObject.Add("LootedElixir", LootedElixir);
            jsonObject.Add("LootedGold", LootedGold);
            jsonObject.Add("Stars", Stars);
            return jsonObject;
        }
    }
}
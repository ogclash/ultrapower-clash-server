using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    internal class TriggerComponent : Component
    {
        const int m_vType = 0x01AB3F00;
        bool triggered = false;

        public void RepairTrap()
        {
            triggered = false;
        }
        
        public void TriggerTrap()
        {
            triggered = true;
        }

        public override int Type => 8;

        public override void Load(JObject jsonObject)
        {
            if (jsonObject["needs_repair"] != null)
            {
                triggered = jsonObject["needs_repair"].ToObject<bool>();
            }
        }

        public override JObject Save(JObject jsonObject)
        {
            if (triggered)
                jsonObject.Add("needs_repair", triggered);
            return jsonObject;
        }
    }
}

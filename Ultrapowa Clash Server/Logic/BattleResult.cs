using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    public class BattleResult
    {
        public JArray units = new JArray();
        public JArray spells = new JArray();
        public JArray levels = new JArray();
        public JArray cc_units = new JArray();
    }
}
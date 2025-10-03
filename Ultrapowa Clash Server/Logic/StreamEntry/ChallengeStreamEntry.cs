using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UCS.Helpers.List;

namespace UCS.Logic.StreamEntry
{
    internal class ChallengeStreamEntry : StreamEntry
    {
        public override byte[] Encode()
        {
            List<byte> data = new List<byte>();
            data.AddRange(base.Encode());
            data.AddString(Message);
            data.AddInt(0);
            return data.ToArray();
        }

        public override int GetStreamEntryType() => 12;

        public override void Load(JObject jsonObject)
        {
            base.Load(jsonObject);
            Message = jsonObject["message"].ToObject<string>();
        }

        public override JObject Save(JObject jsonObject)
        {
            jsonObject = base.Save(jsonObject);
            jsonObject.Add("message", Message);
            return jsonObject;
        }
    }
}
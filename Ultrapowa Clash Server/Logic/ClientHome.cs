using System.Collections.Generic;
using UCS.Helpers.List;

namespace UCS.Logic
{
    internal class ClientHome
    {
        internal long Id;
        internal string Village;
        internal int ShieldTime;
        internal int ProtectionTime;

        public ClientHome()
        {
        }


        public byte[] Encode
        {
            get
            {
                List<byte> data = new List<byte>();
                data.AddLong(this.Id);

                data.AddInt(this.ShieldTime); // Shield
                data.AddInt(this.ProtectionTime); // Protection

                data.AddInt(0);
                data.AddCompressed(Village);
                data.AddCompressed("{\"event\":[]}");
                return data.ToArray();
            }
        }
    }
}

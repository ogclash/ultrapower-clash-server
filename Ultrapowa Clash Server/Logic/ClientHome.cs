using System;
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
        internal int TimeOut;

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

                data.AddInt(TimeOut);
                data.AddCompressed(Village);
                data.AddCompressed("{\"event\":[]}");
                return data.ToArray();
            }
        }
    }
}

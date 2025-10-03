using UCS.Core.Crypto;
using UCS.Helpers.List;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Client;

namespace UCS.Packets.Messages.Server
{
    // Packet 20100
    internal class HandshakeSuccess : Message
    {

        public HandshakeSuccess(Device client, SessionRequest cka) : base(client)
        {
            this.Identifier = 20100;
            this.Device.PlayerState = State.SESSION_OK;
        }

        internal override void Encode()
        {
            this.Data.AddInt(24);
            this.Data.AddRange(Key.NonceKey);
        }
    }
}

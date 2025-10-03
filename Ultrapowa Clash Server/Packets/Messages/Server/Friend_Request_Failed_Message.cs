using UCS.Helpers.List;

namespace UCS.Packets.Messages.Server
{
    internal class Friend_Request_Failed_Message : Message
    {
        public Friend_Request_Failed_Message(Device client) : base(client)
        {
            this.Identifier = 20112;
        }

        internal override void Encode()
        {
            this.Data.AddInt(3); // 1 = Send too much Reuqests; 2 = too many pending Requests; 3 = Failed to send Message;
        }
    }
}

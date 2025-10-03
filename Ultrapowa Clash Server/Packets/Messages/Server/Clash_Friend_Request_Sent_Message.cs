using UCS.Helpers.List;

namespace UCS.Packets.Messages.Server
{
    internal class Clash_Friend_Request_Sent_Message : Message
    {
        public Clash_Friend_Request_Sent_Message(Device client, long FriendID) : base(client)
        {
            this.Identifier = 20106;
            this.Friend_ID = Friend_ID;
        }

        private int Friend_ID { get; set; }

        internal override async void Encode()
        {
            this.Data.AddLong(Friend_ID);
            this.Data.Add(1); // 1 = Sent, 0 = Canceled;
            // still some data missing, research needed
        }
    }
}

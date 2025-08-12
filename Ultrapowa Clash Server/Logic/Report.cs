using UCS.Packets;

namespace UCS.Logic
{
    internal class Report
    {
        internal Report(Device client)
        {
            reporterId = client.Player.Avatar.UserId;
        }

        public long reporterId;
        public Timer timer;
        public long reportedMessageId;
    }
}
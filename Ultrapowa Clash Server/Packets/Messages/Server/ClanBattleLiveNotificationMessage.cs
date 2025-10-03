namespace UCS.Packets.Messages.Server
{
    internal class ClanBattleLiveNotificationMessage : Message
    {
        public ClanBattleLiveNotificationMessage(Device _Device) : base(_Device)
        {
            this.Identifier = 25006;
        }

        internal override void Encode()
        {
        }
    }
}

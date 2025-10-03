using UCS.Helpers.Binary;


namespace UCS.Packets.Commands.Client
{
    internal class DecreaseShieldTimeInfo : Command
    {
        public DecreaseShieldTimeInfo(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }
        
        internal override void Decode()
        {
            this.Reader.ReadData();
        }

        internal override void Process()
        {
            this.Device.ShieldInfo = true;
        }
    }
}

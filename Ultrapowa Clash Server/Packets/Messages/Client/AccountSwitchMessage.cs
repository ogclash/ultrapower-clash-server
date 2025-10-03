using UCS.Helpers.Binary;

namespace UCS.Packets.Messages.Client
{
    class AccountSwitchMessage : Message
    {
        public AccountSwitchMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
        }

        internal override async void Process()
        {
        }
    }
}

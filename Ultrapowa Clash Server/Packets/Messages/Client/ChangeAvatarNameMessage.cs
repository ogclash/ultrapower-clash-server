using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 10212
    internal class ChangeAvatarNameMessage : Message
    {
        public ChangeAvatarNameMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        string PlayerName { get; set; }  

        internal override void Decode()
        {
            this.PlayerName = this.Reader.ReadString();
        }

        internal override void Process()
        {
            if (string.IsNullOrEmpty(PlayerName) || PlayerName.Length > 15)
            {
                ResourcesManager.DisconnectClient(Device);
            }
            else
            {
                this.Device.Player.Avatar.SetName(PlayerName);
                this.Device.Player.Avatar.m_vNameChangingLeft--;
                AvatarNameChangeOkMessage p = new AvatarNameChangeOkMessage(this.Device)
                {
                    AvatarName = this.Device.Player.Avatar.AvatarName
                };
                p.Send();
            }
            this.Device.Player.Avatar.TutorialStepsCount = 13;
        }
    }
}

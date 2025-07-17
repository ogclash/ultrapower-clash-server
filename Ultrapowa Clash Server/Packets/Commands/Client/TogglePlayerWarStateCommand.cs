using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Commands.Client
{
    // Packet 570
    internal class TogglePlayerWarStateCommand : Command
    {
        public TogglePlayerWarStateCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            Alliance a = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
            if (a != null)
            {
                AllianceMemberEntry _AllianceMemberEntry = a.m_vAllianceMembers[this.Device.Player.Avatar.UserId];
                _AllianceMemberEntry.ToggleStatus();
                
                // Save the alliance after changing member status
                await Resources.DatabaseManager.Save(a);
                
                PlayerWarStatusMessage _PlayerWarStatusMessage = new PlayerWarStatusMessage(this.Device)
                {
                    Status = _AllianceMemberEntry.WarOptInStatus
                };
                _PlayerWarStatusMessage.Send();
            }
        }
    }
}

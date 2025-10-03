using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14336
    internal class AskForAllianceWarHistoryMessage : Message
    {
        public AskForAllianceWarHistoryMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        long AllianceID { get; set; }
        long WarID { get; set; }

        internal override void Decode()
        {
            this.AllianceID = this.Reader.ReadInt64();
            this.WarID      = this.Reader.ReadInt64();
        }

        internal override async void Process()
        {
            try
            {
                Alliance all = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                new AllianceWarHistoryMessage(Device, all).Send();
            }
            catch (Exception)
            {
            }
        }
    }
}
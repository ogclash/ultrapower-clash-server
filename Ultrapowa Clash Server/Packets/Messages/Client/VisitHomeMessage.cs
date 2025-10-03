using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Client
{
    // Packet 14113
    internal class VisitHomeMessage : Message
    {
        public VisitHomeMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal long AvatarId;

        internal override void Decode()
        {
            this.AvatarId = this.Reader.ReadInt64();
        }

        internal override async void Process()
        {
            try
            {
                Level targetLevel = await ResourcesManager.GetPlayer(AvatarId);
                targetLevel.Tick();
                if (this.Device.Player.Avatar.minorversion >= 709)
                    new VisitedHomeDataMessage(Device, targetLevel, this.Device.Player).Send();
                else
                    new VisitedHomeDataForOldClients(Device, targetLevel, this.Device.Player).Send();
                
                Logger.Say($"{this.Device.Player.Avatar.AvatarName} [{this.Device.Player.Avatar.UserId}] visits {targetLevel.Avatar.AvatarName} [{targetLevel.Avatar.UserId}]");
                if (this.Device.Player.Avatar.AllianceId > 0)
                {
                    Alliance alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                    if (alliance != null)
                    {
                        new AllianceStreamMessage(Device, alliance).Send();
                    }
                    
                    this.Device.Player.Avatar.SendCLanMessagesToOldClient(this.Device);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

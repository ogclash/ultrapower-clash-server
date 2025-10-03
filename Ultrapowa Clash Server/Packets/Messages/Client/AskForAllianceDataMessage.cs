using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Client
{
    // Packet 14302
    internal class AskForAllianceDataMessage : Message
    {
        long m_vAllianceId;

        public AskForAllianceDataMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            this.m_vAllianceId = this.Reader.ReadInt64();
        }

        internal override async void Process()
        {
            try
            {
                Alliance alliance = ObjectManager.GetAlliance(m_vAllianceId);
                if (this.Device.Player.Avatar.minorversion >= 709)
                    new AllianceDataMessage(Device, alliance).Send(); 
                else
                    new AllianceDataForOldClients(Device, alliance).Send();   
            }
            catch (Exception)
            {
            }
        }
    }
}
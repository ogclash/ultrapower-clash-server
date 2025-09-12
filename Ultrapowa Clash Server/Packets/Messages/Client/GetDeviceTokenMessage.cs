using System;
using System.IO;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 10113
    internal class GetDeviceTokenMessage : Message
    {
        public GetDeviceTokenMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            new SetDeviceTokenMessage(Device).Send();
            if (!this.Device.Player.Avatar.m_vAndroid && this.Device.OpenUDID != null)
            {
                string fileName = this.Device.OpenUDID + ".txt";
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    fileName
                );
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}
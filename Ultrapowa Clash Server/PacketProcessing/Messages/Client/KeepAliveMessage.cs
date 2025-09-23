using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UCS.Old.Helpers;
using UCS.Logic;
using UCS.Network;
using UCS.Packets;

namespace UCS.PacketProcessing
{
    //Packet 10108
    class KeepAliveMessage : Message
    {
        public KeepAliveMessage(Device client, BinaryReader br)
            : base(client, br)
        {
        }

        public override void Decode()
        {
        }

        public override void Process(Level level)
        {
            PacketManager.ProcessOutgoingPacket(new KeepAliveOkMessage(this.Client,this));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCS.Logic;
using UCS.Core;
using UCS.Old.Helpers;
using UCS.Packets;
using UCS.Packets.Messages.Client;

namespace UCS.PacketProcessing
{
    //Packet 24133
    class NpcDataMessage : Message
    {
        public NpcDataMessage(Device client, Level level, AttackNpcMessage cnam) : base (client)
        {
            SetMessageType(24133);

            this.Player = level;

            JsonBase = ObjectManager.NpcLevels[(int)cnam.LevelId - 0x01036640];
          
            LevelId = cnam.LevelId;
        }

        public override async void Encode()
        {
            List<Byte> data = new List<Byte>();

            data.AddInt32(0);
            data.AddInt32(JsonBase.Length);
            data.AddRange(System.Text.Encoding.ASCII.GetBytes(JsonBase));
            data.AddRange(await Player.Avatar.Encode());
            data.AddInt32(0);
            data.AddInt32(LevelId);

            SetData(data.ToArray());
        }

        public String JsonBase { get; set; }
        public int LevelId { get; set; }
        public Level Player { get; set; }
    }
}

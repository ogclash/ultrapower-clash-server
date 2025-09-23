using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using UCS.Logic;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Old.Helpers;
using UCS.Packets;
using UCS.Utilities.ZLib;

namespace UCS.PacketProcessing
{
    //Packet 24101
    class OwnHomeDataMessage : Message
    {

        private byte[] m_vSerializedVillage { get; set; }

        public OwnHomeDataMessage(Device client, Level level) : base (client)
        {
            SetMessageType(24101);
            this.Player = level;
        }

        public override async void Encode()
        {
            List<Byte> data = new List<Byte>();
            
            data.AddInt32(0);//replace previous after patch
            data.AddInt32(-1);
            data.AddInt32(Player.Avatar.LastUpdate); //0x54, 0x47, 0xFD, 0x10 //patch 21/10
            data.AddRange(EncodeVillage());
            data.AddRange(await Player.Avatar.Encode());

            //7.1
            data.AddInt32(0);
            data.AddInt32(0);

            SetData(data.ToArray());
        }
        public virtual byte[] EncodeBase()
        {
            var data = new List<byte>();
            data.AddInt(0);
            return data.ToArray();
        }
        
        public byte[] EncodeVillage()
        {
            byte[] Village = ZlibStream.CompressString(JsonConvert.SerializeObject(Player.GameObjectManager.Save()));
            var data = new List<byte>();
            data.AddRange(EncodeBase());
            data.AddLong(Player.Avatar.UserId);
            data.AddInt(0);
            data.AddInt(1800);
            data.AddInt(0);
            data.AddInt(1200);
            data.AddInt(60);
            data.Add(1);
            data.AddInt(Village.Length + 4);
            data.AddRange(new byte[]
            {
                //0xED, 0x0D, 0x00, 0x00,
                0xFF, 0xFF, 0x00, 0x00
            });
            data.AddRange(Village);

            return data.ToArray();
        }

        public Level Player { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Utilities.ZLib;

namespace UCS.Packets.Messages.Server.Support
{
    internal class EnemyHomeDataForOldClients : Message
    {
        
        
        public EnemyHomeDataForOldClients(Device client, Level ownerLevel, Level visitorLevel) : base(client)
        {
            this.Identifier = 24107;
            this.OwnerLevel = ownerLevel;
            this.VisitorLevel = visitorLevel;
        }

        internal override async void Encode()
        {
            try
            {
                this.Data.AddRange(new byte[]
                {
                    0x00, 0x00, 0x00, 0xF0,
                    0xFF, 0xFF, 0xFF, 0xFF,
                    0x54, 0xCE, 0x5C, 0x4A
                });
                this.Data.AddRange(EncodeVillage(OwnerLevel));
                this.Data.AddRange(await OwnerLevel.Avatar.EncodeForOldVersion());
                this.Data.AddRange(await VisitorLevel.Avatar.EncodeForOldVersion());
                this.Data.AddRange(new byte[] {0x00, 0x00, 0x00, 0x03, 0x00});
                this.Data.AddInt(200);
                this.Data.AddInt(100);
                this.Data.AddInt(0);
                this.Data.AddInt(0);
                this.Data.Add(0);
            }
            catch (Exception)
            {
            }
        }
        public virtual byte[] EncodeBase()
        {
            var data = new List<byte>();
            data.AddInt(0);
            return data.ToArray();
        }
        
        public byte[] EncodeVillage(Level Player)
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

        internal readonly Level OwnerLevel;
        internal readonly Level VisitorLevel;
    }
}
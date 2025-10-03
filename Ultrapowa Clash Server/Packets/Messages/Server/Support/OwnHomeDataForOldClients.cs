using System.Collections.Generic;
using Newtonsoft.Json;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Utilities.ZLib;

namespace UCS.Packets.Messages.Server.Support
{
    internal class OwnHomeDataForOldClients : Message
    {
        public OwnHomeDataForOldClients(Device client, Level level) : base(client)
        {
            this.Identifier = 24101;
            this.Player = level;
            this.Player.Tick();
            Avatar = Player.Avatar;
        }

        public Level Player { get; set; }
        private ClientAvatar Avatar;

        internal override async void Encode()
        {
            this.Data.AddInt(0);

            this.Data.AddInt(-1);

            this.Data.AddInt(Player.Avatar.LastUpdate);
            
            this.Data.AddRange(EncodeVillage());

            this.Data.AddRange(await Avatar.EncodeForOldVersion());

            this.Data.AddInt(200);
            this.Data.AddInt(100);
            this.Data.AddInt(0);
            this.Data.AddInt(0);
            this.Data.Add(0);
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
            data.AddLong(Avatar.UserId);
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
    }
}
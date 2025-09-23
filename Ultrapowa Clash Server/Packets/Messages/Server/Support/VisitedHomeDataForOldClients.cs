using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Utilities.ZLib;

namespace UCS.Packets.Messages.Server.Support
{
    internal class VisitedHomeDataForOldClients : Message
    {
        private bool challenge;
        public VisitedHomeDataForOldClients(Device client, Level ownerLevel, Level visitorLevel, bool challenge = false) : base(client)
        {
            this.Identifier = 24113;
            OwnerLevel = ownerLevel;
            VisitorLevel = visitorLevel;
            this.Device.PlayerState = Logic.Enums.State.VISIT;
            this.challenge = challenge;
        }

        internal override async void Encode()
        {
            try
            {
                this.Data.AddRange(BitConverter.GetBytes(OwnerLevel.Avatar.LastUpdate).Reverse());
                this.Data.AddRange(EncodeVillage(OwnerLevel));
                this.Data.AddRange(await OwnerLevel.Avatar.EncodeForOldVersion());
                this.Data.Add(1);
                this.Data.AddRange(await VisitorLevel.Avatar.EncodeForOldVersion());
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
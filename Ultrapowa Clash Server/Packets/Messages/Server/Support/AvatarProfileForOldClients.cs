using Newtonsoft.Json;
using UCS.Helpers.List;
using UCS.Logic;
using UCS.Utilities.ZLib;

namespace UCS.Packets.Messages.Server.Support
{
    internal class AvatarProfileForOldClients : Message
    {
        public Level Player { get; set; }

        public AvatarProfileForOldClients(Device client, Level level) : base(client)
        {
            this.Identifier = 24334;
            this.Player = level;
            this.Player.Tick();
        }

        internal override async void Encode()
        {
            byte[] Village = ZlibStream.CompressString(JsonConvert.SerializeObject(Player.GameObjectManager.Save()));
            
            this.Data.AddRange(await Player.Avatar.EncodeForOldVersion());
            
            this.Data.AddInt(Village.Length + 4);
            this.Data.AddInt(unchecked((int) 0xFFFF0000));
            this.Data.AddRange(Village);

            this.Data.AddInt(200);
            this.Data.AddInt(100);
            this.Data.AddInt(0);
            this.Data.AddInt(0);
            this.Data.Add(0);
        }
    }
}
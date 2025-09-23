using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Client
{
    internal class RetributionAttackerMessage : Message
    {
        private int battle_id;
        public RetributionAttackerMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            var unknown = this.Reader.ReadDecimal();
            this.battle_id = this.Reader.ReadInt32();
        }


        internal override async void Process()
        {
            ClientAvatar p = this.Device.Player.Avatar;
            if (this.Device.PlayerState == Logic.Enums.State.IN_BATTLE)
            {
                ResourcesManager.DisconnectClient(Device);
            }
            else
            {
                JObject battle = Device.Player.Avatar.battles[battle_id-1];
                Level defender = await ResourcesManager.GetPlayer((long)battle["attacker"]);
                if (Device.Player.Avatar.revenged.Contains(defender.Avatar.UserId) || ResourcesManager.IsPlayerOnline(defender))
                {
                    if (this.Device.Player.Avatar.minorversion >= 709)
                        new OwnHomeDataMessage(Device, this.Device.Player).Send();
                    else
                        new OwnHomeDataForOldClients(this.Device, this.Device.Player).Send();
                }
                else
                {
                    Device.AttackVictim = defender;
                    Device.Player.Avatar.revenged.Add(defender.Avatar.UserId);
                    new EnemyHomeDataMessage(this.Device, defender, this.Device.Player).Send();
                }
            }
        }
    }
}

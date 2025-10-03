using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;
using UCS.Packets.Messages.Server.Support;

namespace UCS.Packets.Messages.Client
{
    // Packet 14134
    internal class AttackNpcMessage : Message
    {
        public AttackNpcMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public int LevelId { get; set; }

        internal override void Decode()
        {
            this.LevelId = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            if (this.Device.PlayerState == State.IN_BATTLE)
            {
                this.Device.AttackInfo = "npc";
                ResourcesManager.DisconnectClient(Device);
            }
            else
            {
                this.Device.AttackInfo = "npc";
                if (LevelId > 0 || LevelId < 1000000)
                {
                    this.Device.PlayerState = State.IN_BATTLE;
                    if (this.Device.Player.Avatar.minorversion >= 709)
                        new NpcDataMessage(Device, this.Device.Player, this).Send();
                    else
                        new NpcDataForOldClients(Device, this.Device.Player, this).Send();
                }

                if (LevelId == 17000001 && this.Device.Player.Avatar.TutorialStepsCount < 10)
                {
                    this.Device.Player.Avatar.TutorialStepsCount = 10;
                }
            }

            this.Device.AttackedNpc = LevelId - 17000000;
        }
    }
}

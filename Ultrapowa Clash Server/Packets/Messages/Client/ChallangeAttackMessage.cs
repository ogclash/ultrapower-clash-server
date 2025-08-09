using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    internal class ChallangeAttackMessage : Message
    {
        public ChallangeAttackMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public long ID { get; set; }

        internal override void Decode()
        {
            var unknown = this.Reader.ReadInt32();
            this.ID = this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                Alliance a = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                StreamEntry _Stream = a.m_vChatMessages.Find(c => c.ID == ID);
                
                Level defender = await ResourcesManager.GetPlayer(_Stream.SenderID); // TODO: FIX BUGS		
                defender.Tick();
                this.Device.AttackInfo = "challenge";
                new EnemyHomeDataMessage(this.Device, defender, this.Device.Player).Send();
                
                if (this.Device.Player.Avatar.AllianceId > 0)
                {
                    Alliance alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                    if (alliance != null)
                    {
                        //new AllianceStreamMessage(this.Device, alliance).Send();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

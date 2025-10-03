using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14317
    internal class JoinRequestAllianceMessage : Message
    {
        public JoinRequestAllianceMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public string Message;

        public long ID;

        internal override void Decode()
        {
            this.ID      = this.Reader.ReadInt64();
            this.Message = this.Reader.ReadString();
        }


        internal override async void Process()
        {
            try
            {
                if (Message.Length > 0 && Message.Length < 100)
                {
                    ClientAvatar player = this.Device.Player.Avatar;
                    Alliance all = ObjectManager.GetAlliance(ID);

                    InvitationStreamEntry cm = new InvitationStreamEntry {ID = all.m_vChatMessages.Count > 0 ? all.m_vChatMessages.Last().ID + 1 : 1};
                    cm.SetSender(player);
                    cm.SetMessage(Message);
                    cm.SetState(1);
                    all.AddChatMessage(cm);

                    foreach (AllianceMemberEntry op in all.GetAllianceMembers())
                    {
                        Level playera = await ResourcesManager.GetPlayer(op.AvatarId);
                        if (playera.Client != null)
                        {
                            new AllianceStreamEntryMessage(playera.Client) {StreamEntry = cm}.Send();
                        }
                    }
                }
                else
                {
                    ResourcesManager.DisconnectClient(this.Device);
                }
            } catch (Exception) { }
        }
    }
}
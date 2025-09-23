using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Commands
{
    internal class ChallangeCommand : Command
    {
        public string Message;
        public int layoutId;

        public ChallangeCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Message = this.Reader.ReadString();
            this.layoutId = this.Reader.ReadByte();
        }

        internal override async void Process()
        {
            try
            {
                ClientAvatar player = this.Device.Player.Avatar;
                long allianceID = player.AllianceId;
                Alliance alliance = ObjectManager.GetAlliance(allianceID);
                
                ChallengeStreamEntry cm = new ChallengeStreamEntry() { Message = Message };
                cm.SetSender(player);
                cm.ID = alliance.m_vChatMessages.Count > 0 ? alliance.m_vChatMessages.Last().ID + 1 : 1;
                
                StreamEntry oldmessage = alliance.m_vChatMessages.Find(c => c.GetStreamEntryType() == 12);
                alliance.m_vChatMessages.Remove(oldmessage);
                alliance.AddChatMessage(cm);
                foreach (AllianceMemberEntry op in alliance.GetAllianceMembers())
                {
                    Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                    if (aplayer.Avatar.minorversion < 709)
                        continue;
                    if (aplayer.Client != null)
                    {
                        if (oldmessage != null)
                        {
                            new AllianceStreamEntryRemovedMessage(aplayer.Client, oldmessage.ID).Send();
                        }
                        new AllianceStreamEntryMessage(aplayer.Client) { StreamEntry = cm }.Send();
                    }
                }

                /*if (s != null)
                {
                    alliance.m_vChatMessages.RemoveAll(t => t == s);
                }
                alliance.AddChatMessage(cm);*/
            }
            catch (Exception)
            {
            }
        }
    }
}

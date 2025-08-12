using System;
using System.IO;
using System.Threading.Tasks;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14322
    internal class AllianceInviteMessage : Message
    {
        internal long invited_userid;
        public AllianceInviteMessage(Device device, Reader reader) : base(device, reader)
        {
        }
        
        internal override void Decode()
        {
            invited_userid = this.Reader.ReadInt64();
        }

        internal override async void Process()
        {
            Level targetAccount = await ResourcesManager.GetPlayer(invited_userid);
            if (targetAccount.Avatar.GetAllianceCastleLevel() == -1)
            {
                return;
            }
            foreach (AvatarStreamEntry message in targetAccount.Avatar.messages)
            {
                if (message.GetStreamEntryType() == 4)
                {
                    AllianceInviteStreamEntry ai = (AllianceInviteStreamEntry) message;
                    if (ai.m_vSenderId == Device.Player.Avatar.UserId &&
                        ai.AllianceId == Device.Player.Avatar.AllianceId)
                    {
                        return;
                    }
                }
            }
            var alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
            var allianceInviteMessage = new AllianceInviteStreamEntry();
            allianceInviteMessage.SetSender(Device.Player.Avatar);
            allianceInviteMessage.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            allianceInviteMessage.SenderId = Device.Player.Avatar.UserId;
            allianceInviteMessage.IsNew = 2;
            allianceInviteMessage.AllianceId = (alliance.m_vAllianceId);
            allianceInviteMessage.AllianceBadgeData = (alliance.m_vAllianceBadgeData);
            allianceInviteMessage.AllianceName = (alliance.m_vAllianceName);
            var p = new AvatarStreamEntryMessage(targetAccount.Client);
            p.SetTargetAcc(targetAccount);
            p.SetAvatarStreamEntry(allianceInviteMessage);
            p.Send();
        }
    }
}
using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Commands.Client
{
    // Packet 537
    internal class SendAllianceMailCommand : Command
    {
        internal string m_vMailContent;

        public SendAllianceMailCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.m_vMailContent = this.Reader.ReadString();
            this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                var avatar = this.Device.Player.Avatar;
                var allianceId = avatar.AllianceId;
                if (allianceId > 0)
                {
                    var alliance = ObjectManager.GetAlliance(allianceId);
                    if (alliance != null)
                    {
                        var mail = new AllianceMailStreamEntry();
                        mail.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        mail.SetSender(avatar);
                        mail.IsNew = 2;
                        mail.SenderId = avatar.UserId;
                        mail.AllianceId = allianceId;
                        mail.AllianceBadgeData = alliance.m_vAllianceBadgeData;
                        mail.AllianceName = alliance.m_vAllianceName;
                        mail.Message = m_vMailContent;
                        var allianceMembers =  ObjectManager.GetAlliance(allianceId).GetAllianceMembers();
                        foreach (var member in allianceMembers)
                        {
                            var player = await ResourcesManager.GetPlayer(member.AvatarId);
                            var p = new AvatarStreamEntryMessage(player.Client);
                            p.SetTargetAcc(player);
                            p.SetAvatarStreamEntry(mail);
                            p.Send();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
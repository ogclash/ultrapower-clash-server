using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 10117
    internal class ReportPlayerMessage : Message
    {
        public ReportPlayerMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public long ReportedPlayerID { get; set; }

        public int Tick { get; set; }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.ReportedPlayerID = this.Reader.ReadInt64();
            this.Reader.ReadInt32();
        }

        internal override async void Process()
        {
            try
            {
                Level ReportedPlayer = await ResourcesManager.GetPlayer(ReportedPlayerID);
                Report report = new Report(this.Device);
                if (ReportedPlayer.Avatar.reports.Find(r => r.reporterId == Device.Player.Avatar.UserId) != null)
                {
                    return;
                }
                ReportedPlayer.Avatar.ReportedTimes++;
                if (ReportedPlayer.Avatar.ReportedTimes >= 3)
                {
                    AvatarChatBanMessage _AvatarChatBanMessage = new AvatarChatBanMessage(ReportedPlayer.Client);
                    _AvatarChatBanMessage.SetBanPeriod(1800); // 30 Minutes
                    _AvatarChatBanMessage.Send();
                    Timer timer = new Timer();
                    timer.StartTime = DateTime.Now;
                    timer.Seconds = 1800;
                    report.timer = timer;
                }
                ReportedPlayer.Avatar.reports.Add(report);
            }
            catch (Exception)
            {
            }
        }
    }
}

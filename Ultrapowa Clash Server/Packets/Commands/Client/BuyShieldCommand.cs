using System;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 522
    internal class BuyShieldCommand : Command
    {
        public BuyShieldCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.ShieldId = this.Reader.ReadInt32();
            this.Tick = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            ClientAvatar player = this.Device.Player.Avatar;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (ShieldId == 20000000)
            {
                long shieldEnd = player.m_vProtectionTimeStamp + player.m_vProtectionTime;
                long remaining = Math.Max(0, shieldEnd - now);
                var shieldData = (ShieldData)CSVManager.DataTables.GetDataById(ShieldId);
                if (remaining == 0)
                {
                    player.m_vProtectionTimeStamp = now;
                }
                player.m_vProtectionTime += 7200;
                player.m_vProtectionTimeValue += 7200;
                player.m_vProtectionTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                player.UseDiamonds(shieldData.Diamonds);
            }
            else
            {
                long shieldEnd = player.mv_ShieldTimeStamp + player.m_vShieldTime;
                long remaining = Math.Max(0, shieldEnd - now);
                var shieldData = (ShieldData)CSVManager.DataTables.GetDataById(ShieldId);
                if (remaining == 0)
                {
                    player.mv_ShieldTimeStamp = now;
                }

                player.m_vShieldTime += Convert.ToInt32(TimeSpan.FromHours((shieldData).TimeH).TotalSeconds);
                player.m_vShieldTimeValue += Convert.ToInt32(TimeSpan.FromHours((shieldData).TimeH).TotalSeconds);
                player.UseDiamonds(shieldData.Diamonds);
            }
        }

        public int ShieldId;
        public int Tick;
    }
}

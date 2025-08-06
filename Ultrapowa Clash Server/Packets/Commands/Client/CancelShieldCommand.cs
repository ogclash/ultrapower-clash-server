using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 534
    internal class CancelShieldCommand : Command
    {
        public CancelShieldCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Tick = this.Reader.ReadInt32();
        }

        public int Tick;

        internal override void Process()
        { 
            ClientAvatar player = this.Device.Player.Avatar;
            if (player.m_vShieldTime == 0)
            { 
                player.m_vProtectionTime = 0;
                player.m_vProtectionTimeValue = 0;
                player.m_vProtectionTimeStamp = 0;
            }
            player.m_vShieldTime = 0;
            player.m_vShieldTimeValue = 0;
            player.mv_ShieldTimeStamp = 0;
        }
    }      
}

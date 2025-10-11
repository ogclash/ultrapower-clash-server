using System;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets
{
    internal class GameOpCommand
    {
        byte m_vRequiredAccountPrivileges;

        public static void SendCommandFailedMessage(Device c, string reason = "")
        {
            Console.WriteLine("GameOp command failed. Requster ID -> " + c.Player.Avatar.UserId);
            var p = new GlobalChatLineMessage(c)
            {
                Message = "GameOp command failed. " + reason,
                HomeId = 0,
                CurrentHomeId = 0,
                LeagueId = 22,
                PlayerName = "Server"
            };
            p.Send();
        }

        public virtual void Execute(Level level)
        {
        }

        public byte GetRequiredAccountPrivileges() => m_vRequiredAccountPrivileges;

        public void SetRequiredAccountPrivileges(byte level)
        {
            m_vRequiredAccountPrivileges = level;
        }
    }
}
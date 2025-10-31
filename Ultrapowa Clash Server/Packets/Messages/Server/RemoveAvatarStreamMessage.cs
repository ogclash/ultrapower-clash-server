using System.Collections.Generic;
using UCS.Helpers.List;

namespace UCS.Packets.Messages.Server
{
    internal class RemoveAvatarStreamMessage : Message
    {
        long userID;
        public RemoveAvatarStreamMessage(Device client, long userid) : base(client)
        {
            this.Identifier = (ushort)AvatarChangeType.DIAMOND;
            this.userID = userid;
        }

        internal override void Encode()
        {
            this.Data.AddInt(5);
        }

        private byte[] encodeMessage()
        {
            List<byte> data = new List<byte>();
            data.AddInt(4);
            data.AddLong(userID);
            data.AddLong(this.Device.Player.Avatar.messages[0].ID);
            return data.ToArray();
        }
        public enum AvatarChangeType
        {
            DIAMOND,
            COMMODITY_COUNT,
            WAR_PREFERENCE,
            EXP_POINTS,
            EXP_LEVEL,
            ALLIANCE_JOINED,
            ALLIANCE_LEFT,
            ALLIANCE_LEVEL,
            ALLIANCE_UNIT_REMOVED,
            ALLIANCE_UNIT_ADDED,
            ALLIANCE_UNIT_COUNT,
            ALLIANCE_CASTLE_LEVEL,
            TOWN_HALL_LEVEL,
            TOWN_HALL_V2_LEVEL,
            LEGEND_SEASON_SCORE,
            SCORE,
            DUEL_SCORE,
            LEAGUE,
            ATTACK_SHIELD_REDUCE_COUNTER,
            DEFENSE_VILLAGE_GUARD_COUNTER,
            RED_PACKAGE_STATE_CHANGED,
            NAME
        }
        
    }
}
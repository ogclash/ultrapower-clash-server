using System;
using System.Collections.Generic;
using UCS.Helpers.List;

namespace UCS.Logic.AvatarStreamEntry
{
    internal class AllianceInviteStreamEntry: AvatarStreamEntry
    {
        public long AllianceId;
        public string AllianceName;
        public int AllianceBadgeData;
        public long SenderId;
        public int AllianceLevel;
        
        public override byte[] Encode()
        {
            List<byte> data = new List<byte>();
            data.AddRange(base.Encode());
            data.AddInt((int)(DateTime.UtcNow - m_vCreationTime).TotalSeconds);
            data.AddBool(false);
            
            data.AddLong(AllianceId);
            data.AddString(AllianceName);
            data.AddInt(AllianceBadgeData);
            
            data.AddBool(true);
            data.AddLong(SenderId);
            data.AddInt(AllianceLevel);
            return data.ToArray();
        }
        
        public override int GetStreamEntryType() => 4;
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UCS.Core;
using System.Threading.Tasks;
using UCS.Helpers.List;

namespace UCS.Logic
{
    internal class AllianceMemberEntry
    { 
        public AllianceMemberEntry(long avatarId)
        {
            AvatarId       = avatarId;
            IsNewMember    = 0;
            Order          = 1;
            PreviousOrder  = 1;
            Role           = 1;
            DonatedTroops  = 200;
            ReceivedTroops = 100;
            WarCooldown    = 0;
            WarOptInStatus = 1;
            createdTime = DateTime.Now;
        }

        internal int DonatedTroops;
        internal byte IsNewMember;
        internal DateTime createdTime;
        internal int ReceivedTroops;
        internal int[] RoleTable = { 1, 1, 4, 2, 3 };
        internal int WarCooldown;
        internal int WarOptInStatus;
        internal long AvatarId;
        internal int Order;
        internal int PreviousOrder;
        internal int Role;
        
        public async Task<byte[]> Encode()
        {
            List<byte> data = new List<byte>();
            Level avatar = await ResourcesManager.GetPlayer(AvatarId);
            data.AddLong(AvatarId);
            if(avatar.Avatar.AvatarName != null)
            {
                data.AddString(avatar.Avatar.AvatarName);
                data.AddInt(Role);
                data.AddInt(avatar.Avatar.m_vAvatarLevel);
                data.AddInt(avatar.Avatar.m_vLeagueId);
                data.AddInt(avatar.Avatar.GetScore());
                data.AddInt(avatar.Avatar.m_vDonated);
                data.AddInt(avatar.Avatar.m_vReceived);
            }
            else
            {
                data.AddString("Player can't be loaded");
                data.AddInt(Role);
                data.AddInt(1);
                data.AddInt(1);
                data.AddInt(400);
                data.AddInt(0);
                data.AddInt(0);
            } 
            data.AddInt(Order);
            data.AddInt(PreviousOrder);
            data.AddInt((int)(DateTime.UtcNow - (DateTime)createdTime).TotalSeconds);
            data.AddInt(WarCooldown);
            data.AddInt(WarOptInStatus);
            data.Add(1);
            data.AddLong(AvatarId);
            return data.ToArray();
        }

        public bool HasLowerRoleThan(int role)
        {
            bool result = true;
            if (role < RoleTable.Length && Role < RoleTable.Length)
            {
                if (RoleTable[Role] >= RoleTable[role])
                    result = false;
            }
            return result;
        }

        public void Load(JObject jsonObject)
        {
            AvatarId = jsonObject["avatar_id"].ToObject<long>();
            Role = jsonObject["role"].ToObject<int>();
            createdTime = jsonObject["created_time"]?.ToObject<DateTime>() ?? DateTime.Now;
            if (createdTime.ToString() == "1/1/0001 12:00:00 AM" || createdTime.ToString() == "01.01.0001 00:00:00")
                createdTime = DateTime.Now;
            //Logger.Say(createdTime.ToString());
            WarOptInStatus = jsonObject["war_opt_in"].ToObject<int>() == 1 ? 1 : 0;
        }

        public JObject Save(JObject jsonObject)
        {
            jsonObject.Add("avatar_id", AvatarId);
            jsonObject.Add("role", Role);
            jsonObject.Add("war_opt_in", WarOptInStatus);
            jsonObject.Add("created_time", createdTime);
            return jsonObject;
        }

        public void ToggleStatus() => WarOptInStatus = WarOptInStatus == 1 ? 0 : 1;
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server
{
    // Packets 24411
    internal class AvatarStreamMessage : Message
    {
        private JObject BattleResult;
        private bool update;
        public AvatarStreamMessage(Device client, bool update = false) : base(client)
        {
            this.update = update;
            this.Identifier = 24411;
        }

        internal override async void Encode()
        {
            List<JObject> battleResults = Device.Player.Avatar.battles;
            this.Data.AddInt(battleResults.Count);
            int count = 1;
            foreach (JObject BattleResult in battleResults)
            {
                int type = 7; // default: attacked
                Level pl;

                long defenderId = (long)BattleResult["defender"];
                long attackerId = (long)BattleResult["attacker"];

                if (defenderId == Device.Player.Avatar.UserId)
                {
                    type = 2; // defended
                    pl = await ResourcesManager.GetPlayer(attackerId);
                }
                else
                {
                    pl = await ResourcesManager.GetPlayer(defenderId);
                }
                if (pl == null)
                    continue;
                ClientAvatar avatar = pl.Avatar;
                JObject jsonList = (JObject)BattleResult["result"];
                if (avatar.AllianceId != 0)
                {
                    Alliance alliance = ObjectManager.GetAlliance(avatar.AllianceId);
                    JObject stats = (JObject)jsonList["stats"];
                    try
                    {
                        stats.Add("allianceBadge", alliance.m_vAllianceBadgeData);
                        stats.Add("allianceName", alliance.m_vAllianceName);
                    }
                    catch (Exception)
                    {
                        stats["allianceBadge"]= alliance.m_vAllianceBadgeData;
                        stats["allianceName"]= alliance.m_vAllianceName;
                    }
                    stats["homeID"] = new JArray(0, avatar.UserId);
                    jsonList["stats"] = stats;
                }
                string newJson = jsonList.ToString(Newtonsoft.Json.Formatting.None);

                // Stream data for each battle
                this.Data.AddInt(type);              
                this.Data.AddLong(count);                
                this.Data.Add(1);
                this.Data.AddInt(avatar.HighID);     
                this.Data.AddInt(avatar.LowID);      
                this.Data.AddString(avatar.AvatarName);
                this.Data.AddInt(avatar.m_vAvatarLevel);
                this.Data.AddInt(0);
                this.Data.AddInt(0);
                if ((int)BattleResult["new"] == 2)
                {
                    this.Data.Add(2);
                    if (update)
                        BattleResult["new"] = 0;
                } else
                    this.Data.Add(0);                    
                this.Data.AddString(newJson);        
                this.Data.AddInt(0);
                this.Data.Add(1);
                this.Data.AddInt(8);
                this.Data.AddInt(709);
                this.Data.AddInt(0);
                this.Data.Add(1);
                this.Data.AddLong(1);
                this.Data.AddInt(int.MaxValue);
                count++;
            }
            Device.Player.Avatar.battles = battleResults;
        }
    }
}

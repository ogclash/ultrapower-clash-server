using System;
using System.Collections.Generic;
using System.Linq;
using MaxMind.GeoIP2.Model;
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

            if (!update)
            {
                this.Data.AddInt(1);
                JObject battleResult = battleResults.Last();
                int type = 7; // default: attacked
                Level pl;

                long defenderId = (long)battleResult["defender"];
                long attackerId = (long)battleResult["attacker"];

                if (defenderId == Device.Player.Avatar.UserId)
                {
                    type = 2; // defended
                    pl = await ResourcesManager.GetPlayer(attackerId);
                }
                else
                {
                    if (battleResult["timestamp_s"] != null && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)battleResult["timestamp_s"] <= 16 * 60 * 60)
                        Device.Player.Avatar.attackedPlayers.Add(defenderId);
                    pl = await ResourcesManager.GetPlayer(defenderId);
                }
                if (pl == null)
                    return;
                ClientAvatar avatar = pl.Avatar;
                JObject jsonList = (JObject)battleResult["result"];
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
                this.Data.AddLong(battleResults.Count);                
                this.Data.Add(1);
                this.Data.AddInt(avatar.HighID);     
                this.Data.AddInt(avatar.LowID);      
                this.Data.AddString(avatar.AvatarName);
                this.Data.AddInt(avatar.m_vAvatarLevel);
                if (battleResult["timestamp_s"] != null)
                {
                    this.Data.AddInt(0);
                    this.Data.AddInt((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (int)battleResult["timestamp_s"]);
                }
                else
                {
                    this.Data.AddInt(0);
                    this.Data.AddInt(0);
                }
                if ((int)battleResult["new"] == 2)
                {
                    this.Data.Add(2);
                    if (update)
                        battleResult["new"] = 0;
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
                return;
            }
            if (battleResults.Count > 80)
                battleResults = battleResults.Skip(battleResults.Count - 80).ToList();
            this.Data.AddInt(battleResults.Count);

            int count = 1;
            foreach (JObject battleResult in battleResults)
            {
                //BattleResult = battleResults[i];
                int type = 7; // default: attacked
                Level pl;

                long defenderId = (long)battleResult["defender"];
                long attackerId = (long)battleResult["attacker"];

                if (defenderId == Device.Player.Avatar.UserId)
                {
                    type = 2; // defended
                    pl = await ResourcesManager.GetPlayer(attackerId);
                }
                else
                {
                    if (battleResult["timestamp_s"] != null && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)battleResult["timestamp_s"] <= 16 * 60 * 60)
                        Device.Player.Avatar.attackedPlayers.Add(defenderId);
                    pl = await ResourcesManager.GetPlayer(defenderId);
                }
                if (pl == null)
                    continue;
                ClientAvatar avatar = pl.Avatar;
                JObject jsonList = (JObject)battleResult["result"];
                if (avatar.AllianceId != 0)
                {
                    Alliance alliance = null;
                    JObject stats = (JObject)jsonList["stats"];
                    try
                    {
                        alliance = ObjectManager.GetAlliance(avatar.AllianceId);
                        stats["allianceBadge"]= alliance.m_vAllianceBadgeData;
                        stats["allianceName"]= alliance.m_vAllianceName;
                    }
                    catch (Exception) 
                    {
                        stats.Remove("allianceBadge");
                        stats.Remove("allianceName");
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
                if (battleResult["timestamp_s"] != null)
                {
                    this.Data.AddInt(0);
                    this.Data.AddInt((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (int)battleResult["timestamp_s"]);
                }
                else
                {
                    this.Data.AddInt(0);
                    this.Data.AddInt(0);
                }
                if ((int)battleResult["new"] == 2)
                {
                    this.Data.Add(2);
                    if (update)
                        battleResult["new"] = 0;
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
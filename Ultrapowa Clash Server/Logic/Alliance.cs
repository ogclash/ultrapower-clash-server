using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Logic.StreamEntry;
using UCS.Helpers.List;

namespace UCS.Logic
{
    internal class Alliance
    {
        const int m_vMaxAllianceMembers = 50;
        const int m_vMaxChatMessagesNumber = 100;
        internal readonly Dictionary<long, AllianceMemberEntry> m_vAllianceMembers;
        internal readonly List<StreamEntry.StreamEntry> m_vChatMessages;
        internal int m_vAllianceBadgeData;
        internal string m_vAllianceDescription;
        internal int m_vAllianceExperience;
        internal int m_vAllianceExperienceInternal;
        internal long m_vAllianceId;
        internal int m_vAllianceLevel;
        internal string m_vAllianceName;
        internal int m_vAllianceOrigin;
        internal int m_vAllianceType;
        internal int m_vDrawWars;
        internal int m_vLostWars;
        internal int m_vRequiredScore;
        internal int m_vScore;
        internal int m_vWarFrequency;
        internal byte m_vWarLogPublic;
        internal int m_vWonWars;
        internal byte m_vFriendlyWar;

        public Alliance()
        {
            m_vChatMessages    = new List<StreamEntry.StreamEntry>();
            m_vAllianceMembers = new Dictionary<long, AllianceMemberEntry>();
        }

        public Alliance(long id)
        {
            m_vAllianceId          = id;
            m_vAllianceName        = "Default";
            m_vAllianceDescription = "Default";
            m_vAllianceBadgeData   = 0;
            m_vAllianceType        = 0;
            m_vRequiredScore       = 0;
            m_vWarFrequency        = 0;
            m_vAllianceOrigin      = 32000001;
            m_vScore               = 0;
            m_vAllianceExperience  = 0;
            m_vAllianceLevel       = 1;
            m_vWonWars             = 0;
            m_vLostWars            = 0;
            m_vDrawWars            = 0;
            m_vChatMessages        = new List<StreamEntry.StreamEntry>();
            m_vAllianceMembers     = new Dictionary<long, AllianceMemberEntry>();
        }

        public void AddAllianceMember(AllianceMemberEntry entry) => m_vAllianceMembers.Add(entry.AvatarId, entry);

        public void AddChatMessage(StreamEntry.StreamEntry message)
        {
            while (m_vChatMessages.Count >= m_vMaxChatMessagesNumber)
            {
                m_vChatMessages.RemoveAt(0);
            }
            m_vChatMessages.Add(message);
        }

        public byte[] EncodeFullEntry(bool old=false)
        {
            List<byte> data = new List<byte>();
            data.AddLong(this.m_vAllianceId);
            data.AddString(this.m_vAllianceName);
            data.AddInt(this.m_vAllianceBadgeData);
            data.AddInt(this.m_vAllianceType);
            data.AddInt(this.m_vAllianceMembers.Count);
            data.AddInt(this.m_vScore);
            data.AddInt(this.m_vRequiredScore);
            data.AddInt(this.m_vWonWars);
            data.AddInt(this.m_vLostWars);
            data.AddInt(this.m_vDrawWars);
            data.AddInt(20000001);
            data.AddInt(this.m_vWarFrequency);
            data.AddInt(this.m_vAllianceOrigin);
            data.AddInt(this.m_vAllianceExperience);
            data.AddInt(this.m_vAllianceLevel);
            data.AddInt(0);
            if (!old)
            {
                data.AddInt(0);
                data.Add(this.m_vWarLogPublic);
                data.Add(this.m_vFriendlyWar);
            }
            return data.ToArray();
        }

        public byte[] EncodeHeader()
        {
            List<byte> data = new List<byte>();
            data.AddLong(this.m_vAllianceId);
            data.AddString(this.m_vAllianceName);
            data.AddInt(this.m_vAllianceBadgeData);
            data.Add(0);
            data.AddInt(this.m_vAllianceLevel);
            data.AddInt(1);
            data.AddInt(-1);
            return data.ToArray();
        }

        public List<AllianceMemberEntry> GetAllianceMembers() => m_vAllianceMembers.Values.ToList();

        public bool IsAllianceFull() => m_vAllianceMembers.Count >= m_vMaxAllianceMembers;

        public async void LoadFromJSON(string jsonString)
        {
            try {
                JObject jsonObject = JObject.Parse(jsonString);
                m_vAllianceId = jsonObject["alliance_id"].ToObject<long>();
                m_vAllianceName = jsonObject["alliance_name"].ToObject<string>();
                m_vAllianceBadgeData = jsonObject["alliance_badge"].ToObject<int>();
                m_vAllianceType = jsonObject["alliance_type"].ToObject<int>();
                m_vRequiredScore = jsonObject["required_score"].ToObject<int>();
                m_vAllianceDescription = jsonObject["description"].ToObject<string>();
                m_vAllianceExperience = jsonObject["alliance_experience"].ToObject<int>();
                m_vAllianceExperienceInternal = jsonObject["alliance_experience_internal"]?.ToObject<int>() ?? m_vAllianceExperience;
                m_vAllianceLevel = jsonObject["alliance_level"].ToObject<int>();
                m_vWarLogPublic = jsonObject["war_log_public"].ToObject<byte>();
                m_vFriendlyWar = jsonObject["friendly_war"].ToObject<byte>();
                m_vWonWars = jsonObject["won_wars"].ToObject<int>();
                m_vLostWars = jsonObject["lost_wars"].ToObject<int>();
                m_vDrawWars = jsonObject["draw_wars"].ToObject<int>();
                m_vWarFrequency = jsonObject["war_frequency"].ToObject<int>();
                m_vAllianceOrigin = jsonObject["alliance_origin"].ToObject<int>();
                JArray jsonMembers = (JArray)jsonObject["members"];
                foreach (JToken jToken in jsonMembers)
                {
                    try {
                        JObject jsonMember = (JObject)jToken;
                        long id = jsonMember["avatar_id"].ToObject<long>();
                        Level pl = await ResourcesManager.GetPlayer(id);
                        if (pl.Avatar.AllianceId == 0)
                            continue;
                        AllianceMemberEntry member = new AllianceMemberEntry(id);
                        m_vScore += pl.Avatar.GetScore();
                        member.Load(jsonMember);
                        m_vAllianceMembers.Add(id, member);
                    } catch (Exception) { }
                }
                m_vScore /= 2;
                JArray jsonMessages = (JArray)jsonObject["chatMessages"];
                if (jsonMessages != null)
                {
                    foreach (JToken jToken in jsonMessages)
                    {
                        try {
                            JObject jsonMessage = (JObject)jToken;
                            StreamEntry.StreamEntry se = new StreamEntry.StreamEntry();
                            if (jsonMessage["type"].ToObject<int>() == 1)
                            {
                                TroopRequestStreamEntry cm = new TroopRequestStreamEntry();
                                cm.ID = jsonMessage["id"].ToObject<int>();
                                cm.Message = jsonMessage["message"].ToObject<string>();

                                cm.m_vDonatedSpell = jsonMessage["donated_spell"].ToObject<int>();
                                cm.m_vDonatedTroop = jsonMessage["donated_troop"].ToObject<int>();

                                cm.m_vMaxTroop = jsonMessage["max_troop"].ToObject<int>();
                                cm.m_vMaxSpell = jsonMessage["max_spell"].ToObject<int>();
        
                                cm.SenderID = jsonMessage["sender_id"].ToObject<long>();
                                cm.m_vHomeId = jsonMessage["home_id"].ToObject<long>();
                                cm.m_vSenderLevel = jsonMessage["sender_level"].ToObject<int>();
                                cm.m_vSenderName = jsonMessage["sender_name"].ToObject<string>();
                                cm.m_vSenderLeagueId = jsonMessage["sender_leagueId"].ToObject<int>();
                                cm.m_vSenderRole = jsonMessage["sender_role"].ToObject<int>();
                                cm.m_vMessageTime = jsonMessage["message_time"].ToObject<DateTime>();
                                cm.m_vState = jsonMessage["state"].ToObject<int>();

                                AddChatMessage(cm);
                            }
                            else if (jsonMessage["type"].ToObject<int>() == 2)
                            {
                                ChatStreamEntry cm = new ChatStreamEntry();
                                cm.ID = jsonMessage["id"].ToObject<int>();
                                
                                cm.SenderID = jsonMessage["sender_id"].ToObject<long>();
                                cm.m_vHomeId = jsonMessage["home_id"].ToObject<long>();
                                cm.m_vSenderName = jsonMessage["sender_name"].ToObject<string>();
                                cm.m_vSenderLeagueId =jsonMessage["sender_leagueId"].ToObject<int>();
                                cm.m_vSenderLevel = jsonMessage["sender_level"].ToObject<int>();
                                cm.m_vMessageTime = jsonMessage["message_time"].ToObject<DateTime>();
                                cm.m_vSenderRole = jsonMessage["sender_role"].ToObject<int>();
                                cm.m_vJudge = "";
                                
                                cm.Message = jsonMessage["message"].ToObject<string>();
                                AddChatMessage(cm);
                            }
                            else if (jsonMessage["type"].ToObject<int>() == 3)
                            {
                                InvitationStreamEntry cm = new InvitationStreamEntry();
                                cm.ID = jsonMessage["id"].ToObject<int>();
                                cm.SenderID = jsonMessage["sender_id"].ToObject<long>();
                                cm.m_vHomeId = jsonMessage["home_id"].ToObject<long>();
                                cm.m_vSenderName = jsonMessage["sender_name"].ToObject<string>();
                                cm.m_vSenderLeagueId = jsonMessage["sender_leagueId"].ToObject<int>();
                                cm.m_vSenderLevel = jsonMessage["sender_level"].ToObject<int>();
                                cm.m_vMessageTime = jsonMessage["message_time"].ToObject<DateTime>();
                                cm.m_vSenderRole = jsonMessage["sender_role"].ToObject<int>();
                                cm.m_vJudge = jsonMessage["judge"].ToObject<string>();
                                cm.m_vState = jsonMessage["state"].ToObject<int>();
                                AddChatMessage(cm);
                            }
                            else if (jsonMessage["type"].ToObject<int>() == 4)
                            {
                                AllianceEventStreamEntry cm  = new AllianceEventStreamEntry();
                                cm.ID = jsonMessage["id"].ToObject<int>();
                                cm.SenderID = jsonMessage["sender_id"].ToObject<long>();
                                cm.m_vHomeId = jsonMessage["home_id"].ToObject<long>();
                                cm.m_vSenderName = jsonMessage["sender_name"].ToObject<string>();
                                cm.m_vSenderLeagueId = jsonMessage["sender_leagueId"].ToObject<int>();
                                cm.m_vSenderLevel = jsonMessage["sender_level"].ToObject<int>();
                                cm.m_vMessageTime = jsonMessage["message_time"].ToObject<DateTime>();
                                cm.m_vSenderRole = jsonMessage["sender_role"].ToObject<int>();
                                cm.EventType = jsonMessage["eventtype"].ToObject<int>();
                                cm.m_vAvatarName = jsonMessage["avatar_name"].ToObject<string>();
                                AddChatMessage(cm);
                            }
                            else if (jsonMessage["type"].ToObject<int>() == 5)
                                se = new ShareStreamEntry();
                        } 
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }

        public void RemoveMember(long avatarId) => m_vAllianceMembers.Remove(avatarId);

        public string SaveToJSON()
        {
            JObject jsonData = new JObject();
            jsonData.Add("alliance_id", m_vAllianceId);
            jsonData.Add("alliance_name", m_vAllianceName);
            jsonData.Add("alliance_badge", m_vAllianceBadgeData);
            jsonData.Add("alliance_type", m_vAllianceType);
            jsonData.Add("score", m_vScore);
            jsonData.Add("required_score", m_vRequiredScore);
            jsonData.Add("description", m_vAllianceDescription);
            jsonData.Add("alliance_experience", m_vAllianceExperience);
            jsonData.Add("alliance_experience_internal", m_vAllianceExperienceInternal);
            jsonData.Add("alliance_level", m_vAllianceLevel);
            jsonData.Add("war_log_public", m_vWarLogPublic);
            jsonData.Add("friendly_war", m_vFriendlyWar);
            jsonData.Add("won_wars", m_vWonWars);
            jsonData.Add("lost_wars", m_vLostWars);
            jsonData.Add("draw_wars", m_vDrawWars);
            jsonData.Add("war_frequency", m_vWarFrequency);
            jsonData.Add("alliance_origin", m_vAllianceOrigin);
            JArray jsonMembersArray = new JArray();
            foreach (AllianceMemberEntry member in m_vAllianceMembers.Values)
            {
                try
                {
                    JObject jsonObject = new JObject();
                    member.Save(jsonObject);
                    jsonMembersArray.Add(jsonObject);
                }
                catch (Exception) { }
            }
            jsonData.Add("members", jsonMembersArray);
            JArray jsonMessageArray = new JArray();
            for (int i = 0; i < m_vChatMessages.Count(); i++)
            { try {
                    var entry = m_vChatMessages[i];
                    var type = entry.GetStreamEntryType();
                    JObject jsonObject = new JObject();
                    if (type == 2)
                    {
                        ChatStreamEntry message = entry as ChatStreamEntry;
                        jsonObject.Add("type", type);
                        jsonObject.Add("id", message.ID);
                        jsonObject.Add("sender_id", message.SenderID);
                        jsonObject.Add("home_id", message.m_vHomeId);
                        jsonObject.Add("sender_level", message.m_vSenderLevel);
                        jsonObject.Add("sender_name", message.m_vSenderName);
                        jsonObject.Add("sender_leagueId", message.m_vSenderLeagueId);
                        jsonObject.Add("sender_role", message.m_vSenderRole);
                        jsonObject.Add("message_time", message.m_vMessageTime);
                        jsonObject.Add("message", message.Message);
                        jsonMessageArray.Add(jsonObject);
                    } else if (type == 4)
                    {
                        AllianceEventStreamEntry message = entry as AllianceEventStreamEntry;
                        if (message.EventType == 0)
                        {
                            continue;
                        }
                        jsonObject.Add("type", type);
                        jsonObject.Add("id", message.ID);
                        jsonObject.Add("eventtype", message.EventType);
                        jsonObject.Add("sender_id", message.SenderID);
                        jsonObject.Add("home_id", message.m_vHomeId);
                        jsonObject.Add("sender_level", message.m_vSenderLevel);
                        jsonObject.Add("sender_name", message.m_vSenderName);
                        jsonObject.Add("sender_leagueId", message.m_vSenderLeagueId);
                        jsonObject.Add("sender_role", message.m_vSenderRole);
                        jsonObject.Add("message_time", message.m_vMessageTime);
                        jsonObject.Add("avatar_name", message.m_vAvatarName);
                        jsonMessageArray.Add(jsonObject);
                    } else if (type == 3)
                    {
                        InvitationStreamEntry  message = entry as InvitationStreamEntry;
                        jsonObject.Add("type", type);
                        jsonObject.Add("id", message.ID);
                        jsonObject.Add("sender_id", message.SenderID);
                        jsonObject.Add("home_id", message.m_vHomeId);
                        jsonObject.Add("sender_level", message.m_vSenderLevel);
                        jsonObject.Add("sender_name", message.m_vSenderName);
                        jsonObject.Add("sender_leagueId", message.m_vSenderLeagueId);
                        jsonObject.Add("sender_role", message.m_vSenderRole);
                        jsonObject.Add("message_time", message.m_vMessageTime);
                        jsonObject.Add("state", message.m_vState);
                        jsonObject.Add("judge", message.m_vJudge);
                        jsonMessageArray.Add(jsonObject);
                    } else if (type == 1)
                    {
                        TroopRequestStreamEntry message = entry as TroopRequestStreamEntry;
                        jsonObject.Add("type", type);
                        jsonObject.Add("id", message.ID);
                        jsonObject.Add("message", message.Message);
                        jsonObject.Add("donated_spell", message.m_vDonatedSpell);
                        jsonObject.Add("donated_troop", message.m_vDonatedTroop);
                        jsonObject.Add("max_troop", message.m_vMaxTroop);
                        jsonObject.Add("max_spell", message.m_vMaxSpell);
                        jsonObject.Add("sender_id", message.SenderID);
                        jsonObject.Add("home_id", message.m_vHomeId);
                        jsonObject.Add("sender_level", message.m_vSenderLevel);
                        jsonObject.Add("sender_name", message.m_vSenderName);
                        jsonObject.Add("sender_leagueId", message.m_vSenderLeagueId);
                        jsonObject.Add("sender_role", message.m_vSenderRole);
                        jsonObject.Add("message_time", message.m_vMessageTime);
                        jsonObject.Add("state", message.m_vState);
                        jsonMessageArray.Add(jsonObject);
                    }
                } catch (Exception) { }
            }
            jsonData.Add("chatMessages", jsonMessageArray);
            return JsonConvert.SerializeObject(jsonData);
        }

        public void SetWarAndFriendlytStatus(byte status)
        {
            switch (status)
            {
                case 1:
                    m_vWarLogPublic = 1;
                    break;
                case 2:
                    m_vWarLogPublic = 1;
                    break;
                case 3:
                    m_vWarLogPublic = 1;
                    m_vFriendlyWar = 1;
                    break;
                case 0:
                    m_vWarLogPublic = 0;
                    m_vFriendlyWar = 0;
                    break;
            }
        }
    }
}

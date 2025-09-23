using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;
using static System.Convert;
using static System.Configuration.ConfigurationManager;
using UCS.Logic.DataSlots;
using System.Threading.Tasks;
using UCS.Core.Network;
using UCS.Helpers.List;
using UCS.Logic.AvatarStreamEntry;
using UCS.Logic.StreamEntry;
using UCS.Packets;
using UCS.Packets.Messages.Server;

namespace UCS.Logic
{
    internal class ClientAvatar : Avatar
    {
        // Long
        internal long AllianceId = 0;
        internal long CurrentHomeId;
        internal long UserId;

        // Int
        internal int mayorversion;
        internal int minorversion;
        internal int HighID;
        internal int LowID;
        internal int m_vAvatarLevel;
        internal int m_vCurrentGems;
        internal int m_vExperience = 0;
        internal int m_vLeagueId;
        internal int m_vScore;
        internal int m_vDonatedUnits;
        internal int m_vRecievedUnits;
        internal int m_vActiveLayout;
        internal int m_vAlliance_Gold = 0;
        internal int m_vAlliance_Elixir = 0;
        internal int m_vAlliance_DarkElixir = 0;
        internal int m_vShieldTime;
        internal int m_vShieldTimeValue;
        internal long mv_ShieldTimeStamp;
        internal int m_vProtectionTime;
        internal int m_vProtectionTimeValue;
        internal long m_vProtectionTimeStamp;
        internal int ReportedTimes = 0;
        internal List<Report> reports = new List<Report>();
        internal int m_vDonated;
        internal int m_vReceived;
        
        internal int account_switch = 0;
        internal int old_account = 0;
        
        internal int attacks_won = 0;
        internal int defenses_won = 0;

        // UInt
        internal uint TutorialStepsCount = 0x00;

        // Byte
        internal byte m_vNameChangingLeft = 0x02;
        internal byte m_vnameChosenByUser = 0x00;

        internal byte AccountPrivileges = 0x00;

        // String
        internal string AvatarName;
        internal string UserToken;
        internal string Region;
        internal string FacebookId;
        internal string FacebookToken;
        internal string GoogleId;
        internal string GoogleToken;
        internal string IPAddress;
        internal string TroopRequestMessage;
        
        internal string account_password = "";

        // Boolean
        internal bool m_vPremium = false;
        internal bool m_vAndroid;
        internal bool AccountBanned = false;

        //Datetime
        internal DateTime m_vAccountCreationDate;
        internal DateTime LastTickSaved;
        
        List<int[]> buildings = new List<int[]>();
        public List<NpcLevel> NpcLevels = new List<NpcLevel>();
        
        public BattleResult battle = new BattleResult();
        public List<long> revenged = new List<long>();

        public List<JObject> battles = new List<JObject>();
        public List<AvatarStreamEntry.AvatarStreamEntry> messages = new List<AvatarStreamEntry.AvatarStreamEntry>();


        public List<int[]> getBuildings()
        {
            return this.buildings;
        }

        public void setBuidlings(List<int[]> adbuildings)
        {
            this.buildings = adbuildings;
        }

        public ClientAvatar()
        {
            Achievements = new List<DataSlot>();
            AchievementsUnlocked = new List<DataSlot>();
            AllianceUnits = new List<DonationSlot>();
            NpcStars = new List<DataSlot>();
            NpcLootedGold = new List<DataSlot>();
            NpcLootedElixir = new List<DataSlot>();
            BookmarkedClan = new List<BookmarkSlot>();
            QuickTrain1 = new List<DataSlot>();
            QuickTrain2 = new List<DataSlot>();
            QuickTrain3 = new List<DataSlot>();
            for (var i = 0; i < ObjectManager.NpcLevels.Count; i++)
            {
                NpcLevel npcLevel = new NpcLevel(i);
                NpcLevels.Add(npcLevel);
            }
        }

        public ClientAvatar(long id, string token) : this()
        {
            Random rnd = new Random();
            this.LastUpdate = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            this.Login = id.ToString() + (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            this.UserId = id;
            this.HighID = (int)(id >> 32);
            this.LowID = (int)(id & 0xffffffffL);
            this.UserToken = token;
            this.CurrentHomeId = id;
            this.m_vAvatarLevel = ToInt32(AppSettings["startingLevel"]);
            this.EndShieldTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            this.m_vCurrentGems = ToInt32(AppSettings["startingGems"]);
            this.m_vScore = AppSettings["startingTrophies"] == "random"
                ? rnd.Next(1500, 4999)
                : ToInt32(AppSettings["startingTrophies"]);

            this.AvatarName = "NoNameYet";

            SetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"), ToInt32(AppSettings["startingGold"]));
            SetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"), ToInt32(AppSettings["startingElixir"]));
            SetResourceCount(CSVManager.DataTables.GetResourceByName("DarkElixir"),
                ToInt32(AppSettings["startingDarkElixir"]));
            SetResourceCount(CSVManager.DataTables.GetResourceByName("Diamonds"), ToInt32(AppSettings["startingGems"]));
        }

        public List<DataSlot> Achievements { get; set; }
        public List<DataSlot> AchievementsUnlocked { get; set; }
        public List<DonationSlot> AllianceUnits { get; set; }
        public int EndShieldTime { get; set; }
        public int LastUpdate { get; set; }
        public string Login { get; set; }
        public List<DataSlot> NpcLootedElixir { get; set; }
        public List<DataSlot> NpcLootedGold { get; set; }
        public List<DataSlot> NpcStars { get; set; }
        public List<BookmarkSlot> BookmarkedClan { get; set; }
        public List<DataSlot> QuickTrain1 { get; set; }
        public List<DataSlot> QuickTrain2 { get; set; }
        public List<DataSlot> QuickTrain3 { get; set; }

        private void updateLeague()
        {
            DataTable table = CSVManager.DataTables.GetTable(12);
            int i = 0;
            bool found = false;
            while (!found)
            {
                var league = (LeagueData)table.GetItemAt(i);
                if (m_vScore <= league.BucketPlacementRangeHigh[league.BucketPlacementRangeHigh.Count - 1] &&
                    m_vScore >= league.BucketPlacementRangeLow[0])
                {
                    found = true;
                    m_vLeagueId = i;
                }
                i++;
            }
        }

        public void AddDiamonds(int diamondCount)
        {
            this.m_vCurrentGems += diamondCount;
        }

        public void AddExperience(int exp)
        {
            m_vExperience += exp;
            var experienceCap =
                ((ExperienceLevelData)CSVManager.DataTables.GetTable(10).GetDataByName(m_vAvatarLevel.ToString()))
                .ExpPoints;
            if (m_vExperience >= experienceCap)
                if (CSVManager.DataTables.GetTable(10).GetItemCount() > m_vAvatarLevel + 1)
                {
                    m_vAvatarLevel += 1;
                    m_vExperience = m_vExperience - experienceCap;
                }
                else
                    m_vExperience = 0;
        }

        public List<DataSlot> getHerostate()
        {
            return m_vHeroState;
        }

        public void setHeroState(List<DataSlot> heroState)
        {
            m_vHeroState = heroState;
        }

        public void SendCLanMessagesToOldClient(Device client)
        {
            if (this.AllianceId > 0 && this.minorversion < 709)
            {
                Alliance alliance = ObjectManager.GetAlliance(this.AllianceId);
                foreach (StreamEntry.StreamEntry test in alliance.m_vChatMessages)
                {
                    try
                    {
                        if (test.m_vUnitDonation != null)
                        {
                            TroopRequestStreamEntry tr = (TroopRequestStreamEntry)test;
                            new AllianceStreamEntryMessage(client) { StreamEntry = tr }.Send();
                        } else if (test.m_vJudge != "")
                        {
                            InvitationStreamEntry ie = (InvitationStreamEntry)test;
                            new AllianceStreamEntryMessage(client) { StreamEntry = ie }.Send();
                        }
                        else
                        {
                            ChatStreamEntry cm = (ChatStreamEntry)test;
                            if (cm.Message != null)
                                new AllianceStreamEntryMessage(client) { StreamEntry = cm }.Send();
                        }
                    } catch(Exception) {}
                }
            }
        }

        public async Task<byte[]> EncodeForOldVersion()
        {
            var data = new List<byte>();

            data.AddInt(0);
            data.AddLong(UserId);
            data.AddLong(UserId);
            if (AllianceId != 0)
            {
                data.Add(1);
                data.AddLong(AllianceId);
                var alliance = ObjectManager.GetAlliance(AllianceId);
                data.AddString(alliance.m_vAllianceName);
                data.AddInt(alliance.m_vAllianceBadgeData);
                data.AddInt(await GetAllianceRole());
                data.AddInt(alliance.m_vAllianceLevel);
            }
            data.Add(0);
            //7.156
            data.AddInt(0); //1
            data.AddInt(0); //2
            data.AddInt(0); //3
            data.AddInt(0); //4
            data.AddInt(0); //5
            data.AddInt(0); //6
            data.AddInt(0); //7
            data.AddInt(0); //8
            data.AddInt(0); //9
            data.AddInt(0); //10
            data.AddInt(1); //11

            data.AddInt(m_vLeagueId);

            data.AddInt(GetAllianceCastleLevel());
            data.AddInt(GetAllianceCastleTotalCapacity());
            data.AddInt(GetAllianceCastleUsedCapacity());
            data.AddInt(0);
            data.AddInt(0);
            data.AddInt(m_vTownHallLevel);
            data.AddString(AvatarName);
            data.AddInt(-1);
            data.AddInt(m_vAvatarLevel);
            data.AddInt(m_vExperience);
            data.AddInt(m_vCurrentGems);
            data.AddInt(m_vCurrentGems);
            data.AddInt(1200);
            data.AddInt(60);
            data.AddInt(m_vScore);

            data.AddInt(100); //Attack win
            data.AddInt(0);
            data.AddInt(100);
            data.AddInt(0);

            data.AddInt(0);
            data.AddInt(0);
            data.AddInt(0);
            data.Add(1);
            data.AddLong(0);

            data.Add(m_vnameChosenByUser);

            data.AddInt(0);
            data.AddInt(0);
            data.AddInt(0);
            data.AddInt(1);

            data.AddInt(1);
            data.AddInt(0);

            data.AddDataSlots(GetResourceCaps());
            data.AddDataSlots(GetResources());
            data.AddDataSlots(GetUnits());
            data.AddDataSlots(GetSpells());
            data.AddDataSlots(m_vUnitUpgradeLevel);
            data.AddDataSlots(m_vSpellUpgradeLevel);
            data.AddDataSlots(m_vHeroUpgradeLevel);
            data.AddDataSlots(m_vHeroHealth);
            data.AddDataSlots(m_vHeroState);

            data.AddRange(BitConverter.GetBytes(AllianceUnits.Count).Reverse());
            foreach (DonationSlot u in AllianceUnits)
            {
                data.AddInt(u.ID);
                data.AddInt(u.Count);
                data.AddInt(u.UnitLevel);
            }

            data.AddRange(BitConverter.GetBytes(TutorialStepsCount).Reverse());
            for (uint i = 0; i < TutorialStepsCount; i++)
                data.AddRange(BitConverter.GetBytes(0x01406F40 + i).Reverse());

            data.AddRange(BitConverter.GetBytes(Achievements.Count).Reverse());
            foreach (var a in Achievements)
                data.AddRange(BitConverter.GetBytes(a.Data.GetGlobalID()).Reverse());

            data.AddRange(BitConverter.GetBytes(Achievements.Count).Reverse());
            foreach (var a in Achievements)
            {
                data.AddRange(BitConverter.GetBytes(a.Data.GetGlobalID()).Reverse());
                data.AddRange(BitConverter.GetBytes(0).Reverse());
            }

            data.AddDataSlots(NpcStars);
            data.AddDataSlots(NpcLootedGold);
            data.AddDataSlots(NpcLootedElixir);

            data.AddDataSlots(new List<DataSlot>());
            data.AddDataSlots(new List<DataSlot>());
            data.AddDataSlots(new List<DataSlot>());
            data.AddDataSlots(new List<DataSlot>());

            return data.ToArray();
        }

        public async Task<byte[]> Encode()
        {
            try
            {
                if (AllianceId == 0)
                {
                    m_vDonated = 0;
                    m_vReceived = 0;
                }
                Random rnd = new Random();
                List<byte> data = new List<byte>();
                data.AddLong(this.UserId);
                data.AddLong(this.CurrentHomeId);
                var war_optin = 1;
                if (this.AllianceId != 0)
                {
                    data.Add(1);
                    data.AddLong(this.AllianceId);
                    Alliance alliance = ObjectManager.GetAlliance(this.AllianceId);
                    data.AddString(alliance.m_vAllianceName);
                    data.AddInt(alliance.m_vAllianceBadgeData);
                    data.AddInt(alliance.m_vAllianceMembers[this.UserId].Role);
                    data.AddInt(alliance.m_vAllianceLevel);
                    war_optin = alliance.m_vAllianceMembers[this.UserId].WarOptInStatus;
                }
                data.Add(0);

                if (m_vLeagueId == 22)
                {
                    data.AddInt(m_vScore / 12);
                    data.AddInt(1);
                    int month = DateTime.Now.Month;
                    data.AddInt(month);
                    data.AddInt(DateTime.Now.Year);
                    data.AddInt(rnd.Next(1, 10));
                    data.AddInt(this.m_vScore);
                    data.AddInt(1);
                    if (month == 1)
                    {
                        data.AddInt(12);
                        data.AddInt(DateTime.Now.Year - 1);
                    }
                    else
                    {
                        data.AddInt(month - 1);
                        data.AddInt(DateTime.Now.Year);
                    }
                    data.AddInt(rnd.Next(1, 10));
                    data.AddInt(this.m_vScore / 2);
                }
                else
                {
                    data.AddInt(0); //1
                    data.AddInt(0); //2
                    data.AddInt(0); //3
                    data.AddInt(0); //4
                    data.AddInt(0); //5
                    data.AddInt(0); //6
                    data.AddInt(0); //7
                    data.AddInt(0); //8
                    data.AddInt(0); //9
                    data.AddInt(0); //10
                    data.AddInt(0); //11
                }

                data.AddInt(this.m_vLeagueId);
                data.AddInt(GetAllianceCastleLevel());
                data.AddInt(GetAllianceCastleTotalCapacity());
                data.AddInt(GetAllianceCastleUsedCapacity());
                data.AddInt(0);
                data.AddInt(-1);
                data.AddInt(m_vTownHallLevel);
                data.AddString(this.AvatarName);
                data.AddString(this.FacebookId);
                data.AddInt(this.m_vAvatarLevel);
                data.AddInt(this.m_vExperience);
                data.AddInt(this.m_vCurrentGems);
                data.AddInt(this.m_vCurrentGems);
                data.AddInt(1200);
                data.AddInt(60);
                data.AddInt(m_vScore);
                data.AddInt(attacks_won); // Attack Wins
                data.AddInt(m_vDonated);
                data.AddInt(0); // Attack Loses
                data.AddInt(m_vReceived);

                data.AddInt(this.m_vAlliance_Gold);
                data.AddInt(this.m_vAlliance_Elixir);
                data.AddInt(this.m_vAlliance_DarkElixir);
                data.AddInt(0);
                data.Add(1);
                data.AddLong(946720861000);

                data.Add(this.m_vnameChosenByUser);

                data.AddInt(0);
                data.AddInt(0);
                data.AddInt(0);
                data.AddInt(war_optin);

                data.AddInt(0);
                data.AddInt(0);
                data.Add(0);
                data.AddDataSlots(GetResourceCaps());
                data.AddDataSlots(GetResources());
                data.AddDataSlots(GetUnits());
                data.AddDataSlots(GetSpells());
                data.AddDataSlots(m_vUnitUpgradeLevel);
                data.AddDataSlots(m_vSpellUpgradeLevel);
                data.AddDataSlots(m_vHeroUpgradeLevel);
                data.AddDataSlots(m_vHeroHealth);
                data.AddDataSlots(m_vHeroState);

                data.AddRange(BitConverter.GetBytes(AllianceUnits.Count).Reverse());
                foreach (DonationSlot u in AllianceUnits)
                {
                    data.AddInt(u.ID);
                    data.AddInt(u.Count);
                    data.AddInt(u.UnitLevel);
                }
                
                data.AddRange(BitConverter.GetBytes(TutorialStepsCount).Reverse());
                for (uint i = 0; i < TutorialStepsCount; i++)
                    data.AddRange(BitConverter.GetBytes(0x01406F40 + i).Reverse());

                data.AddRange(BitConverter.GetBytes(Achievements.Count).Reverse());
                foreach (var a in Achievements)
                    data.AddRange(BitConverter.GetBytes(a.Data.GetGlobalID()).Reverse());

                data.AddRange(BitConverter.GetBytes(Achievements.Count).Reverse());
                foreach (var a in Achievements)
                {
                    data.AddRange(BitConverter.GetBytes(a.Data.GetGlobalID()).Reverse());
                    data.AddRange(BitConverter.GetBytes(0).Reverse());
                }

                
                data.AddRange(BitConverter.GetBytes(ObjectManager.NpcLevels.Count).Reverse());
                {
                    foreach (NpcLevel npclevel in NpcLevels)
                    {
                        data.AddRange(BitConverter.GetBytes(npclevel.Id).Reverse());
                        data.AddRange(BitConverter.GetBytes(npclevel.Stars).Reverse());
                    }
                }

                data.AddDataSlots(NpcLootedGold);
                data.AddDataSlots(NpcLootedElixir);
                data.AddDataSlots(new List<DataSlot>());
                data.AddDataSlots(new List<DataSlot>());
                data.AddDataSlots(new List<DataSlot>());
                data.AddDataSlots(new List<DataSlot>());

                data.AddInt(QuickTrain1.Count);
                foreach (var i in QuickTrain1)
                {
                    data.AddInt(i.Data.GetGlobalID());
                    data.AddInt(i.Value);
                }

                data.AddInt(QuickTrain2.Count);
                foreach (var i in QuickTrain2)
                {
                    data.AddInt(i.Data.GetGlobalID());
                    data.AddInt(i.Value);
                }
                data.AddInt(QuickTrain3.Count);
                foreach (var i in QuickTrain3)
                {
                    data.AddInt(i.Data.GetGlobalID());
                    data.AddInt(i.Value);
                }
                data.AddInt(QuickTrain1.Count);
                foreach (var i in QuickTrain1)
                {
                    data.AddInt(i.Data.GetGlobalID());
                    data.AddInt(i.Value);
                }
                data.AddDataSlots(new List<DataSlot>());
                return data.ToArray();
            } catch (Exception) { return null; }
        }

        public async Task<AllianceMemberEntry> GetAllianceMemberEntry()
        {
            try
            {
                Alliance alliance = ObjectManager.GetAlliance(this.AllianceId);
                return alliance?.m_vAllianceMembers[this.UserId];
            } catch (Exception) { return null; }
        }

        public async Task<int> GetAllianceRole()
        {
            try
            {
                var ame = await GetAllianceMemberEntry();
                if (ame != null)
                    return ame.Role;
                return -1;
            } catch (Exception) { return 1; }
        }

        public int GetScore()
        {
            updateLeague();
            return m_vScore;
        }

        public int GetActiveLayout()
        {
            return this.m_vActiveLayout;
        }

        public int GetSecondsFromLastUpdate() => (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - LastUpdate;

        public bool HasEnoughDiamonds(int diamondCount) => m_vCurrentGems >= diamondCount;

        public bool HasEnoughResources(ResourceData rd, int buildCost) => GetResourceCount(rd) >= buildCost;

        public void LoadFromJSON(string jsonString)
        {
            var jsonObject = JObject.Parse(jsonString);
            this.account_switch = jsonObject["acc_switch"]?.ToObject<int>() ?? 0;
            this.attacks_won = jsonObject["attacks_won"]?.ToObject<int>() ?? 0;
            this.account_password = jsonObject["acc_password"]?.ToObject<string>() ?? "";
            this.UserId = jsonObject["avatar_id"].ToObject<long>();
            this.HighID = jsonObject["id_high_int"].ToObject<int>();
            this.LowID = jsonObject["id_low_int"].ToObject<int>();
            this.UserToken = jsonObject["token"].ToObject<string>();
            this.Region = jsonObject["region"].ToObject<string>();
            this.IPAddress = jsonObject["IPAddress"].ToObject<string>();
            
            this.m_vAccountCreationDate = jsonObject["avatar_creation_date"].ToObject<DateTime>();
            this.AccountPrivileges = jsonObject["avatar_privilages"].ToObject<byte>();
            this.AccountBanned = false;
            
            this.m_vActiveLayout = jsonObject["active_layout"].ToObject<int>();
            this.LastTickSaved = jsonObject["last_tick_save"].ToObject<DateTime>();
            this.m_vAndroid = jsonObject["android"].ToObject<bool>();
            this.CurrentHomeId = jsonObject["current_home_id"].ToObject<long>();
            
            SetAllianceCastleLevel(jsonObject["alliance_castle_level"].ToObject<int>());
            if (jsonObject["alliance_castle_level"].ToObject<int>() == -1)
                this.AllianceId = 0;
            else
                this.AllianceId = jsonObject["alliance_id"].ToObject<long>();
            SetAllianceCastleTotalCapacity(jsonObject["alliance_castle_total_capacity"].ToObject<int>());
            SetAllianceCastleUsedCapacity(jsonObject["alliance_castle_used_capacity"].ToObject<int>());
            
            SetTownHallLevel(jsonObject["townhall_level"].ToObject<int>());
            
            this.AvatarName = jsonObject["avatar_name"].ToObject<string>();
            this.m_vAvatarLevel = jsonObject["avatar_level"].ToObject<int>();
            
            this.m_vExperience = jsonObject["experience"].ToObject<int>();
            this.m_vCurrentGems = jsonObject["current_gems"].ToObject<int>();
            SetScore(jsonObject["score"].ToObject<int>());
            
            this.m_vNameChangingLeft = 0xFF;
            this.m_vnameChosenByUser = jsonObject["nameChosenByUser"].ToObject<byte>();
            
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // current timestamp
            
            long shieldStart = jsonObject["shield_timestamp"]?.ToObject<long?>() ?? now;
            int shieldTimeRemaining = Math.Max(0, this.m_vShieldTimeValue - (int)(now - shieldStart));
            if (shieldTimeRemaining == 0)
            {
                this.m_vShieldTime = 0;
                this.mv_ShieldTimeStamp = 0;
            }
            shieldStart = jsonObject["protection_timestamp"]?.ToObject<long?>() ?? now; 
            shieldTimeRemaining = Math.Max(0, this.m_vProtectionTimeValue - (int)(now - shieldStart));
            if (shieldTimeRemaining == 0)
            {
                this.m_vProtectionTime = 0;
                this.m_vProtectionTimeStamp = 0;
            }
            this.m_vShieldTime = jsonObject["shield_time"].ToObject<int>();
            this.m_vShieldTimeValue = jsonObject["shield_timevalue"]?.ToObject<int>() ?? m_vShieldTime;
            this.m_vProtectionTime = jsonObject["protection_time"].ToObject<int>();
            this.m_vProtectionTimeValue = jsonObject["protection_timevalue"]?.ToObject<int>() ?? m_vProtectionTime;
            
            this.FacebookId = jsonObject["fb_id"].ToObject<string>();
            this.FacebookToken = jsonObject["fb_token"].ToObject<string>();
            this.GoogleId = jsonObject["gg_id"].ToObject<string>();
            this.GoogleToken = jsonObject["gg_token"].ToObject<string>();
            if (this.AllianceId != 0)
            {
                this.m_vReceived = jsonObject["troops_received"].ToObject<int>();
                this.m_vDonated = jsonObject["troops_donated"].ToObject<int>();
            }
            else
            {
                this.m_vReceived = 0;
                this.m_vDonated = 0;
            }
            if (jsonObject["rq_message"] != null && jsonObject["rq_message"].Type != JTokenType.Null)
            {
                string rawMessage = jsonObject["rq_message"].ToString();
                this.TroopRequestMessage = Regex.Replace(rawMessage, @"[^a-zA-Z0-9 ]", "");
            }
            else
            {
                this.TroopRequestMessage = null;
            }
            

            JArray jmessages = (JArray)jsonObject["messages"] ?? new JArray();
            foreach (JObject jobject in jmessages)
            {
                var type = jobject["type"].ToObject<int>();
                if (type == 3)
                {
                    AllianceDeclineStreamEntry ai = new AllianceDeclineStreamEntry();
                    ai.AllianceBadgeData = jobject["AllianceBadgeData"].ToObject<int>();
                    ai.AllianceId = jobject["AllianceId"].ToObject<int>();
                    ai.AllianceName = jobject["AllianceName"].ToObject<string>();
                    ai.m_vSenderLeagueId = jobject["SenderLeague"]?.ToObject<int>() ?? 0;
                    ai.ID = jobject["ID"].ToObject<int>();
                    ai.IsNew = jobject["isNew"].ToObject<byte>();
                    ai.SenderId = jobject["SenderId"].ToObject<int>();
                    ai.m_vSenderId = jobject["SenderId"].ToObject<int>();
                    ai.m_vSenderLevel = jobject["SenderLevel"].ToObject<int>();
                    ai.m_vSenderName = jobject["SenderName"].ToObject<string>();
                    ai.m_vCreationTime = jobject["CreationTime"].ToObject<DateTime>();
                    messages.Add(ai);
                }
                else if (type == 4)
                {
                    AllianceInviteStreamEntry ai = new AllianceInviteStreamEntry();
                    ai.AllianceBadgeData = jobject["AllianceBadgeData"].ToObject<int>();
                    ai.AllianceId = jobject["AllianceId"].ToObject<int>();
                    ai.AllianceName = jobject["AllianceName"].ToObject<string>();
                    ai.m_vSenderLeagueId = jobject["SenderLeague"]?.ToObject<int>() ?? 0;
                    ai.ID = jobject["ID"].ToObject<int>();
                    ai.IsNew = jobject["isNew"].ToObject<byte>();
                    ai.SenderId = jobject["SenderId"].ToObject<int>();
                    ai.m_vSenderId = jobject["SenderId"].ToObject<int>();
                    ai.m_vSenderLevel = jobject["SenderLevel"].ToObject<int>();
                    ai.m_vSenderName = jobject["SenderName"].ToObject<string>();
                    ai.m_vCreationTime = jobject["CreationTime"].ToObject<DateTime>();
                    messages.Add(ai);
                }
                else if (type == 6)
                {
                    AllianceMailStreamEntry ai = new AllianceMailStreamEntry();
                    ai.AllianceBadgeData = jobject["AllianceBadgeData"].ToObject<int>();
                    ai.AllianceId = jobject["AllianceId"].ToObject<int>();
                    ai.AllianceName = jobject["AllianceName"].ToObject<string>();
                    ai.m_vSenderLeagueId = jobject["SenderLeague"]?.ToObject<int>() ?? 0;
                    ai.ID = jobject["ID"].ToObject<int>();
                    ai.IsNew = jobject["isNew"].ToObject<byte>();
                    ai.SenderId = jobject["SenderId"].ToObject<int>();
                    ai.Message = jobject["Message"].ToObject<string>();
                    ai.m_vSenderId = jobject["SenderId"].ToObject<int>();
                    ai.m_vSenderLevel = jobject["SenderLevel"].ToObject<int>();
                    ai.m_vSenderName = jobject["SenderName"].ToObject<string>();
                    ai.m_vCreationTime = jobject["CreationTime"].ToObject<DateTime>();
                    messages.Add(ai);
                }
            }
            
            JArray jsonBookmarkedClan = (JArray)jsonObject["bookmark"];
            foreach (JObject jobject in jsonBookmarkedClan)
            {
                JObject data = (JObject)jobject;
                BookmarkSlot ds = new BookmarkSlot(0);
                ds.Load(data);
                BookmarkedClan.Add(ds);
            }

            JArray jsonResources = (JArray) jsonObject["resources"];
            foreach (JObject resource in jsonResources)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(resource);
                GetResources().Add(ds);
            }

            JArray jsonUnits = (JArray) jsonObject["units"];
            foreach (JObject unit in jsonUnits)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(unit);
                if (ds.Value < 0)
                    ds.Value = 0;
                m_vUnitCount.Add(ds);
            }

            JArray jsonSpells = (JArray) jsonObject["spells"];
            foreach (JObject spell in jsonSpells)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(spell);
                if (ds.Value < 0)
                    ds.Value = 0;
                m_vSpellCount.Add(ds);
            }

            JArray jsonUnitLevels = (JArray) jsonObject["unit_upgrade_levels"];
            foreach (JObject unitLevel in jsonUnitLevels)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(unitLevel);
                m_vUnitUpgradeLevel.Add(ds);
            }

            JArray jsonSpellLevels = (JArray) jsonObject["spell_upgrade_levels"];
            foreach (JObject data in jsonSpellLevels)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                m_vSpellUpgradeLevel.Add(ds);
            }

            JArray jsonHeroLevels = (JArray) jsonObject["hero_upgrade_levels"];
            foreach (JObject data in jsonHeroLevels)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                m_vHeroUpgradeLevel.Add(ds);
            }

            JArray jsonHeroHealth = (JArray) jsonObject["hero_health"];
            foreach (JObject data in jsonHeroHealth)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                m_vHeroHealth.Add(ds);
            }

            JArray jsonHeroState = (JArray) jsonObject["hero_state"];
            foreach (JObject data in jsonHeroState)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                m_vHeroState.Add(ds);
            }

            JArray jsonAllianceUnits = (JArray) jsonObject["alliance_units"];
            foreach (JObject data in jsonAllianceUnits)
            {
                DonationSlot ds = new DonationSlot(0, 0, 0, 0);
                ds.Load(data);
                AllianceUnits.Add(ds);
            }
            TutorialStepsCount = jsonObject["tutorial_step"].ToObject<uint>();

            JArray jsonAchievementsProgress = (JArray) jsonObject["achievements_progress"];
            foreach (JObject data in jsonAchievementsProgress)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                Achievements.Add(ds);
            }

            JArray jsonNpcLevels = (JArray) jsonObject["npc_levels"] ?? new JArray();
            foreach (JObject data in jsonNpcLevels)
            {
                NpcLevels[(int)data["Index"]].Stars = (int)data["Stars"];
            }
            
            JArray jsonNpcStars = (JArray) jsonObject["npc_stars"];
            foreach (JObject data in jsonNpcStars)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                NpcStars.Add(ds);
            }

            JArray jsonNpcLootedGold = (JArray) jsonObject["npc_looted_gold"];
            foreach (JObject data in jsonNpcLootedGold)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                NpcLootedGold.Add(ds);
            }

            JArray jsonNpcLootedElixir = (JArray) jsonObject["npc_looted_elixir"];
            foreach (JObject data in jsonNpcLootedElixir)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                NpcLootedElixir.Add(ds);
            }
            JArray jsonQuickTrain1 = (JArray)jsonObject["quick_train_1"];
            foreach (JObject data in jsonQuickTrain1)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                QuickTrain1.Add(ds);
            }
            JArray jsonQuickTrain2 = (JArray)jsonObject["quick_train_2"];
            foreach (JObject data in jsonQuickTrain2)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                QuickTrain2.Add(ds);
            }
            JArray jsonQuickTrain3 = (JArray)jsonObject["quick_train_3"];
            foreach (JObject data in jsonQuickTrain3)
            {
                DataSlot ds = new DataSlot(null, 0);
                ds.Load(data);
                QuickTrain3.Add(ds);
            }
            m_vPremium = true;
        }

        public void testload(string jsonString)
        {
            var jsonObject = JObject.Parse(jsonString);
            JArray jmessages = (JArray)jsonObject["test"] ?? new JArray();
            foreach (JObject jobject in jmessages) { }
        }

        public string testSave()
        {
            JObject jsonData = new JObject { {"test", "test"} };
            return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        }

        public string SaveToJSON()
        {
            #region Foreach Stuff
            JArray jsonBookmarkClan = new JArray();
            foreach (BookmarkSlot clan in BookmarkedClan)
                jsonBookmarkClan.Add(clan.Save(new JObject()));

            JArray jsonResourcesArray = new JArray();
            foreach (DataSlot resource in GetResources())
                jsonResourcesArray.Add(resource.Save(new JObject()));

            JArray jsonUnitsArray = new JArray();
            foreach (DataSlot unit in GetUnits())
                jsonUnitsArray.Add(unit.Save(new JObject()));

            JArray jsonSpellsArray = new JArray();
            foreach (DataSlot spell in GetSpells())
                jsonSpellsArray.Add(spell.Save(new JObject()));

            JArray jsonUnitUpgradeLevelsArray = new JArray();
            foreach (DataSlot unitUpgradeLevel in m_vUnitUpgradeLevel)
                jsonUnitUpgradeLevelsArray.Add(unitUpgradeLevel.Save(new JObject()));


            JArray jsonSpellUpgradeLevelsArray = new JArray();
            foreach (DataSlot spellUpgradeLevel in m_vSpellUpgradeLevel)
                jsonSpellUpgradeLevelsArray.Add(spellUpgradeLevel.Save(new JObject()));

            JArray jsonHeroUpgradeLevelsArray = new JArray();
            foreach (DataSlot heroUpgradeLevel in m_vHeroUpgradeLevel)
                jsonHeroUpgradeLevelsArray.Add(heroUpgradeLevel.Save(new JObject()));

            JArray jsonHeroHealthArray = new JArray();
            foreach (DataSlot heroHealth in m_vHeroHealth)
                jsonHeroHealthArray.Add(heroHealth.Save(new JObject()));

            JArray jsonHeroStateArray = new JArray();
            foreach (DataSlot heroState in m_vHeroState)
                jsonHeroStateArray.Add(heroState.Save(new JObject()));

             JArray jsonAllianceUnitsArray = new JArray();
            foreach (DonationSlot allianceUnit in AllianceUnits)
                jsonAllianceUnitsArray.Add(allianceUnit.Save(new JObject()));

            JArray jsonAchievementsProgressArray = new JArray();
            foreach (DataSlot achievement in Achievements)
                jsonAchievementsProgressArray.Add(achievement.Save(new JObject()));
            
            JArray jsonNpcLevelProgression = new JArray();
            foreach (NpcLevel npclevel in NpcLevels)
                jsonNpcLevelProgression.Add(npclevel.Save(new JObject()));

            JArray jsonNpcStarsArray = new JArray();
            foreach (DataSlot npcLevel in NpcStars)
                jsonNpcStarsArray.Add(npcLevel.Save(new JObject()));

            JArray jsonNpcLootedGoldArray = new JArray();
            foreach (DataSlot npcLevel in NpcLootedGold)
                jsonNpcLootedGoldArray.Add(npcLevel.Save(new JObject()));
  
            JArray jsonNpcLootedElixirArray = new JArray();
            foreach (DataSlot npcLevel in NpcLootedElixir)
                jsonNpcLootedElixirArray.Add(npcLevel.Save(new JObject()));

            JArray jsonQuickTrain1Array = new JArray();
            foreach (DataSlot quicktrain1 in QuickTrain1)
                jsonQuickTrain1Array.Add(quicktrain1.Save(new JObject()));

            JArray jsonQuickTrain2Array = new JArray();
            foreach (DataSlot quicktrain2 in QuickTrain2)
                jsonQuickTrain2Array.Add(quicktrain2.Save(new JObject()));

            JArray jsonQuickTrain3Array = new JArray();
            foreach (DataSlot quicktrain3 in QuickTrain3)
                jsonQuickTrain3Array.Add(quicktrain3.Save(new JObject()));
            
            JArray jmessages = new JArray();
            foreach (AvatarStreamEntry.AvatarStreamEntry message in messages)
            {
                message.IsNew = 0;
                var type = message.GetStreamEntryType();
                if (type == 3)
                {
                    AllianceDeclineStreamEntry ai = (AllianceDeclineStreamEntry) message;
                    JObject jO = new JObject();
                    jO.Add("AllianceBadgeData", ai.AllianceBadgeData);
                    jO.Add("AllianceId", ai.AllianceId);
                    jO.Add("AllianceName", ai.AllianceName);
                    jO.Add("SenderLeague", ai.m_vSenderLeagueId);
                    jO.Add("ID", ai.ID);
                    jO.Add("isNew", ai.IsNew);
                    jO.Add("SenderId", ai.m_vSenderId);
                    jO.Add("CreationTime", ai.m_vCreationTime);
                    jO.Add("SenderLevel", ai.m_vSenderLevel);
                    jO.Add("SenderName", ai.m_vSenderName);
                    jO.Add("type", type);
                    jmessages.Add(jO);
                }
                else if (type == 4)
                {
                    AllianceInviteStreamEntry ai = (AllianceInviteStreamEntry) message;
                    JObject jO = new JObject();
                    jO.Add("AllianceBadgeData", ai.AllianceBadgeData);
                    jO.Add("AllianceId", ai.AllianceId);
                    jO.Add("AllianceName", ai.AllianceName);
                    jO.Add("SenderLeague", ai.m_vSenderLeagueId);
                    jO.Add("ID", ai.ID);
                    jO.Add("isNew", ai.IsNew);
                    jO.Add("SenderId", ai.m_vSenderId);
                    jO.Add("CreationTime", ai.m_vCreationTime);
                    jO.Add("SenderLevel", ai.m_vSenderLevel);
                    jO.Add("SenderName", ai.m_vSenderName);
                    jO.Add("type", type);
                    jmessages.Add(jO);
                }
                else if (type == 6)
                {
                    AllianceMailStreamEntry am = (AllianceMailStreamEntry) message;
                    JObject jO = new JObject();
                    jO.Add("AllianceBadgeData", am.AllianceBadgeData);
                    jO.Add("AllianceId", am.AllianceId);
                    jO.Add("AllianceName", am.AllianceName);
                    jO.Add("SenderLeague", am.m_vSenderLeagueId);
                    jO.Add("ID", am.ID);
                    jO.Add("isNew", am.IsNew);
                    jO.Add("Message", am.Message);
                    jO.Add("SenderId", am.m_vSenderId);
                    jO.Add("CreationTime", am.m_vCreationTime);
                    jO.Add("SenderLevel", am.m_vSenderLevel);
                    jO.Add("SenderName", am.m_vSenderName);
                    jO.Add("type", type);
                    jmessages.Add(jO);
                }
            }
            #endregion
            
            if (this.AllianceId == 0)
            {
                this.m_vReceived = 0;
                this.m_vDonated = 0;
            }

            JObject jsonData = new JObject
            {
                {"acc_switch", this.account_switch},
                {"acc_password", this.account_password},
                {"attacks_won", this.attacks_won},
                {"avatar_id", this.UserId},
                {"id_high_int", this.HighID},
                {"id_low_int", this.LowID},
                {"token", this.UserToken},
                {"region", this.Region},
                {"IPAddress", this.IPAddress},
                {"avatar_creation_date", this.m_vAccountCreationDate},
                {"avatar_privilages", this.AccountPrivileges},
                {"avatar_banned", false},
                {"active_layout", this.m_vActiveLayout},
                {"last_tick_save", this.LastTickSaved},
                {"android", this.m_vAndroid},
                {"current_home_id", this.CurrentHomeId},
                {"alliance_id", this.AllianceId},
                {"alliance_castle_level", GetAllianceCastleLevel()},
                {"alliance_castle_total_capacity", GetAllianceCastleTotalCapacity()},
                {"alliance_castle_used_capacity", GetAllianceCastleUsedCapacity()},
                {"townhall_level", this.m_vTownHallLevel},
                {"avatar_name", this.AvatarName},
                {"avatar_level", this.m_vAvatarLevel},
                {"experience", this.m_vExperience},
                {"current_gems", this.m_vCurrentGems},
                {"score", GetScore()},
                {"nameChangesLeft", this.m_vNameChangingLeft},
                {"nameChosenByUser", (ushort) m_vnameChosenByUser},
                {"shield_time", this.m_vShieldTime},
                {"shield_timevalue", this.m_vShieldTimeValue},
                {"shield_timestamp", this.mv_ShieldTimeStamp},
                {"protection_time", this.m_vProtectionTime},
                {"protection_timevalue", this.m_vProtectionTimeValue},
                {"protection_timestamp", this.m_vProtectionTimeStamp},
                {"fb_id", this.FacebookId},
                {"fb_token", this.FacebookToken},
                {"gg_id", this.GoogleId},
                {"troops_received", this.m_vReceived},
                {"troops_donated", this.m_vDonated},
                {"gg_token", this.GoogleToken},
                {"rq_message", this.TroopRequestMessage},
                {"bookmark", jsonBookmarkClan},
                {"resources", jsonResourcesArray},
                {"units", jsonUnitsArray},
                {"spells", jsonSpellsArray},
                {"unit_upgrade_levels", jsonUnitUpgradeLevelsArray},
                {"spell_upgrade_levels", jsonSpellUpgradeLevelsArray},
                {"hero_upgrade_levels", jsonHeroUpgradeLevelsArray},
                {"hero_health", jsonHeroHealthArray},
                {"hero_state", jsonHeroStateArray},
                {"alliance_units", jsonAllianceUnitsArray},
                {"tutorial_step", this.TutorialStepsCount},
                {"achievements_progress", jsonAchievementsProgressArray},
                {"npc_levels", jsonNpcLevelProgression},
                {"npc_stars", jsonNpcStarsArray},
                {"npc_looted_gold", jsonNpcLootedGoldArray},
                {"npc_looted_elixir", jsonNpcLootedElixirArray},
                {"quick_train_1", jsonQuickTrain1Array},
                {"quick_train_2", jsonQuickTrain2Array},
                {"quick_train_3", jsonQuickTrain3Array},
                {"messages", jmessages},
                {"battles", new JArray()},
                {"Premium", true}
            };

            return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        }

        public void loadBattlesFromJson(string jsonString)
        {
            var jsonObject = JObject.Parse(jsonString);
            JArray jbattles = (JArray)jsonObject["battles"] ?? new JArray();
            foreach (JObject jobject in jbattles)
            {
                battles.Add(jobject);
            }
        }

        public string saveBattlesToJson()
        {
            JArray json_battles = new JArray();
            foreach (JObject battle_json in battles)
                json_battles.Add(battle_json);
            Object jsonData = new JObject
            {
                { "battles", json_battles }
            };
            return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        }

        public void InitializeAccountCreationDate() => m_vAccountCreationDate = DateTime.Now;

        public void AddAllianceTroop(long did, int id, int value, int level)
        {
            DonationSlot e = AllianceUnits.Find(t => t.ID == id && t.DonatorID == did && t.UnitLevel == level);
            if (e != null)
            {
                int i = AllianceUnits.IndexOf(e);
                e.Count = e.Count + value;
                AllianceUnits[i] = e;
            }
            else
            {
                DonationSlot ds = new DonationSlot(did, id, value, level);
                AllianceUnits.Add(ds);
            }
        }
        
        public void RemoveAllianceTroop(DonationSlot e, int value)
        {
            if (e != null)
            {
                int i = AllianceUnits.IndexOf(e);
                e.Count -= value;
                
                if (e.Count <= 0)
                    AllianceUnits.RemoveAt(i);
                else
                    AllianceUnits[i] = e;
            }
        }

        public void SetAchievment(AchievementData ad, bool finished)
        {
            int index = GetDataIndex(Achievements, ad);
            int value = finished ? 1 : 0;
            if (index != -1)
                Achievements[index].Value = value;
            else
            {
                DataSlot ds = new DataSlot(ad, value);
                Achievements.Add(ds);
            }
        }

        public async void SetAllianceRole(int a)
        {
            try
            {
                AllianceMemberEntry ame = await GetAllianceMemberEntry();
                if (ame != null)
                    ame.Role = a;
            }
            catch (Exception){}
        }

        public void SetName(string name)
        {
            this.AvatarName = name;
            TutorialStepsCount = 0x0D;
        }

        public void SetScore(int newScore)
        {
            if (newScore < 0)
            {
                newScore = 0;
            }
            m_vScore = newScore;
            updateLeague();
        }

        public void UseDiamonds(int diamondCount) => m_vCurrentGems -= diamondCount;
    }
}

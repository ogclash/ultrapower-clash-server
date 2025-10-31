using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCS.Core.Network;
using UCS.Core.Settings;
using UCS.Files;
using UCS.Helpers.List;
using UCS.Logic;
using Timer = System.Threading.Timer;
using static UCS.Core.Logger;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;

namespace UCS.Core
{
    
    public static class ThreadSafeRandom
    {
        [ThreadStatic] 
        private static Random local;

        // Thread-local random instance
        public static Random Range
        {
            get
            {
                if (local == null)
                    local = new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId);
                return local;
            }
        }
    }
    internal class ObjectManager : IDisposable
    {
        private static string filePath = "current_season";
        public static string filePathPrevius = "previuos_season";
        public static string filePathPreviusNumber = "previuos_season_count";
        private static int currentMonth = 0;
        private static long m_vAllianceSeed;
        private static long m_vAvatarSeed;
        public static int m_vDonationSeed;
        private static int m_vRandomBaseAmount;
        private static DatabaseManager m_vDatabase;
        private static string[] m_vHomeDefault;
        public static Timer TimerReferenceRedis;
        public static Timer TimerReferenceMysql;
        public static Timer TimerReferencePlayersMysql;
        public static Timer TimerReferenceClansMysql;
        public static Timer TimerReferenceOfflineTick;
        public static Dictionary<int, string> NpcLevels;
        public static FingerPrint FingerPrint;
        static int MaxPlayerID;
        static int MaxAllianceID;

        public ObjectManager()
        {
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, DateTime.Now.Month.ToString());
            currentMonth = Convert.ToInt32(File.ReadAllText(filePath).Trim());

            m_vDatabase            = new DatabaseManager();

            NpcLevels              = new Dictionary<int, string>();
            FingerPrint            = new FingerPrint();

            MaxPlayerID            = Convert.ToInt32(m_vDatabase.GetMaxPlayerId() + 1);
            MaxAllianceID          = Convert.ToInt32(m_vDatabase.GetMaxAllianceId() + 1);

            m_vAvatarSeed          = MaxPlayerID;
            m_vAllianceSeed        = MaxAllianceID;
            m_vHomeDefault = new string[5];
            using (StreamReader sr = new StreamReader(@"Gamefiles/starting_home_with_obstacles.json"))
                m_vHomeDefault[0] = sr.ReadToEnd();
            using (StreamReader sr = new StreamReader(@"Gamefiles/starting_home_1.json"))
                m_vHomeDefault[1] = sr.ReadToEnd();
            using (StreamReader sr = new StreamReader(@"Gamefiles/starting_home_2.json"))
                m_vHomeDefault[2] = sr.ReadToEnd();
            using (StreamReader sr = new StreamReader(@"Gamefiles/starting_home_3.json"))
                m_vHomeDefault[3] = sr.ReadToEnd();
            using (StreamReader sr = new StreamReader(@"Gamefiles/starting_home_4.json"))
                m_vHomeDefault[4] = sr.ReadToEnd();

            LoadNpcLevels();

            if (!Constants.DebugMode)
            {
                if (Constants.UseCacheServer)
                    TimerReferenceRedis = new Timer(SaveRedis, null, 10000, 45000);
                TimerReferencePlayersMysql = new Timer(SavePlayersMysql, null, 45000, 60000);
                TimerReferenceClansMysql = new Timer(SaveClansMysql, null, 45000, 30000);
                TimerReferenceOfflineTick = new Timer(StartOfflineTick, null, 10000, 2500);
            }
            Say($"UCS Database has been succesfully loaded. ({Convert.ToInt32(MaxAllianceID + MaxPlayerID)} Tables)");
        }

        public static DatabaseManager getDatabaseManager()
        {
            return m_vDatabase;
        }
        
        private async void OfflineTick(List<Level> avatars)
        {
            if (currentMonth != DateTime.Now.Month)
            {
                File.WriteAllText(filePath, DateTime.Now.Month.ToString());
                currentMonth = DateTime.Now.Month;
                List<byte> packet1 = new List<byte>();
                int i = 0;
                foreach (Level player in avatars.OrderByDescending(t => t.Avatar.GetScore()).Take(10))
                {
                    if (player.Avatar.m_vAvatarLevel >= 1 && player.Avatar.AvatarName != "NoNameYet")
                    {
                        ClientAvatar pl = player.Avatar;
                        packet1.AddLong(pl.UserId);
                        packet1.AddString(pl.AvatarName);
                        packet1.AddInt(i + 1);
                        packet1.AddInt(pl.GetScore());
                        packet1.AddInt(i + 1);
                        packet1.AddInt(pl.m_vAvatarLevel);
                        //Attacks
                        packet1.AddInt(0);
                        packet1.AddInt(i);
                        //Defense
                        packet1.AddInt(0);
                        packet1.AddInt(1);
                        packet1.AddInt(pl.m_vLeagueId);
                        packet1.AddString(pl.Region.ToUpper());
                        packet1.AddLong(pl.UserId);
                        packet1.AddInt(1);
                        packet1.AddInt(1);
                        if (pl.AllianceId > 0)
                        {
                            Alliance _Alliance = ObjectManager.GetAlliance(pl.AllianceId);
                            if (_Alliance.m_vAllianceMembers.ContainsKey(pl.UserId))
                            {
                                packet1.Add(1);
                                packet1.AddLong(pl.AllianceId);
                                packet1.AddString(_Alliance.m_vAllianceName);
                                packet1.AddInt(_Alliance.m_vAllianceBadgeData);
                            }
                            else
                            {
                                packet1.Add(0);
                                pl.AllianceId = 0;
                            }
                        }
                        else
                            packet1.Add(0);
                        i++;
                    }
                }
                File.WriteAllText(filePathPreviusNumber, i.ToString());
                File.WriteAllBytes(filePathPrevius, packet1.ToArray());

                int c = 0;
                foreach (Alliance clan in ResourcesManager.GetInMemoryAlliances().OrderByDescending(t => t.m_vScore).Take(3))
                {
                    List<Level> Levels = new List<Level>();
                    foreach (int memberid in clan.m_vAllianceMembers.Keys)
                        Levels.Add(await ResourcesManager.GetPlayer(memberid));
                    int reward = 0;
                    switch (c)
                    {
                        case 0:
                            reward = 50000;
                            break;
                        case 1:
                            reward = 30000;
                            break;
                        case 2:
                            reward = 15000;
                            break;
                    }
                    if (clan.m_vAllianceMembers.Count < 20)
                        reward /= clan.m_vAllianceMembers.Count;
                    else
                        reward /= 20;
                    foreach (Level player in Levels.ToList().OrderByDescending(t => t.Avatar.GetScore()).Take(20))
                    {
                        
                        player.Avatar.AddDiamonds(reward);
                        if (player.Client != null)
                            new OwnHomeDataMessage(player.Client, player).Send();
                    }
                    c++;
                }
            }
            foreach (Level pl in avatars)
            {
                try
                {
                    await Task.Run(() => pl.Tick(true));
                }
                catch (Exception ex)
                {
                    Write("RunTime-Error: " + ex);
                }
            }
        }

        private void StartOfflineTick(object state)
        {
            OfflineTick(ResourcesManager.m_vInMemoryLevels.Values.ToList());
        }

        private static void SaveRedis(object state)
        {
            m_vDatabase.Save(ResourcesManager.m_vInMemoryLevels.Values.ToList(), Save.Redis);
            m_vDatabase.Save(ResourcesManager.GetInMemoryAlliances(), Save.Redis).Wait();
        }
        private static void SavePlayersMysql(object state)
        {
            m_vDatabase.Save(ResourcesManager.m_vInMemoryLevels.Values.ToList()).Wait();
        }
        private static void SaveClansMysql(object state)
        {
            m_vDatabase.Save(ResourcesManager.GetInMemoryAlliances()).Wait();
        }

        public static Alliance CreateAlliance(long seed)
        {
            Alliance alliance;
            if (seed == 0)
                seed = m_vAllianceSeed;
            alliance = new Alliance(seed);
            m_vAllianceSeed++;
            m_vDatabase.CreateAlliance(alliance);
            ResourcesManager.AddAllianceInMemory(alliance);
            return alliance;
        }

        public static Level CreateAvatar(long seed, string token)
        {
            Level pl;
            if (seed == 0)
                seed = m_vAvatarSeed;
            pl = new Level(seed, token);
            m_vAvatarSeed++;
            pl.LoadFromJSON(m_vHomeDefault[ThreadSafeRandom.Range.Next(0, m_vHomeDefault.Length)]);
            m_vDatabase.CreateAccount(pl);
            return pl;
        }

        public static Alliance GetAlliance(long allianceId)
        {
            Alliance alliance;
            if (ResourcesManager.InMemoryAlliancesContain(allianceId))
                return ResourcesManager.GetInMemoryAlliance(allianceId);
            
            alliance = m_vDatabase.GetAlliance(allianceId);
            if (alliance != null)
                ResourcesManager.AddAllianceInMemory(alliance);
            else
                return null;
            return alliance;
        }

        public static List<Alliance> GetInMemoryAlliances() => ResourcesManager.GetInMemoryAlliances();
        
        public static Level GetRandomOfflinePlayer()
        {
            int index = ThreadSafeRandom.Range.Next(0, ResourcesManager.m_vInMemoryLevels.Count);
            Level defender = ResourcesManager.m_vInMemoryLevels.Values.ToList().ElementAt(index);
            while (ResourcesManager.IsPlayerOnline(defender))
            {
                index = ThreadSafeRandom.Range.Next(0, ResourcesManager.m_vInMemoryLevels.Count);
                defender = ResourcesManager.m_vInMemoryLevels.Values.ToList().ElementAt(index);
            }
            return defender;
        }

        public static void LoadNpcLevels()
        {
            int Count = 0;
            NpcLevels.Add(17000000, new StreamReader(@"Gamefiles/level/NPC/tutorial_npc.json").ReadToEnd());
            NpcLevels.Add(17000001, new StreamReader(@"Gamefiles/level/NPC/tutorial_npc2.json").ReadToEnd());
            for (int i = 2; i < 50; i++)
            {
                using (StreamReader sr = new StreamReader(@"Gamefiles/level/NPC/npc" + (Count + 1) + ".json"))
                    NpcLevels.Add(i + 17000000, sr.ReadToEnd());
                Count++;
            }

            Say($"NPC Levels  have been succesfully loaded. ({Count})");
        }

        public static void RemoveInMemoryAlliance(long id)
        {
            ResourcesManager.RemoveAllianceFromMemory(id);
        }

        public static int GetMaxAllianceID() => MaxAllianceID;

        public static int GetMaxPlayerID() => MaxPlayerID;

        public void Dispose()
        {
            if (TimerReferenceRedis != null && TimerReferenceMysql != null)
            {
                TimerReferenceRedis.Dispose();
                TimerReferenceMysql.Dispose();
                TimerReferenceRedis = null;
                TimerReferenceMysql = null;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using MySql.Data.MySqlClient;
using System.Linq;
using UCS.Database;
using UCS.Logic;
using UCS.Core.Settings;
using static UCS.Core.Logger;
using System.Threading.Tasks;
using UCS.Logic.Enums;
using UCS.Helpers;

namespace UCS.Core
{
    internal class DatabaseManager 
    {
        internal string Mysql;

        public void CreateAccount(Level l)
        {
            try
            {
                if (Constants.UseCacheServer) //Redis As Cache Server
                    Redis.Players.StringSet(l.Avatar.UserId.ToString(), l.Avatar.SaveToJSON() + "#:#:#:#" + l.SaveToJSON(),
                        TimeSpan.FromHours(4));

                using (Mysql db = new Mysql())
                {
                    db.Player.Add(
                        new Player
                        {
                            Battles = l.Avatar.saveBattlesToJson(),
                            PlayerId = l.Avatar.UserId,
                            Avatar = l.Avatar.SaveToJSON(),
                            GameObjects = l.SaveToJSON()
                        }
                    );
                    db.SaveChanges();
                }
            } catch (Exception) { }
        }

        public void CreateAlliance(Alliance a)
        {
            try
            {
                if (Constants.UseCacheServer) //Redis As Cache Server
                    Redis.Clans.StringSet(a.m_vAllianceId.ToString(), a.SaveToJSON(), TimeSpan.FromHours(4));

                using (Mysql db = new Mysql())
                {
                    db.Clan.Add(
                        new Clan()
                        {
                            ClanId = a.m_vAllianceId,
                            LastUpdateTime = DateTime.Now,
                            Data = a.SaveToJSON()
                        }
                    );
                    db.SaveChanges();
                }
            } catch (Exception) { }
        }

        public async Task<Level> GetAccount(long playerId)
        {
            try
            {
                Level account = null;
                if (Constants.UseCacheServer) //Redis as cache server
                {
                    string _Data = Redis.Players.StringGet(playerId.ToString()).ToString();

                    if (!string.IsNullOrEmpty(_Data) && _Data.Contains("#:#:#:#"))
                    {
                        string[] _Datas = _Data.Split(new string[1] { "#:#:#:#" }, StringSplitOptions.None);

                        if (!string.IsNullOrEmpty(_Datas[0]) && !string.IsNullOrEmpty(_Datas[1]))
                        {
                            account = new Level();
                            account.Avatar.LoadFromJSON(_Datas[0]);
                            account.LoadFromJSON(_Datas[1]);
                        }
                    }
                    else
                    {
                        using (Mysql db = new Mysql())
                        {
                            Player p = await db.Player.FindAsync(playerId);

                            if (p != null)
                            {
                                account = new Level();
                                account.Avatar.LoadFromJSON(p.Avatar);
                                account.LoadFromJSON(p.GameObjects);
                                Redis.Players.StringSet(playerId.ToString(), p.Avatar + "#:#:#:#" + p.GameObjects,
                                    TimeSpan.FromHours(4));
                            }
                        }

                        ;
                    }
                }
                else
                {
                    using (Mysql db = new Mysql())
                    {
                        Player p = await db.Player.FindAsync(playerId);

                        if (p != null)
                        {
                            account = new Level();
                            if (p.Battles != "")
                                account.Avatar.loadBattlesFromJson(p.Battles);
                            account.Avatar.LoadFromJSON(p.Avatar);
                            account.LoadFromJSON(p.GameObjects);
                        }
                    }
                }

                return account;
            }
            catch (Exception ex)
            {
                if (Constants.DebugMode)
                    Logger.Write("Failed loading account because of following exception: " + ex);
                return null;
            }
        }
        public async Task<List<Level>> GetAllAccountsFromDb()
        {
            List<Level> accounts = new List<Level>();
            using (Mysql db = new Mysql())
            {
                var players = await db.Player.ToListAsync(); // Make sure ToListAsync is available

                foreach (var p in players)
                {
                    try
                    {
                        Level account = new Level();
                        account.Avatar.LoadFromJSON(p.Avatar);
                        if (account.Avatar.HighID > account.Avatar.LowID)
                            account.Avatar.LowID = account.Avatar.HighID;
                        account.Avatar.HighID = 0;
                        if (account.Avatar.AvatarName != "NoNameYet" && account.Avatar.TutorialStepsCount > 10)
                        {
                            try
                            {
                                if (p.Battles != "")
                                    account.Avatar.loadBattlesFromJson(p.Battles);
                                account.LoadFromJSON(p.GameObjects);
                                accounts.Add(account);
                            }
                            catch (Exception ex)
                            {
                                Logger.Say("User with id: " + account.Avatar.UserId + " failed to load");
                                if (Constants.DebugMode)
                                    Logger.Say(" because of following reason: " +ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Constants.DebugMode)
                            Logger.Write("Failed loading account because of following exception: " + ex);
                    }
                }
            }
            return accounts;
        }


        public Alliance GetAlliance(long allianceId)
        {
            try 
            {
                Alliance alliance = null;
                if (Constants.UseCacheServer) 
                {
                    string _Data = Redis.Clans.StringGet(allianceId.ToString()).ToString();


                    if (!string.IsNullOrEmpty(_Data))
                    {
                        alliance = new Alliance();
                        alliance.LoadFromJSON(_Data);
                    }
                    else
                    {
                        using (Mysql db = new Mysql())
                        {
                            Clan p = db.Clan.Find(allianceId);
                            if (p != null)
                            {
                                alliance = new Alliance();
                                alliance.LoadFromJSON(p.Data);
                                Redis.Clans.StringSet(allianceId.ToString(), p.Data, TimeSpan.FromHours(4));
                            }
                        }
                    }
                }
                else
                {
                    using (Mysql db = new Mysql())
                    {
                        Clan p = db.Clan.Find(allianceId);
                        if (p != null)
                        {
                            alliance = new Alliance();
                            alliance.LoadFromJSON(p.Data);
                        }
                    }
                }
                return alliance;
            } catch (Exception) { return null; }
        }
        
        public List<Alliance> GetAllAlliancesFromDb()
        {
            try
            {
                List<Alliance> alliances = new List<Alliance>();

                using (Mysql db = new Mysql())
                {
                    var clans = db.Clan.ToList();

                    foreach (var c in clans)
                    {
                        Alliance alliance = new Alliance();
                        alliance.LoadFromJSON(c.Data);
                        alliances.Add(alliance);
                    }
                }

                return alliances;
            }
            catch (Exception)
            {
                return new List<Alliance>();
            }
        }
        public async Task<List<Alliance>> GetAllAlliancesFromDbAsync()
        {
            try
            {
                List<Alliance> alliances = new List<Alliance>();

                using (Mysql db = new Mysql())
                {
                    var clans = await db.Clan.ToListAsync();

                    foreach (var c in clans)
                    {
                        Alliance alliance = new Alliance();
                        alliance.LoadFromJSON(c.Data);
                        alliances.Add(alliance);
                    }
                }

                return alliances;
            }
            catch (Exception)
            {
                return new List<Alliance>();
            }
        }



        public List<long> GetAllPlayerIds()
        {
            List<long> ids = new List<long>();
            using (Mysql db = new Mysql())
                ids.AddRange(db.Player.Select(p => p.PlayerId));
            return ids;
        }

        public List<long> GetAllClanIds()
        {
            List<long> ids = new List<long>();
            using (Mysql db = new Mysql())
                ids.AddRange(db.Clan.Select(p => p.ClanId));
            return ids;
        }

        public long GetMaxAllianceId()
        {
            const string SQL = "SELECT coalesce(MAX(ClanId), 0) FROM Clan";
            int Seed = -1;

            using (MySqlConnection Conn = new MySqlConnection(this.Mysql))
            {
                Conn.Open();

                using (MySqlCommand CMD = new MySqlCommand(SQL, Conn))
                {
                    CMD.Prepare();
                    Seed = Convert.ToInt32(CMD.ExecuteScalar());
                }
            }

            return Seed;
        }

        public long GetMaxPlayerId()
        {
            try
            {
                const string SQL = "SELECT coalesce(MAX(PlayerId), 0) FROM Player";
                int Seed = -1;

                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder()
                {
                    Server = Utils.ParseConfigString("MysqlIPAddress"),
                    UserID = Utils.ParseConfigString("MysqlUsername"),
                    Port = (uint)Utils.ParseConfigInt("MysqlPort"),
                    Pooling = false,
                    Database = Utils.ParseConfigString("MysqlDatabase"),
                    MinimumPoolSize = 1
                };

                if (!string.IsNullOrWhiteSpace(Utils.ParseConfigString("MysqlPassword")))
                {
                    builder.Password = Utils.ParseConfigString("MysqlPassword");
                }

                Mysql = builder.ToString();

                using (MySqlConnection Conn = new MySqlConnection(Mysql))
                {
                    Conn.Open();

                    using (MySqlCommand CMD = new MySqlCommand(SQL, Conn))
                    {
                        CMD.Prepare();
                        Seed = Convert.ToInt32(CMD.ExecuteScalar());
                    }
                }

                return Seed;
            }
            catch (Exception ex)
            {
                Say();
                Error("An exception occured when reconnecting to the MySQL Server.");
                Error("Please check your database configuration!");
                Error(ex.Message);
                Console.ReadKey();
                UCSControl.UCSRestart();
            }
            return 0;
        }

        public void RemoveAlliance(Alliance alliance)
        {
            try
            {
                long id = alliance.m_vAllianceId;
                using (Mysql db = new Mysql())
                {
                    db.Clan.Remove(db.Clan.Find((int)id));
                    db.SaveChanges();
                }
                ObjectManager.RemoveInMemoryAlliance(id);
            } catch (Exception) { }
        }

        public Level GetPlayerViaFacebook(string FacebookID)
        {
            try {
                Level account = null;
                Player Data = null;
                using (Mysql Database = new Mysql())
                {
                    Parallel.ForEach(Database.Player.ToList(), (Query, state) =>
                    {
                        if (Query.Avatar.Contains(FacebookID))
                        {
                            Data = Query;
                            state.Break();
                        }
                    });

                    if (Data != null)
                    {
                        account = new Level();
                        account.Avatar.LoadFromJSON(Data.Avatar);
                        account.LoadFromJSON(Data.GameObjects);
                        if (Constants.UseCacheServer)
                            Redis.Players.StringSet(Data.PlayerId.ToString(), Data.Avatar + "#:#:#:#" + Data.GameObjects,
                                TimeSpan.FromHours(4));
                    }

                }
                return account;
            } catch (Exception) { return null; }
        }

        public async Task Save(Alliance alliance)
        {
            try {
                if (Constants.UseCacheServer)
                    Redis.Clans.StringSet(alliance.m_vAllianceId.ToString(), alliance.SaveToJSON(), TimeSpan.FromHours(4));

                using (Mysql context = new Mysql())
                {
                    Clan c = await context.Clan.FindAsync((int)alliance.m_vAllianceId);
                    if (c != null)
                    {
                        c.LastUpdateTime = DateTime.Now;
                        c.Data = alliance.SaveToJSON();
                        context.Entry(c).State = EntityState.Modified;
                    }
                    await context.SaveChangesAsync();
                }
            } catch (Exception) { }
        }

        public async Task Save(Level avatar)
        {
            try
            {
                if (Constants.UseCacheServer)
                    Redis.Players.StringSet(avatar.Avatar.UserId.ToString(),
                        avatar.Avatar.SaveToJSON() + "#:#:#:#" + avatar.SaveToJSON(), TimeSpan.FromHours(4));

                using (Mysql context = new Mysql())
                {
                    Player p = await context.Player.FindAsync(avatar.Avatar.UserId);
                    if (p != null)
                    {
                        //p.LastUpdateTime = DateTime.Now;
                        p.Battles = avatar.Avatar.saveBattlesToJson();
                        p.Avatar = avatar.Avatar.SaveToJSON();
                        p.GameObjects = avatar.SaveToJSON();
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Cant save player with id {avatar.Avatar.UserId}: "+ex);
            }
        }

        public async Task Save(List<Level> avatars, Save Save = Logic.Enums.Save.Mysql)
        {
            try
            {
                switch (Save)
                {
                    case Save.Redis:
                    {
                        foreach (Level pl in avatars)
                        {
                            try
                            {
                                Redis.Players.StringSet(pl.Avatar.UserId.ToString(),
                                    pl.Avatar.SaveToJSON() + "#:#:#:#" + pl.SaveToJSON(), TimeSpan.FromHours(4));
                            }
                            catch (Exception) { }
                        }
                        break;
                    }

                    case Save.Mysql:
                    {
                        using (Mysql context = new Mysql())
                        {
                            foreach (Level pl in avatars)
                            {
                                try
                                {
                                    Player p = context.Player.Find(pl.Avatar.UserId);
                                    if (p != null)
                                    {
                                        //p.LastUpdateTime = DateTime.Now;
                                        p.Battles = pl.Avatar.saveBattlesToJson();
                                        p.Avatar = pl.Avatar.SaveToJSON();
                                        p.GameObjects = pl.SaveToJSON();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Write($"Cant save player with id {pl.Avatar.UserId}: "+ex);
                                }
                            }
                            await context.SaveChangesAsync();
                            //context.SaveChanges();
                        }
                        break;
                    }
                    case Save.Both:
                    {
                        await this.Save(avatars, Save.Redis);
                        await this.Save(avatars, Save.Mysql);
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task Save(List<Alliance> alliances, Save Save = Logic.Enums.Save.Mysql)
        {
            switch (Save)
            {

                case Save.Redis:
                {
                    foreach (Alliance alliance in alliances)
                    {
                        try
                        {
                            Redis.Clans.StringSet(alliance.m_vAllianceId.ToString(), alliance.SaveToJSON(),
                                TimeSpan.FromHours(4));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
                }
                case Save.Mysql:
                {
                    using (Mysql context = new Mysql())
                    {
                        foreach (Alliance alliance in alliances)
                        {
                            try
                            {
                                Clan c = context.Clan.Find((int)alliance.m_vAllianceId);
                                if (c != null)
                                {
                                    c.LastUpdateTime = DateTime.Now;
                                    c.Data = alliance.SaveToJSON();
                                }
                            }
                            catch (Exception) { }

                        }
                        await context.SaveChangesAsync();
                        //context.SaveChanges();
                    }
                    break;
                }
                case Save.Both:
                {
                    await this.Save(alliances, Save.Redis);
                    await this.Save(alliances, Save.Mysql);
                    break;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;

namespace UCS.Logic.Manager
{
    internal class GameObjectManager
    {
        public GameObjectManager(Level l)
        {
            m_vLevel                = l;
            m_vGameObjects          = new List<List<GameObject>>();
            m_vGameObjectRemoveList = new List<GameObject>();
            m_vGameObjectsIndex     = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                m_vGameObjects.Add(new List<GameObject>());
                m_vGameObjectsIndex.Add(0);
            }
            m_vComponentManager     = new ComponentManager(m_vLevel);
			m_vObstacleManager      = new ObstacleManager(m_vLevel);
		}

        ComponentManager m_vComponentManager;
        List<GameObject> m_vGameObjectRemoveList;
        List<List<GameObject>> m_vGameObjects;
        internal IList<GameObject>removedObstacles;
        List<int> m_vGameObjectsIndex;
        readonly Level m_vLevel;
        ObstacleManager m_vObstacleManager;
        
        public void RemoveObstalce(GameObject go)
        {
            if (removedObstacles == null)
                removedObstacles = new List<GameObject> { };

            if (!removedObstacles.Contains(go))
                removedObstacles.Add(go);
            
            if (go.GetData().GetGlobalID() == 8000030)
                this.m_vLevel.Avatar.AddDiamonds(25);
        }

		public void AddGameObject(GameObject go)
        {
            go.GlobalId = GenerateGameObjectGlobalId(go);
            if (go.ClassId == 0)
            {
                var b = (Building) go;
                var bd = b.GetBuildingData();
                if (bd.IsWorkerBuilding())
                    m_vLevel.WorkerManager.IncreaseWorkerCount();
            }
            m_vGameObjects[go.ClassId].Add(go);
        }

        public List<List<GameObject>> GetAllGameObjects() => m_vGameObjects;

        public ComponentManager GetComponentManager() => m_vComponentManager;

		public ObstacleManager GetObstacleManager() => m_vObstacleManager;

		public GameObject GetGameObjectByID(int id)
        {
            var classId = GlobalID.GetClassID(id) - 500;
            if (m_vGameObjects.Capacity < classId)
            return null;
            return m_vGameObjects[classId].Find(g => g.GlobalId == id);
        }

        public List<GameObject> GetGameObjects(int id) => m_vGameObjects[id];

        public void Load(JObject jsonObject)
        {
            var jsonBuildings = (JArray) jsonObject["buildings"];
            foreach (JObject jsonBuilding in jsonBuildings)
            {
                var bd = (BuildingData)CSVManager.DataTables.GetDataById(jsonBuilding["data"].ToObject<int>());
                var b = new Building(bd, m_vLevel);
                AddGameObject(b);
                b.Load(jsonBuilding);
            }

            var jsonTraps = (JArray) jsonObject["traps"];
            foreach (JObject jsonTrap in jsonTraps)
            {
                var td = (TrapData)CSVManager.DataTables.GetDataById(jsonTrap["data"].ToObject<int>());
                var t = new Trap(td, m_vLevel);
                AddGameObject(t);
                t.Load(jsonTrap);
            }

            var jsonDecos = (JArray) jsonObject["decos"];

            foreach (JObject jsonDeco in jsonDecos)
            {
                var dd = (DecoData)CSVManager.DataTables.GetDataById(jsonDeco["data"].ToObject<int>());
                var d = new Deco(dd, m_vLevel);
                AddGameObject(d);
                d.Load(jsonDeco);
            }

            if (jsonObject["obstacles"] != null)
            {
                JArray jsonObstacles = (JArray)jsonObject["obstacles"];
                foreach (JObject jsonObstacle in jsonObstacles)
                {
                    var od = (ObstacleData)CSVManager.DataTables.GetDataById(jsonObstacle["data"].ToObject<int>());
                    var o = new Obstacle(od, m_vLevel);
                    AddGameObject(o);
                    o.Load(jsonObstacle);
                }
            }
			m_vObstacleManager.Load(jsonObject);
		}

        public void RemoveGameObject(GameObject go)
        {
            m_vGameObjects[go.ClassId].Remove(go);
            if (go.ClassId == 0)
            {
                var b = (Building) go;
                var bd = b.GetBuildingData();
                if (bd.IsWorkerBuilding())
                {
                    m_vLevel.WorkerManager.DecreaseWorkerCount();
                }
            }
            RemoveGameObjectReferences(go);
        }

        public void RemoveGameObjectReferences(GameObject go)
        {
            m_vComponentManager.RemoveGameObjectReferences(go);
        }

        public JObject Save(int challange = 0, bool reload = false)
        {
            ClientAvatar pl = m_vLevel.Avatar;
            var jsonData = new JObject();
            jsonData = m_vObstacleManager.Save(jsonData); 
            jsonData.Add("exp_ver", 1);
            jsonData.Add("android_client", pl.m_vAndroid);
            jsonData.Add("active_layout", pl.m_vActiveLayout);
            jsonData.Add("war_layout", pl.m_vActiveLayout);
            jsonData.Add("layout_state", new JArray { 0, 0, 0, 0, 0, 0 });
            
            JArray JObstacles = new JArray();
            int o = 0;
            var gembox = false;
            foreach (GameObject go in new List<GameObject>(m_vGameObjects[3]))
            {
                Obstacle d = (Obstacle)go;
                JObject j = new JObject();
                j.Add("data", d.GetObstacleData().GetGlobalID());
                if (d.IsClearingOnGoing())
                    j.Add("clear_t", d.m_vTimer.GetRemainingSeconds(m_vLevel.Avatar.LastTickSaved));
                j.Add("id", 503000000 + o);
                d.Save(j);
                if (removedObstacles != null)
                {
                    if (removedObstacles.Contains(go))
                    {
                        continue;
                    }
                }

                if (gembox && d.GetData().GetGlobalID() == 8000030)
                    continue;
                
                if (!gembox && d.GetData().GetGlobalID() == 8000030)
                    gembox = true;
                
                JObstacles.Add(j);
                o++;
            }
            jsonData.Add("obstacles", JObstacles);

            JArray JBuildings = new JArray();
            int c = 0;
            foreach (GameObject go in new List<GameObject>(m_vGameObjects[0]))
            {
                Building b = (Building)go;
                JObject j = new JObject();
                if (challange == 1 && b.GetData().GetGlobalID() == 1000019)
                    continue;
                try {
                    if (m_vLevel.Avatar.m_vTownHallLevel+1 < Convert.ToInt32(b.GetBuildingData().ReqTh[b.UpgradeLevel]))
                        b.UpgradeLevel--;
                } catch (Exception) {}
                if (b.X == -1 || b.Y == -1)
	                b.SetPositionXY(1, 1, this.m_vLevel.Avatar.m_vActiveLayout);
                j.Add("data", b.GetBuildingData().GetGlobalID());
                j.Add("id", 500000000 + c);
                b.Save(j);
                if (challange != 0)
                    j.Remove("const_t");
                JBuildings.Add(j);
                c++;
            }
            jsonData.Add("buildings", JBuildings);

            JArray JTraps = new JArray();
            int u = 0;
            foreach (GameObject go in new List<GameObject>(m_vGameObjects[4]))
            {
                Trap t = (Trap)go;
                JObject j = new JObject();
                j.Add("data", t.GetTrapData().GetGlobalID());
                j.Add("id", 504000000 + u);
                if (t.X == -1 || t.Y == -1)
                {
	                t.SetPositionXY(1, 1, this.m_vLevel.Avatar.m_vActiveLayout);
                }
                t.Save(j);
                if (challange != 1)
					JTraps.Add(j);
                u++;
            }
            jsonData.Add("traps", JTraps);

            JArray JDecos = new JArray();
            int e = 0;
            foreach (GameObject go in new List<GameObject>(m_vGameObjects[6]))
            {
                Deco d = (Deco)go;
                JObject j = new JObject();
                j.Add("data", d.GetDecoData().GetGlobalID());
                j.Add("id", 506000000 + e);
                if (d.X == -1 || d.Y == -1)
                {
	                d.SetPositionXY(1, 1, this.m_vLevel.Avatar.m_vActiveLayout);
                }
                d.Save(j);
                JDecos.Add(j);
                e++;
            }
            jsonData.Add("decos", JDecos);

			var cooldowns = new JArray();
            jsonData.Add("cooldowns", cooldowns);
            var newShopBuildings = new JArray
            {
                1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000,
                1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000,
                1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000,
                1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 
                1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000
            };
            jsonData.Add("newShopBuildings", newShopBuildings);
            var newShopTraps = new JArray { 8, 8, 8, 8, 8, 8, 8, 8, 8 };
            jsonData.Add("newShopTraps", newShopTraps);
            var newShopDecos = new JArray
            {
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10,
                10
            };
            jsonData.Add("newShopDecos", newShopDecos);
            jsonData.Add("troop_req_msg", pl.TroopRequestMessage);
            jsonData.Add("last_league_rank", pl.m_vLeagueId);
            jsonData.Add("last_league_shuffle", 1);
            jsonData.Add("last_season_seen", 1);
            jsonData.Add("last_news_seen", 999);
            jsonData.Add("edit_mode_shown", true);
            jsonData.Add("war_tutorials_seen", 1);
            jsonData.Add("war_base", true);
            jsonData.Add("help_opened", true);
            jsonData.Add("bool_layout_edit_shown_erase", false);
            if (reload)
            {
                m_vGameObjects          = new List<List<GameObject>>();
                m_vGameObjectRemoveList = new List<GameObject>();
                m_vGameObjectsIndex     = new List<int>();
                for (int i = 0; i < 7; i++)
                {
                    m_vGameObjects.Add(new List<GameObject>());
                    m_vGameObjectsIndex.Add(0);
                }
                m_vComponentManager     = new ComponentManager(m_vLevel);
                m_vLevel.WorkerManager = new WorkerManager();
                Load(jsonData);
            }
            return jsonData;
        }

        public void Tick()
        {
            m_vComponentManager.Tick();
            m_vObstacleManager.Tick();
            foreach (var l in m_vGameObjects)
            {
                foreach (var go in l)
                    go.Tick();
            }
            foreach (var g in new List<GameObject>(m_vGameObjectRemoveList))
            {
                RemoveGameObjectTotally(g);
                m_vGameObjectRemoveList.Remove(g);
            }
        }

        int GenerateGameObjectGlobalId(GameObject go)
        {
            var index = m_vGameObjectsIndex[go.ClassId];
            m_vGameObjectsIndex[go.ClassId]++;
            return GlobalID.CreateGlobalID(go.ClassId + 500, index);
        }

        void RemoveGameObjectTotally(GameObject go)
        {
            m_vGameObjects[go.ClassId].Remove(go);
            if (go.ClassId == 0)
            {
                var b = (Building) go;
                var bd = b.GetBuildingData();
                if (bd.IsWorkerBuilding())
                    m_vLevel.WorkerManager.DecreaseWorkerCount();
            }
            RemoveGameObjectReferences(go);
        }
    }
}

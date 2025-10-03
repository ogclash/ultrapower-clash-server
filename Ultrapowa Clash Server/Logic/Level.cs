using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Logic.Manager;
using UCS.Packets;

namespace UCS.Logic
{
    internal class Level
    {
        internal GameObjectManager GameObjectManager;
        internal WorkerManager WorkerManager;
        internal Device Client;
        internal ClientAvatar Avatar;
        public CombatItemData lastTrainedUnitData;
        public int lastTrainedUnitIndex = 0;
        public bool IsBuildingPending = false;
        public JObject unitProductionJson;

        public Level()
        {
            this.WorkerManager = new WorkerManager();
            this.GameObjectManager = new GameObjectManager(this);
            this.Avatar = new ClientAvatar();
        }

        public Level(long id, string token)
        {
            this.WorkerManager = new WorkerManager();
            this.GameObjectManager = new GameObjectManager(this);
            this.Avatar = new ClientAvatar(id, token);
        }

        public ComponentManager GetComponentManager() => GameObjectManager.GetComponentManager();

        public bool HasFreeWorkers() => WorkerManager.GetFreeWorkers() > 0;

        public void LoadFromJSON(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);
            GameObjectManager.Load(jsonObject);
        }

        public string SaveToJSON() => JsonConvert.SerializeObject(GameObjectManager.Save(), Formatting.Indented);
        public string SaveToJSONforPlayer() => JsonConvert.SerializeObject(GameObjectManager.Save(0, true), Formatting.Indented);
        public string SaveToJSONforChallange() => JsonConvert.SerializeObject(GameObjectManager.Save(1), Formatting.Indented);
        public string SaveToJSONforChallangeAttack() => JsonConvert.SerializeObject(GameObjectManager.Save(2), Formatting.Indented);

        public void SetHome(string jsonHome) => GameObjectManager.Load(JObject.Parse(jsonHome));

        public async Task Tick(bool offline = false)
        {
            this.Avatar.LastTickSaved = DateTime.UtcNow;
            GameObjectManager.Tick(offline);
        }
        
        public async Task startTick()
        {
            try
            {
                this.Avatar.LastTickSaved = DateTime.UtcNow;
                GameObjectManager.Tick(true);
            }
            catch (Exception ex)
            {
                Logger.Write($"Runtime-Error: {ex.Message}");
            }
        }
    }
}

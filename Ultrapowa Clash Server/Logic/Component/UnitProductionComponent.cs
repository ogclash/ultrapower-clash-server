using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;

namespace UCS.Logic
{
    internal class UnitProductionComponent : Component
    {
        public UnitProductionComponent(GameObject go) : base(go)
        {
            m_vUnits = new List<DataSlot>();
            cookedUnits = new List<DataSlot>();
            SetUnitType(go);
            m_vTimer = null;
            m_vIsWaitingForSpace = false;
            this.lastUnitData = GetParent().Avatar.lastTrainedUnitData;
            this.lastIndex = GetParent().Avatar.lastTrainedUnitIndex;
        }

        public override int Type => 3;

        readonly List<DataSlot> m_vUnits;
        readonly List<DataSlot> cookedUnits;
        private CombatItemData lastUnitData;
        private int lastIndex;
        bool m_vIsSpellForge;
        bool currentTick;
        bool m_vIsWaitingForSpace;
        Timer m_vTimer;


        public void setLastUnitData(CombatItemData unitData, int index)
        {
            this.lastUnitData = unitData;
            this.lastIndex = index;
            GetParent().Avatar.lastTrainedUnitData = unitData;
            GetParent().Avatar.lastTrainedUnitIndex = index;
        }

        public void AddUnitToProductionQueue(CombatItemData cd, bool new_method = false)
        {
            if (CanAddUnitToQueue(cd) || new_method)
            {
                int count = getBarracksCount(cd);
                for (var i = 0; i < GetSlotCount(); i++)
                {
                    if ((CombatItemData) m_vUnits[i].Data == cd && lastUnitData == cd && i == lastIndex)
                    {
                        setLastUnitData(cd, i);
                        m_vUnits[i].Value++;
                        return;
                    }
                }
                var ds = new DataSlot(cd, 1);
                setLastUnitData(cd, m_vUnits.Count);
                m_vUnits.Add(ds);
                if (m_vTimer == null && m_vUnits.Count > 0)
                {
                    var ca = GetParent().Avatar.Avatar;
                    m_vTimer = new Timer();
                    var trainingTime = cd.GetTrainingTime(ca.GetUnitUpgradeLevel(cd))/count;
                    m_vTimer.StartTimer(trainingTime-1, DateTime.Now);;
                }
            }
        }
        
        public void AddUnitToProductionQueue(SpellData cd, bool new_method = false)
        {
            if (CanAddUnitToQueue(cd) || new_method)
            {
                for (var i = 0; i < GetSlotCount(); i++)
                {
                    if ((SpellData) m_vUnits[i].Data == cd && lastUnitData == cd && i == lastIndex)
                    {
                        setLastUnitData(cd, i);
                        m_vUnits[i].Value++;
                        return;
                    }
                }
                var ds = new DataSlot(cd, 1);
                setLastUnitData(cd, m_vUnits.Count);
                m_vUnits.Add(ds);
                lastUnitData = cd;
                if (m_vTimer == null)
                {
                    var ca = GetParent().Avatar.Avatar;
                    m_vTimer = new Timer();
                    var trainingTime = cd.GetTrainingTime(ca.GetUnitUpgradeLevel(cd));
                    m_vTimer.StartTimer(trainingTime-1, DateTime.Now);
                }
            }
        }
        
        public bool CanAddUnitToQueue(CombatItemData cd) => GetMaxTrainCount() >= GetTotalCount() + cd.GetHousingSpace();

        public int GetMaxTrainCount()
        {
            var b = (Building) GetParent();
            var bd = b.GetBuildingData();
            return bd.GetUnitProduction(b.GetUpgradeLevel());
        }

        public int GetSlotCount() => m_vUnits.Count;

        public int GetTotalCount()
        {
            var count = 0;
            if (GetSlotCount() >= 1)
            {
                for (var i = 0; i < GetSlotCount(); i++)
                {
                    var cnt = m_vUnits[i].Value;
                    var housingSpace = ((CombatItemData) m_vUnits[i].Data).GetHousingSpace();
                    count += cnt * housingSpace;
                }
            }
            if (m_vIsSpellForge)
            {
                count += GetParent().Avatar.GetComponentManager().GetTotalUsedHousing(true);
            }
            return count;
        }

        public bool IsSpellForge() => m_vIsSpellForge;

        public bool IsWaitingForSpace()
        {
            var result = false;
            if (m_vUnits.Count > 0)
            {
                if (m_vTimer != null)
                {
                    if (m_vTimer.GetRemainingSeconds(DateTime.Now) == 0)
                    {
                        result = m_vIsWaitingForSpace;
                    }
                }
            }
            return result;
        }
        
        public void AddUnitToCookedProduction()
        {
            if (IsSpellForge())
            {
                SpellData cd = m_vUnits.Last().Data as SpellData;
                for (var i = 0; i < cookedUnits.Count; i++)
                {
                    if ((SpellData) cookedUnits[i].Data == cd)
                    {
                        cookedUnits[i].Value++;
                        return;
                    }
                }
                var ds = new DataSlot(cd, 1);
                cookedUnits.Add(ds);
            }
            else
            {
                CombatItemData cd2 = m_vUnits.Last().Data as CombatItemData;
                for (var i = 0; i <  cookedUnits.Count; i++)
                {
                    if ((CombatItemData) cookedUnits[i].Data == cd2)
                    {
                        cookedUnits[i].Value++;
                        return;
                    }
                }
                var ds = new DataSlot(cd2, 1);
                cookedUnits.Add(ds);
            }
            Logger.Say("Added unit to precooked");
        }
        
        
        public bool ProductionCompleted()
        {
            if (m_vUnits.Count <= 0)
                return false;
            bool result = false;
            CombatItemData cda;
            /*if (m_vUnits.Count <= 0)
            {
                if (cookedUnits.Count > 0)
                    cda = (CombatItemData)cookedUnits[0].Data;
                else
                    return result;
            }
            else*/
            cda = (CombatItemData) m_vUnits[0].Data;
            int housingspace = cda.GetHousingSpace();
            if (cda.GetGlobalID().ToString().StartsWith("400"))
            {
                List<DataSlot> _PlayerUnits = GetParent().Avatar.Avatar.GetUnits();
                int usedspace = 0;
                foreach (var unit in _PlayerUnits)
                {
                    if (unit.Value < 0)
                        unit.Value = 0;
                    var unitData = (CharacterData) unit.Data;
                    var housingSpace = unitData.HousingSpace;
                    usedspace += unit.Value * housingSpace;
                }
                int maxspace = GetParent().Avatar.GetComponentManager().GetTotalMaxHousing();
                if (housingspace+usedspace <= maxspace)
                {
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == cda.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value += 1;
                    }
                    else
                    {
                        DataSlot ds = new DataSlot(cda, 1);
                        _PlayerUnits.Add(ds);
                    }
                    StartProducingNextUnit();
                    result = true;
                }
                else
                {
                    m_vIsWaitingForSpace = true;
                }
            }
            else
            {
                List<DataSlot> _PlayerSpells = GetParent().Avatar.Avatar.GetSpells();
                int usedspace = 0;
                foreach (var unit in _PlayerSpells)
                {
                    if (unit.Value < 0)
                        unit.Value = 0;
                    var unitData = (UCS.Files.Logic.SpellData) unit.Data;
                    var housingSpace = unitData.HousingSpace;
                    usedspace += unit.Value * housingSpace;
                }
                int maxspace = GetParent().Avatar.GetComponentManager().GetTotalMaxHousing(true);
                if (housingspace+usedspace <= maxspace)
                {
                    DataSlot _DataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == cda.GetGlobalID());
                    if (_DataSlot != null)
                    {
                        _DataSlot.Value += 1;
                    }
                    else
                    {
                        DataSlot ds = new DataSlot(cda, 1);
                        _PlayerSpells.Add(ds);
                    }
                    StartProducingNextUnit();
                    result = true;
                }
                else
                {
                    m_vIsWaitingForSpace = true;
                }
            }
            //StartProducingNextUnit();
            return result;
            var cf = new ComponentFilter(0);
            var x = GetParent().X;
            var y = GetParent().Y;
            var cm = GetParent().Avatar.GetComponentManager();
            var c = cm.GetClosestComponent(x, y, cf);

            while (c != null)
            {
                Data d = null;
                if (m_vUnits.Count > 0)
                    d = m_vUnits[0].Data;
                if (!((UnitStorageComponent) c).CanAddUnit((CombatItemData) d))
                {
                    cf.AddIgnoreObject(c.GetParent());
                    c = cm.GetClosestComponent(x, y, cf);
                }
                else
                    break;
            }

            if (c != null)
            {
                var cd = (CombatItemData) m_vUnits[0].Data;
                ((UnitStorageComponent) c).AddUnit(cd);
                StartProducingNextUnit();
                result = true;
            }
            else
            {
                m_vIsWaitingForSpace = true;
            }
            return result;
        }
        
        public void StartProducingNextUnit()
        {
            m_vTimer = null;
            if (GetSlotCount() >= 1)
            {
                RemoveUnit((CombatItemData) m_vUnits[0].Data);
            }
        }
        

        public void RemoveUnit(CombatItemData cd, int input_index = -1)
        {
            if (input_index == 0 && m_vUnits[0].Data != cd)
                return;
            if (input_index > m_vUnits.Count - 1 && cd == m_vUnits[m_vUnits.Count - 1].Data)
                input_index = m_vUnits.Count - 1;
            if (input_index >= 0 && input_index < m_vUnits.Count && m_vUnits[input_index].Data != cd)
            {
                if (input_index + 1 < m_vUnits.Count && m_vUnits[input_index + 1].Data == cd)
                    input_index += 1;
                else if (input_index - 1 >= 0 && m_vUnits[input_index - 1].Data == cd)
                    input_index -= 1;
                else
                    return;
            }
            int index = -1;
            if (input_index != -1)
                index = input_index;
            else
                index = 0;
            if (m_vUnits[index].Data == cd)
            {
                if (m_vUnits[index].Value >= 1)
                {
                    m_vUnits[index].Value--;
                    if (m_vUnits[index].Value == 0)
                    {
                        m_vUnits.RemoveAt(index);
                        if (GetSlotCount() >= 1)
                        {
                            var newcd = (CombatItemData) m_vUnits[0].Data;
                            if (m_vTimer == null)
                            {
                                var trainingTime =  0;
                                m_vTimer = new Timer();
                                if (newcd.GetGlobalID().ToString().StartsWith("400"))
                                    trainingTime = newcd.GetTrainingTime(0)/getBarracksCount(newcd);
                                else
                                    trainingTime = newcd.GetTrainingTime(0);
                                m_vTimer.StartTimer(trainingTime, DateTime.Now);
                            }
                        }
                        else
                        {
                            m_vTimer = null;
                        }
                    }
                    else
                    {
                        if (m_vTimer == null)
                        {
                            m_vTimer = new Timer();
                            var trainingTime =  0;
                            if (cd.GetGlobalID().ToString().StartsWith("400"))
                                trainingTime = cd.GetTrainingTime(0)/getBarracksCount(cd);
                            else
                                trainingTime = cd.GetTrainingTime(0);
                            m_vTimer.StartTimer(trainingTime, DateTime.Now);
                        }
                    }
                }
            }
        }

        public List<GameObject> getBarracks()
        {
            List<GameObject> barracks = new List<GameObject>();
            foreach (GameObject gameObject in GetParent().Avatar.GameObjectManager.GetAllGameObjects()[0])
            {
                if (gameObject.GetData().GetGlobalID() == 1000006)
                {
                    barracks.Add(gameObject);
                }
            }
            return barracks;
        }
        
        public List<GameObject> getDarkBarracks()
        {
            List<GameObject> barracks = new List<GameObject>();
            foreach (GameObject gameObject in GetParent().Avatar.GameObjectManager.GetAllGameObjects()[0])
            {
                if (gameObject.GetData().GetGlobalID() == 1000026)
                {
                    barracks.Add(gameObject);
                }
            }
            return barracks;
        }

        public int getBarracksCount(CombatItemData cd)
        {
            CharacterData c = (CharacterData)cd;
            List<GameObject> barracks = new List<GameObject>();
            if (c.UpgradeResource[0] == "DarkElixir")
                barracks = getDarkBarracks();
            else
                barracks = getBarracks();
            int count = 0;
            foreach (GameObject barrack in barracks)
            {
                Building building = (Building)barrack;
                if (building.UpgradeLevel + 1 >= ((CharacterData)cd).BarrackLevel)
                    count++;
            }
            if (count == 0)
                count = 1;
            return count;
        }
        
        public void SetUnitType(GameObject go)
        {
            var b = (Building) GetParent();
            var bd = b.GetBuildingData();
            m_vIsSpellForge = bd.IsSpellForge();
        }

        public void SpeedUp()
        {
            while (m_vUnits.Count >= 1 && ProductionCompleted())
            {
            }
        }

        
        public override void Load(JObject jsonObject)
        {
            var unitProdObject = (JObject) jsonObject["unit_prod"];
            m_vIsSpellForge = unitProdObject["unit_type"].ToObject<int>() == 1;
            var timeToken = unitProdObject["t"];
            if (timeToken != null)
            {
                m_vTimer = new Timer();
                var remainingTime = timeToken.ToObject<int>();
                m_vTimer.StartTimer(remainingTime, DateTime.Now);
            }
            var unitJsonArray = (JArray) unitProdObject["slots"];
            if (unitJsonArray != null)
            {
                foreach (JObject unitJsonObject in unitJsonArray)
                {
                    var id = unitJsonObject["id"].ToObject<int>();
                    var cnt = unitJsonObject["cnt"].ToObject<int>();
                    m_vUnits.Add(new DataSlot(CSVManager.DataTables.GetDataById(id), cnt));
                    /*if (unitJsonObject["cooked"].ToObject<int>() == 0)
                        m_vUnits.Add(new DataSlot(CSVManager.DataTables.GetDataById(id), cnt));
                    else
                        cookedUnits.Add(new DataSlot(CSVManager.DataTables.GetDataById(id), cnt));*/
                }
            }

            if (m_vUnits.Count > 0)
                setLastUnitData((CombatItemData)m_vUnits.Last().Data, m_vUnits.Count-1);
        }

        public override JObject Save(JObject jsonObject)
        {
            var unitProdObject = new JObject();
            if (m_vIsSpellForge)
                unitProdObject.Add("unit_type", 1);
            else
                unitProdObject.Add("unit_type", 0);
            
            if (m_vTimer != null)
            {
                unitProdObject.Add("t", m_vTimer.GetRemainingSeconds(DateTime.Now));
            }

            if (GetSlotCount() >= 1)
            {
                var unitJsonArray = new JArray();
                
                /*for (int i = 0; i < this.cookedUnits.Count(); i++)
                {
                    var unit = cookedUnits[i];
                    var unitJsonObject = new JObject();
                    unitJsonObject.Add("id", unit.Data.GetGlobalID());
                    unitJsonObject.Add("cnt", unit.Value);
                    unitJsonObject.Add("cooked", 1);
                    unitJsonArray.Add(unitJsonObject);
                }*/
                    
                foreach (var unit in m_vUnits)
                {
                    var unitJsonObject = new JObject();
                    unitJsonObject.Add("id", unit.Data.GetGlobalID());
                    unitJsonObject.Add("cnt", unit.Value);
                    unitJsonObject.Add("cooked", 0);
                    unitJsonArray.Add(unitJsonObject);
                }
                unitProdObject.Add("slots", unitJsonArray);
            }
            jsonObject.Add("unit_prod", unitProdObject);
            return jsonObject;
        }
        
        public override void Tick()
        {
            InternalTick();
        }

        public void InternalTick()
        {
            if (!currentTick)
            {
                currentTick = true;
                if (m_vUnits.Count > 0)
                {
                    //Logger.Say($"{GetParent().Avatar.Avatar.AvatarName} [{GetParent().Avatar.Avatar.UserId}] producing Unit: {m_vUnits[0].Data.GetName()} (1/{m_vUnits[0].Value})....");
                    if (m_vTimer.GetRemainingSeconds(DateTime.Now) <= 0)
                    {
                        string UnitName = m_vUnits[0].Data.GetName();
                        int UnitValue = m_vUnits[0].Value;
                        if(ProductionCompleted())
                            Logger.Say($"{GetParent().Avatar.Avatar.AvatarName} [{GetParent().Avatar.Avatar.UserId}] Produced Unit: {UnitName} (1/{UnitValue})");
                        else
                            Logger.Say($"{GetParent().Avatar.Avatar.AvatarName} [{GetParent().Avatar.Avatar.UserId}] Produced Unit ({m_vUnits[0].Data.GetName()}) is waiting for space");
                    }
                }
                currentTick = false;
            }
        }
        
        public void OfflineTick()
        {
            if (m_vTimer != null && m_vUnits.Count > 0 && !IsWaitingForSpace())
            {
                InternalTick();
            }
        }
    }
}

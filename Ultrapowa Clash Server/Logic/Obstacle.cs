using Newtonsoft.Json.Linq;
using System;
using UCS.Core;
using UCS.Helpers;
using UCS.Files.Logic;

namespace UCS.Logic
{
	internal class Obstacle : GameObject
	{
		private readonly Level m_vLevel;

		public Timer m_vTimer;

		public Obstacle(Data data, Level l) : base(data, l)
		{
			AddComponent(new ObstacleComponent(this));
			m_vLevel = l;
		}

        public override int ClassId => 3;

		public void CancelClearing()
		{
			m_vTimer = null;
			m_vLevel.WorkerManager.DeallocateWorker(this);
			var od = GetObstacleData();
			var rd = od.GetClearingResource();
			var cost = od.ClearCost;
			Avatar.Avatar.CommodityCountChangeHelper(0, rd, cost);
		}

		public void ClearingFinished()
		{
			m_vLevel.GameObjectManager.GetObstacleManager().IncreaseObstacleClearCount();
			m_vLevel.WorkerManager.DeallocateWorker(this);
			m_vTimer = null;
            Avatar.GameObjectManager.RemoveObstalce(this);
			LootObstacle();
		}

		public void LootObstacle()
		{
			var constructionTime = GetObstacleData().ClearTimeSeconds;
			var exp = (int)Math.Sqrt(constructionTime);
			Avatar.Avatar.AddExperience(exp);
			
			var rd = CSVManager.DataTables.GetResourceByName(GetObstacleData().LootResource);
			var count = GetObstacleData().LootCount;
			Avatar.Avatar.CommodityCountChangeHelper(0, rd, GetObstacleData().LootCount);
		}

        public ObstacleData GetObstacleData() => (ObstacleData)GetData();

        public int GetRemainingClearingTime() => m_vTimer.GetRemainingSeconds(m_vLevel.Avatar.LastTickSaved);

        public bool IsClearingOnGoing() => m_vTimer != null;

        public void SpeedUpClearing()
		{
			var remainingSeconds = 0;
			if (IsClearingOnGoing())
			{
				remainingSeconds = m_vTimer.GetRemainingSeconds(m_vLevel.Avatar.LastTickSaved);
			}
			var cost = GamePlayUtil.GetSpeedUpCost(remainingSeconds);
			var ca = Avatar.Avatar;
			if (ca.HasEnoughDiamonds(cost))
			{
				ca.UseDiamonds(cost);
				ClearingFinished();
			}
		}

		public void StartClearing()
		{
			double constructionTime = GetObstacleData().ClearTimeSeconds-0.5;
			if (constructionTime < 1)
			{
				ClearingFinished();
			}
			else
			{
				//Avatar.GameObjectManager.RemoveObstacle(this);
				m_vTimer = new Timer();
				m_vTimer.StartTimerDouble(constructionTime, m_vLevel.Avatar.LastTickSaved);
				m_vLevel.WorkerManager.AllocateWorker(this);
			}
		}

		public override void Tick()
		{
			if (IsClearingOnGoing())
			{
				if (m_vTimer.GetRemainingSecondsDouble(m_vLevel.Avatar.LastTickSaved) <= 0)
					ClearingFinished();
			}
		}

		public new void Load(JObject jsonObject)
		{
			m_vLevel.WorkerManager.DeallocateWorker(this);
			var constTimeToken = jsonObject["clear_t"];
			if (constTimeToken != null)
			{
				m_vTimer = new Timer();
				var remainingConstructionTime = constTimeToken.ToObject<int>();
				m_vTimer.StartTimer(remainingConstructionTime, m_vLevel.Avatar.LastTickSaved);
				m_vLevel.WorkerManager.AllocateWorker(this);
			}
			base.Load(jsonObject);
		}

		public JObject ToJson()
		{
			var jsonObject = new JObject();
			jsonObject.Add("data", GetObstacleData().GetGlobalID());
			if (IsClearingOnGoing())
				jsonObject.Add("clear_t", m_vTimer.GetRemainingSecondsDouble(m_vLevel.Avatar.LastTickSaved));
			jsonObject.Add("x", X);
			jsonObject.Add("y", Y);
			return jsonObject;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
	internal class TrainQuickUnitsCommand : Command
	{
	    public TrainQuickUnitsCommand(Reader reader, Device client, int id) : base(reader, client, id)
	    {
	        
	    }

	    internal override void Decode()
		{
		    this.DataSlotID = this.Reader.ReadInt32(); 
			this.Reader.ReadInt32(); 
		}

	    public int DataSlotID;
	    public ClientAvatar player;

		internal override void Process()
		{
			this.player = this.Device.Player.Avatar;

			if (DataSlotID == 1)
			{
				AddToTraining(player.QuickTrain1);
            }
			else if (DataSlotID == 2)
			{
				AddToTraining(player.QuickTrain2);
			}
			else if (DataSlotID == 3)
			{
				AddToTraining(player.QuickTrain3);
			}
			else
			{
				AddToTraining(player.QuickTrain1);
			}
		}

		internal List<DataSlot> AddToTraining(List<DataSlot> quicktrain)
		{
			List<DataSlot> _PlayerUnits = player.GetUnits();
			List<DataSlot> _PlayerSpells = player.GetSpells();
			var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
			var troops = 0;
			foreach (var unit in _PlayerUnits)
			{
				try
				{
					var unitData = (UCS.Files.Logic.CharacterData) unit.Data;
					var housingSpace = unitData.HousingSpace; 
					troops += unit.Value * housingSpace;
				}catch(Exception) {}
			}
			var spells = 0;
			foreach (var unit in _PlayerSpells)
			{
				if (unit.Value < 0)
					unit.Value = 0;
				var unitData = (UCS.Files.Logic.SpellData) unit.Data;
				var housingSpace = unitData.HousingSpace;
				spells += unit.Value * housingSpace;
			}
			var sorted = quicktrain
				.OrderBy(i =>
				{
					// troop = 0, spell = 1
					return i.Data is SpellData ? 1 : 0;
				})
				.ThenBy(i =>
				{
					int resourceOrder = 0;
					// normal elixir = 0, dark elixir = 1
					if (i.Data is CharacterData)
					{
						CharacterData c = (CharacterData)i.Data;
						if (c.UpgradeResource[0] == "DarkElixir")
							resourceOrder = 1;
					}
					else if (i.Data is SpellData)
					{
						SpellData s = (SpellData)i.Data;
						if (s.UpgradeResource[0] == "DarkElixir")
							resourceOrder = 1;
					}

					return resourceOrder;
				})
				.ThenBy(i =>
				{
					if (i.Data is CharacterData)
					{
						CharacterData c = (CharacterData)i.Data;
						return c.BarrackLevel;
					}

					if (i.Data is SpellData)
					{
						SpellData s = (SpellData)i.Data;
						return s.SpellForgeLevel;
					}
					return int.MaxValue;
				})
				.ToList();
			
			foreach (DataSlot i in sorted)
			{
				DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
				int maxTrainableUnits = i.Value;
				if (i.Data.GetGlobalID().ToString().StartsWith("400"))
				{
					int traincount = 0;
					UnitProductionComponent barrack = (UnitProductionComponent)this.Device.Player.GameObjectManager.GetGameObjectByID(500000010).GetComponent(3);
					foreach (var newbarrack in barrack.getBarracks())
					{
						newbarrack.GetComponent(3);
						UnitProductionComponent c = (UnitProductionComponent)newbarrack.GetComponent(3);
						traincount += c.GetTotalCount();
					}

					CharacterData cd = (CharacterData) i.Data;
					traincount = gameobjects.GetTotalMaxHousing()*2-traincount;
					traincount = (traincount-troops)/cd.HousingSpace;
					
					if (i.Value < traincount)
						traincount = i.Value;
					for (int j = 0; j < traincount; j++)
					{
						barrack.AddUnitToProductionQueue(cd, true);
					}
					continue;
					if (_DataSlot != null)
					{
						var unitData = (UCS.Files.Logic.CharacterData) _DataSlot.Data;
						var housingSpace = unitData.HousingSpace;
		                    
						int housingLeft = gameobjects.GetTotalMaxHousing() - troops;
						if (i.Value * housingSpace > housingLeft)
						{
							maxTrainableUnits = housingLeft / housingSpace;
						}
		                    
						troops += maxTrainableUnits * housingSpace;
					}

					if (troops <= gameobjects.GetTotalMaxHousing())
					{
						if (_DataSlot != null)
						{
							_DataSlot.Value += maxTrainableUnits;
						}
						else
						{
							DataSlot ds = new DataSlot(i.Data, maxTrainableUnits);
							_PlayerUnits.Add(ds);
						}
					}
				}
				else
				{
					List<GameObject> buildings = this.Device.Player.GameObjectManager.GetAllGameObjects()[0];
					List<GameObject> factories = new List<GameObject>();
					foreach (GameObject gameObject in buildings)
					{
						if (gameObject.GetData().GetGlobalID() == 1000020)
						{
							factories.Add(gameObject);
						}
					}
					UnitProductionComponent factory = (UnitProductionComponent)factories[0].GetComponent(3, false);
					int traincount = factory.GetTotalCount();
					traincount = gameobjects.GetTotalMaxHousing()*2-traincount;
					SpellData cd = (SpellData) i.Data;
					traincount = (traincount-troops)/cd.HousingSpace;
					
					if (i.Value < traincount)
						traincount = i.Value;
					for (int j = 0; j < traincount; j++)
					{
						factory.AddUnitToProductionQueue(cd, true);
					}
					continue;
					
					if (_DataSlot != null)
					{
						SpellData _SpellData = (SpellData)_DataSlot.Data;
						if (_SpellData != null)
						{
							var spellData = (SpellData)_SpellData;
							var housingSpace = spellData.HousingSpace;
							spells += maxTrainableUnits * housingSpace;
						}
					}

					if (spells < gameobjects.GetTotalMaxHousing(true))
					{
						DataSlot _NewDataSlot = _PlayerSpells.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
						if (_NewDataSlot != null)
						{
							_NewDataSlot.Value = _NewDataSlot.Value + maxTrainableUnits;
						}
						else
						{
							DataSlot ds = new DataSlot(i.Data, maxTrainableUnits);
							_PlayerSpells.Add(ds);
						}
						
					}
				}
				

			}
			return quicktrain;
		}

	}
}

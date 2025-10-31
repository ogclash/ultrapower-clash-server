using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core.Settings;
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

		internal void AddToTraining(List<DataSlot> quicktrain)
		{
			List<DataSlot> _PlayerUnits = player.GetUnits();
			List<DataSlot> _PlayerSpells = player.GetSpells();
			int troops = 0;
			foreach (var unit in _PlayerUnits)
			{
				try
				{
					int housingSpace = ((CharacterData) unit.Data).HousingSpace; 
					troops += unit.Value * housingSpace;
				}catch(Exception) {}
			}
			int spells = 0;
			foreach (var unit in _PlayerSpells)
			{
				if (unit.Value < 0)
					unit.Value = 0;
				var housingSpace = ((SpellData) unit.Data).HousingSpace;
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
			UnitProductionComponent barrack = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetMainProduction().GetComponent(3);
			UnitProductionComponent factory = (UnitProductionComponent) this.Device.Player.GameObjectManager.GetMainProduction(true).GetComponent(3);
			foreach (DataSlot i in sorted)
			{
				if (!Constants.DeveloperBuild)
					if ((i.Data.GetGlobalID() == 4000030 || i.Data.GetGlobalID() == 26000004) && DateTime.Now.Month != 12 && DateTime.Now.Month != 1)
						continue;
				if (i.Data.GetGlobalID().ToString().StartsWith("400"))
				{
					CharacterData cd = (CharacterData) i.Data;
					int traincount = this.Device.Player.GameObjectManager.GetComponentManager().GetTotalMaxHousing()*2;
					traincount = (traincount-troops)/cd.HousingSpace;
					
					if (i.Value < traincount)
						traincount = i.Value;
					
					for (int j = 0; j < traincount; j++)
						barrack.AddUnitToProductionQueue(cd, true);
					
					ResourceData _TrainingResource = cd.GetTrainingResource();
					for (int j = 0; j < traincount; j++)
						this.Device.Player.Avatar.SetResourceCount(_TrainingResource, this.Device.Player.Avatar.GetResourceCount(_TrainingResource) - cd.GetTrainingCost(this.Device.Player.Avatar.GetUnitUpgradeLevel(cd)));
				}
				else
				{
					if (factory == null)
						continue;
					int traincount = factory.GetTotalCount();
					traincount = this.Device.Player.GameObjectManager.GetComponentManager().GetTotalMaxHousing()*2-traincount;
					SpellData cd = (SpellData) i.Data;
					traincount = (traincount-spells)/cd.HousingSpace;
					
					if (i.Value < traincount)
						traincount = i.Value;
					
					for (int j = 0; j < traincount; j++)
						factory.AddUnitToProductionQueue(cd, true);
					
					ResourceData _CastResource = cd.GetTrainingResource();
					for (int j = 0; j < traincount; j++)
						this.Device.Player.Avatar.SetResourceCount(_CastResource, this.Device.Player.Avatar.GetResourceCount(_CastResource) - cd.GetTrainingCost(this.Device.Player.Avatar.GetUnitUpgradeLevel(cd)));
				}
				

			}
		}

	}
}

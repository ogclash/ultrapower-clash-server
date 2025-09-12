using System;
using System.Collections.Generic;
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

			foreach (DataSlot i in quicktrain)
			{
				
				DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
				int maxTrainableUnits = i.Value;
				if (i.Data.GetGlobalID().ToString().StartsWith("400"))
				{
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

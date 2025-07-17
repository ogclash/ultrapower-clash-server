using System.Collections.Generic;
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
			this.Tick = this.Reader.ReadInt32(); 
		}

	    public int DataSlotID;
	    public int Tick;

		internal override void Process()
		{
			var player = this.Device.Player.Avatar;

			if (DataSlotID == 1)
			{
				foreach (DataSlot i in player.QuickTrain1)
				{
                    List<DataSlot> _PlayerUnits = player.GetUnits();
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
                    var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
                    var troops = 0;
                    foreach (var unit in _PlayerUnits)
                    {
	                    var unitData = (UCS.Files.Logic.CharacterData) unit.Data;
	                    var housingSpace = unitData.HousingSpace;
	                    troops += unit.Value * housingSpace;
                    }

                    int maxTrainableUnits = i.Value;
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
	                        _DataSlot.Value = _DataSlot.Value + maxTrainableUnits;
	                    }
	                    else
	                    {
		                    DataSlot ds = new DataSlot(i.Data, maxTrainableUnits);
		                    _PlayerUnits.Add(ds);
	                    }
                    }
                }
            }
			else if (DataSlotID == 2)
			{
				foreach (DataSlot i in player.QuickTrain2)
				{
                    List<DataSlot> _PlayerUnits = player.GetUnits();
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
                    var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
                    var troops = 0;
                    foreach (var unit in _PlayerUnits)
                    {
	                    var unitData = (UCS.Files.Logic.CharacterData) unit.Data;
	                    var housingSpace = unitData.HousingSpace;
	                    troops += unit.Value * housingSpace;
                    }

                    int maxTrainableUnits = i.Value;
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
		                    _DataSlot.Value = _DataSlot.Value + maxTrainableUnits;
	                    }
	                    else
	                    {
		                    DataSlot ds = new DataSlot(i.Data, maxTrainableUnits);
		                    _PlayerUnits.Add(ds);
	                    }
                    }
				}
			}
			else if (DataSlotID == 3)
			{
				foreach (DataSlot i in player.QuickTrain3)
				{
                    List<DataSlot> _PlayerUnits = player.GetUnits();
                    DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == i.Data.GetGlobalID());
                    var gameobjects = this.Device.Player.GameObjectManager.GetComponentManager();
                    var troops = 0;
                    foreach (var unit in _PlayerUnits)
                    {
	                    var unitData = (UCS.Files.Logic.CharacterData) unit.Data;
	                    var housingSpace = unitData.HousingSpace;
	                    troops += unit.Value * housingSpace;
                    }

                    int maxTrainableUnits = i.Value;
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
		                    _DataSlot.Value = _DataSlot.Value + maxTrainableUnits;
	                    }
	                    else
	                    {
		                    DataSlot ds = new DataSlot(i.Data, maxTrainableUnits);
		                    _PlayerUnits.Add(ds);
	                    }
                    }
				}
			}
		}
		
	}
}

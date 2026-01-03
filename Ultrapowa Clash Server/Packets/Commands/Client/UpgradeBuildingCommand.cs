using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;
using System.Diagnostics;

namespace UCS.Packets.Commands.Client
{
    // Packet 502
    internal class UpgradeBuildingCommand : Command
    {
        public int BuildingId;
        public uint UpgradeWithEilixir;

        public UpgradeBuildingCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            
        }
        internal override void Decode()
        {
            this.BuildingId = this.Reader.ReadInt32();
            this.UpgradeWithEilixir = this.Reader.ReadByte();
            this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);
            if (go != null)
            {
                var b = (ConstructionItem)go;
                var bd = b.GetConstructionItemData();
                int cost = bd.GetBuildCost(b.GetUpgradeLevel() + 1);
                if (this.Device.Player.HasFreeWorkers())
                {
                    ResourceData rd = null;
                    if (UpgradeWithEilixir == 1)
                    {
                        rd = CSVManager.DataTables.GetResourceByName("Elixir");
                    }
                    else
                    {
                        rd = bd.GetBuildResource(b.GetUpgradeLevel());
                    }
                    if (ca.HasEnoughResources(rd, cost))
                    {
                        string name = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId).GetData().GetName();
                        Logger.Say("Building To Upgrade : " + name + " (" + BuildingId + ')');
                        
                        b.StartUpgrading();
                        ca.SetResourceCount(rd, ca.GetResourceCount(rd) - cost);
                    }
                    else
                    {
                        if (ca.AddCheatFlag())
                            ResourcesManager.DisconnectClient(level.Client);
                    }
                }
            }
            else
            {
                Debug.Write("[Debug] some how gameobject is equal to null");
            }
        }
    }
}

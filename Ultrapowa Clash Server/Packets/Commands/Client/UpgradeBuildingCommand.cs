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
                if (this.Device.Player.HasFreeWorkers())
                {
                    string name = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId).GetData().GetName();
                    Logger.Say("Building To Upgrade : " + name + " (" + BuildingId + ')');
                    
                    b.StartUpgrading();
                }
                if (UpgradeWithEilixir == 1)
                {
                    ResourceData elixirLocation = CSVManager.DataTables.GetResourceByName("Elixir");
                    ca.SetResourceCount(elixirLocation, ca.GetResourceCount(elixirLocation) - bd.GetBuildCost(b.GetUpgradeLevel() + 1));
                    b.StartUpgrading();
                }
                else
                {
                    var rd = bd.GetBuildResource(b.GetUpgradeLevel() + 1);
                    ca.SetResourceCount(rd, ca.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel() + 1));
                }
            }
            else
            {
                Debug.Write("[Debug] some how gameobject is equal to null");
            }
        }
    }
}

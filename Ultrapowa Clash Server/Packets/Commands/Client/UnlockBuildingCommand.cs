using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 520
    internal class UnlockBuildingCommand : Command
    {
        public UnlockBuildingCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            
        }

        internal override void Decode()
        {
            this.BuildingId = this.Reader.ReadInt32();
            this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);

            var b = (ConstructionItem) go;

            var bd = (BuildingData) b.GetConstructionItemData();

            string name = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId).GetData().GetName();
            
            ca.SetAllianceCastleLevel(0);
            Building a = (Building)this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);
            BuildingData al = a.GetBuildingData();
            ca.SetAllianceCastleTotalCapacity(al.GetUnitStorageCapacity(ca.GetAllianceCastleLevel()));
            Logger.Write("Unlocking Building: " + name + " (" + BuildingId + ')');
            b.Unlock();
            var rd = bd.GetBuildResource(b.GetUpgradeLevel());
            if (ca.GetResourceCount(rd)-bd.GetBuildCost(b.GetUpgradeLevel()) < 0)
                ca.SetResourceCount(rd, 0);
            else 
                ca.SetResourceCount(rd, ca.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel()));
        }

        public int BuildingId;
    }
}
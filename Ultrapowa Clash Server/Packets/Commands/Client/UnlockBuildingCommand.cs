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
            this.Unknown1 = this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);

            var b = (ConstructionItem) go;

            var bd = (BuildingData) b.GetConstructionItemData();

            string name = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId).GetData().GetName();
            
            if (string.Equals(name, "Alliance Castle"))
            {
                ca.IncrementAllianceCastleLevel();
                Building a = (Building)this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);
                BuildingData al = a.GetBuildingData();
                ca.SetAllianceCastleTotalCapacity(al.GetUnitStorageCapacity(ca.GetAllianceCastleLevel()));
            }
            if (ca.HasEnoughResources(bd.GetBuildResource(b.GetUpgradeLevel()), bd.GetBuildCost(b.GetUpgradeLevel())))
            {
                Logger.Write("Unlocking Building: " + name + " (" + BuildingId + ')');
                var rd = bd.GetBuildResource(b.GetUpgradeLevel());
                ca.SetResourceCount(rd, ca.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel()));
                b.Unlock();
            }
        }

        public int BuildingId;
        public uint Unknown1;
    }
}
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
            var b = (ConstructionItem) this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);
            var bd = (BuildingData) b.GetConstructionItemData();

            string name = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId).GetData().GetName();
            
            var rd = bd.GetBuildResource(b.GetUpgradeLevel());
            if (this.Device.Player.Avatar.HasEnoughResources(rd, bd.GetBuildCost(0)))
            {
                Logger.Write("Unlocking Building: " + name + " (" + BuildingId + ')');
                b.Unlock();
                this.Device.Player.Avatar.CastleUnlocked = true;
                
                if (this.Device.Player.Avatar.GetResourceCount(rd)-bd.GetBuildCost(b.GetUpgradeLevel()) < 0)
                    this.Device.Player.Avatar.SetResourceCount(rd, 0);
                else 
                    this.Device.Player.Avatar.SetResourceCount(rd, this.Device.Player.Avatar.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel()));
            }
        }

        public int BuildingId;
    }
}
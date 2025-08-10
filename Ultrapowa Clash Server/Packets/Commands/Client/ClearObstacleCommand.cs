using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 507
    internal class ClearObstacleCommand : Command
    {
        public ClearObstacleCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.ObstacleId = this.Reader.ReadInt32();
            this.Tick = this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            ClientAvatar playerAvatar = this.Device.Player.Avatar;
            Obstacle gameObjectByID = (Obstacle)this.Device.Player.GameObjectManager.GetGameObjectByID(ObstacleId);
            ObstacleData obstacleData = gameObjectByID.GetObstacleData();
            if (playerAvatar.HasEnoughResources(obstacleData.GetClearingResource(), obstacleData.ClearCost) &&  this.Device.Player.HasFreeWorkers())
            {
                gameObjectByID.StartClearing();
                ResourceData clearingResource = obstacleData.GetClearingResource();
                playerAvatar.SetResourceCount(clearingResource, playerAvatar.GetResourceCount(clearingResource) - obstacleData.ClearCost);
            }
        }

        public int ObstacleId;
        public uint Tick;
    }
}

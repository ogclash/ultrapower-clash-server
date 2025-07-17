using System.Threading.Tasks;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 500
    internal class BuyBuildingCommand : Command
    {
        public BuyBuildingCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.BuildingId = this.Reader.ReadInt32();
            this.Unknown1 = this.Reader.ReadUInt32();
        }
        private void TryBuildWithRetry(BuildingData bd, Building b, int attempt)
        {
            var ca = this.Device.Player.Avatar;

            if (!bd.IsWorkerBuilding() && !this.Device.Player.HasFreeWorkers())
            {
                if (attempt >= 15)
                {
                    Logger.Say("Failed to find free worker after 10 attempts.");
                    this.Device.Player.IsBuildingPending = false;
                    return;
                }

                Task.Delay(1000).ContinueWith(_ =>
                {
                    TryBuildWithRetry(bd, b, attempt + 1);
                });

                return;
            }

            var rd = bd.GetBuildResource(0);
            ca.CommodityCountChangeHelper(0, rd, -bd.GetBuildCost(0));

            b.StartConstructing(X, Y);
            this.Device.Player.GameObjectManager.AddGameObject(b);

            this.Device.Player.IsBuildingPending = false;
            Logger.Say("Construction started successfully.");
        }



        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;
            var bd = (BuildingData)CSVManager.DataTables.GetDataById(BuildingId);
            var b = new Building(bd, this.Device.Player);

            if (!ca.HasEnoughResources(bd.GetBuildResource(0), bd.GetBuildCost(0)))
                return;

            // Prevent duplicate scheduling
            if (this.Device.Player.IsBuildingPending)
                return;

            this.Device.Player.IsBuildingPending = true;

            TryBuildWithRetry(bd, b, 0);
        }


        public int BuildingId;
        public uint Unknown1;
        public int X;
        public int Y;
    }
}
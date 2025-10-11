using System;
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



        internal override void Process()
        {
            if(this.Device.Player.GameObjectManager.removedObstacles != null && this.Device.Player.GameObjectManager.removedObstacles.Count > 0)
                this.Device.Player.SaveToJSONforPlayer();
            var ca = this.Device.Player.Avatar;
            var bd = (BuildingData)CSVManager.DataTables.GetDataById(BuildingId);
            var b = new Building(bd, this.Device.Player);


            if (!bd.IsWorkerBuilding() && !this.Device.Player.HasFreeWorkers())
                return;
            

            var rd = bd.GetBuildResource(0);
            ca.CommodityCountChangeHelper(0, rd, -bd.GetBuildCost(0));

            if (bd.BuildingClass == "Worker")
            {
                int currentWorkers = this.Device.Player.WorkerManager.m_vWorkerCount; // e.g., 1
                int baseCost = 250;
                int nextWorkerCost = baseCost * (int)Math.Pow(2, currentWorkers - 1);

                if (!this.Device.Player.Avatar.HasEnoughDiamonds(nextWorkerCost))
                    return;
                this.Device.Player.Avatar.UseDiamonds(nextWorkerCost);
            }
            b.StartConstructing(X, Y);
            this.Device.Player.GameObjectManager.AddGameObject(b);
            Logger.Say("Construction started successfully.");
            
        }


        public int BuildingId;
        public uint Unknown1;
        public int X;
        public int Y;
    }
}
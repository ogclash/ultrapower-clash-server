using System;
using System.Collections.Generic;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 549
    internal class UpgradeMultipleBuildingsCommand : Command
    {
        public UpgradeMultipleBuildingsCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.UpgradeWithEilixir = this.Reader.ReadByte();
            this.m_vBuildingIdList = new List<int>();
            var buildingCount = this.Reader.ReadInt32();
            for (var i = 0; i < buildingCount; i++)
            {
                var buildingId = this.Reader.ReadInt32();
                this.m_vBuildingIdList.Add(buildingId);
            }
            var unknown1 = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;

            foreach (var buildingId in m_vBuildingIdList)
            {
                var b = (Building) this.Device.Player.GameObjectManager.GetGameObjectByID(buildingId);
                if (this.Device.Player.Avatar.m_vTownHallLevel+1 < Convert.ToInt32(b.GetBuildingData().ReqTh[b.UpgradeLevel+1]))
                    continue;
                if (b.CanUpgrade())
                {
                    var bd = b.GetBuildingData();
                    var cost = bd.GetBuildCost(b.GetUpgradeLevel() + 1);
                    if (this.Device.Player.HasFreeWorkers())
                    {
                        b.StartUpgrading();
                        if (UpgradeWithEilixir == 1)
                        {
                            ResourceData elixirLocation = CSVManager.DataTables.GetResourceByName("Elixir");
                            ca.SetResourceCount(elixirLocation, ca.GetResourceCount(elixirLocation) - cost);
                        }
                        else
                        {
                            var rd = bd.GetBuildResource(b.GetUpgradeLevel());
                            ca.SetResourceCount(rd, ca.GetResourceCount(rd) - cost);
                        }
                        Logger.Write("Upgrade with Eilixir: " + buildingId + " cost: " + cost);
                    }
                }
            }
        }
        internal List<int> m_vBuildingIdList;
        internal byte m_vIsAltResource;
        internal uint  UpgradeWithEilixir;
    }
}
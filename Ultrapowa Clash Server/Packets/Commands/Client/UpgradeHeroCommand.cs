using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 527
    internal class UpgradeHeroCommand : Command
    {
        public UpgradeHeroCommand(Reader reader, Device client, int id) : base(reader, client, id)
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
            if (go != null)
            {
                var b = (Building) go;
                var hbc = b.GetHeroBaseComponent();
                if (hbc != null)
                {
                    var hd = CSVManager.DataTables.GetHeroByName(b.GetBuildingData().HeroType);
                    var currentLevel = ca.GetUnitUpgradeLevel(hd);
                    var rd = hd.GetUpgradeResource(currentLevel);
                    var cost = hd.GetUpgradeCost(currentLevel);
                    if (ca.HasEnoughResources(rd, cost))
                    {
                        ca.SetResourceCount(rd, ca.GetResourceCount(rd) - cost);
                        if (this.Device.Player.HasFreeWorkers())
                        {
                            Logger.Write("Hero To Upgrade : " + b.GetData().GetName() + " (" + BuildingId + ')');
                            hbc.StartUpgrading();
                        }
                    }
                    else
                    {
                        if (ca.CheatFlags != null && ca.CheatFlags.Count > 0)
                        {
                            DateTime lastFlag = ca.CheatFlags.Last();
                            if (DateTime.Now - lastFlag < TimeSpan.FromMinutes(30))
                                if (ca.CheatFlags.Count > 10)
                                    ca.AccountBanned = true;
                            else
                                ca.CheatFlags.Clear();
                        }
                        else
                            ca.CheatFlags = new List<DateTime>();
                        ca.CheatFlags.Add(DateTime.Now);
                    }
                }
            }
        }

        public int BuildingId;
        public uint Unknown1;
    }
}
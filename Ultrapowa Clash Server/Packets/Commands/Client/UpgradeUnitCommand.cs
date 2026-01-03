using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 516
    internal class UpgradeUnitCommand : Command
    {
        public UpgradeUnitCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.BuildingId = this.Reader.ReadInt32();
            this.Unknown1 = this.Reader.ReadUInt32();
            this.UnitData = (CombatItemData)this.Reader.ReadDataReference();
            this.Unknown2 = this.Reader.ReadUInt32();
        }
        internal override void Process()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(BuildingId);
            var b = (Building) go;
            var uuc = b.GetUnitUpgradeComponent();
            var unitLevel = ca.GetUnitUpgradeLevel(UnitData);
            if (uuc.CanStartUpgrading(UnitData))
            {
                var cost = UnitData.GetUpgradeCost(unitLevel);
                var rd = UnitData.GetUpgradeResource(unitLevel);
                if (ca.HasEnoughResources(rd, cost))
                {
                    Logger.Write("Unit To Upgrade : " + UnitData.GetName() + " (" + UnitData.GetGlobalID() + ')');
                    ca.SetResourceCount(rd, ca.GetResourceCount(rd) - cost);
                    uuc.StartUpgrading(UnitData);
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

        public int BuildingId;
        public CombatItemData UnitData;
        public uint Unknown1;
        public uint Unknown2;
    }
}
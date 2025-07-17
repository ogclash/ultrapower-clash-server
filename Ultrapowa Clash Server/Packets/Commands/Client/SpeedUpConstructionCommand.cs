using System.Threading;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Core;
using UCS.Files.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 504
    internal class SpeedUpConstructionCommand : Command
    {
        internal int m_vBuildingId;
        internal int Unknown;

        public SpeedUpConstructionCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.m_vBuildingId = this.Reader.ReadInt32();
            this.Unknown = this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(this.m_vBuildingId);
            while (go == null)
            {
                Thread.Sleep(10); // Wait a bit
                go = this.Device.Player.GameObjectManager.GetGameObjectByID(this.m_vBuildingId);
            }
            ((ConstructionItem)go).FinishConstruction();
            ((ConstructionItem)go).SpeedUpConstruction();

            /*if (go != null)
            {
                ((ConstructionItem) go).FinishConstruction();
                if (go.ClassId == 0 || go.ClassId == 4)
                {
                    ((ConstructionItem) go).SpeedUpConstruction();
                }
            }*/
        }

        internal void UnlockBuilding()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId);

            var b = (ConstructionItem) go;

            var bd = (BuildingData) b.GetConstructionItemData();

            if (ca.HasEnoughResources(bd.GetBuildResource(b.GetUpgradeLevel()), bd.GetBuildCost(b.GetUpgradeLevel())))
            {
                string name = this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId).GetData().GetName();
                Logger.Write("Unlocking Building: " + name + " (" + m_vBuildingId + ')');
                if (string.Equals(name, "Alliance Castle"))
                {
                    ca.IncrementAllianceCastleLevel();
                    Building a = (Building)this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId);
                    BuildingData al = a.GetBuildingData();
                    ca.SetAllianceCastleTotalCapacity(al.GetUnitStorageCapacity(ca.GetAllianceCastleLevel()));
                }
                var rd = bd.GetBuildResource(b.GetUpgradeLevel());
                ca.SetResourceCount(rd, ca.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel()));
                b.Unlock();
            }
        }

        internal void StartUpgrade()
        {
            var ca = this.Device.Player.Avatar;
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId);
            if (go != null)
            {
                var b = (ConstructionItem)go;
                if (b.CanUpgrade())
                {
                    var bd = b.GetConstructionItemData();
                    if (ca.HasEnoughResources(bd.GetBuildResource(b.GetUpgradeLevel() + 1),bd.GetBuildCost(b.GetUpgradeLevel() + 1)))
                    {
                        if (this.Device.Player.HasFreeWorkers())
                        {
                            string name = this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId).GetData().GetName();
                            Logger.Write("Building To Upgrade : " + name + " (" + m_vBuildingId + ')');
                            if (string.Equals(name, "Alliance Castle"))
                            {
                                ca.IncrementAllianceCastleLevel();
                                Building a = (Building)this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId);
                                BuildingData al = a.GetBuildingData();
                                ca.SetAllianceCastleTotalCapacity(al.GetUnitStorageCapacity(ca.GetAllianceCastleLevel()));
                            }
                            else if (string.Equals(name, "Town Hall"))
                                ca.IncrementTownHallLevel();

                            var rd = bd.GetBuildResource(b.GetUpgradeLevel() + 1);
                            ca.SetResourceCount(rd, ca.GetResourceCount(rd) - bd.GetBuildCost(b.GetUpgradeLevel() + 1));
                            b.StartUpgrading();
                        }
                    }
                    else
                    {
                        Logger.Write("[Debug] cannot upgrade not enough resources ?");
                    }
                }
                else
                {
                    Logger.Write("[Debug] cannot upgrade for some reason ");
                }
            }
            else
            {
                Logger.Write("[Debug] some how gameobject is equal to null");
            }
        }
    }
}
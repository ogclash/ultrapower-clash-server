using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 517
    internal class SpeedUpUpgradeUnitCommand : Command
    {
        internal int m_vBuildingId;
        internal uint m_vUnitId;
        

        public SpeedUpUpgradeUnitCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.m_vBuildingId = this.Reader.ReadInt32();
            this.m_vUnitId = this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID(m_vBuildingId);
            if (go?.ClassId == 0)
            {
                var b = (Building) go;
                var uuc = b.GetUnitUpgradeComponent();
                if (uuc?.GetCurrentlyUpgradedUnit() != null)
                {
                    uuc.SpeedUp();
                }
            }
        }
    }
}
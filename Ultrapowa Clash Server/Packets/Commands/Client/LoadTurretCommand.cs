using System.Collections.Generic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 525
    internal class LoadTurretCommand : Command
    {
        public LoadTurretCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.m_vUnknown1 = this.Reader.ReadUInt32();
            for (uint i = 0; i < m_vUnknown1; i++)
            {
                buildings.Add(this.Reader.ReadInt32());
            }
            this.m_vUnknown2 = this.Reader.ReadUInt32();
        }

        internal override void Process()
        {
            foreach (int buildingID in buildings)
            {
                var go = this.Device.Player.GameObjectManager.GetGameObjectByID(buildingID);
                if (go?.GetComponent(1, true) != null)
                    ((CombatComponent) go.GetComponent(1, true)).FillAmmo();
            }
        }

        public int m_vBuildingId;
        public uint m_vUnknown1;
        public uint m_vUnknown2;
        public uint m_vUnknown3;
        public List<int> buildings = new List<int>();
    }
}
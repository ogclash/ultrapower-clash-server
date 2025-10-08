using System.Collections.Generic;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 525
    internal class RearmTrapCommand : Command
    {
        public RearmTrapCommand(Reader reader, Device client, int id) : base(reader, client, id)
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
                if ((TriggerComponent)go?.GetComponent(8) != null)
                {
                    Trap t = (Trap)go;
                    TrapData td = (TrapData)go.GetData();
                    ResourceData goldLocation = CSVManager.DataTables.GetResourceByName("Gold");
                    this.Device.Player.Avatar.SetResourceCount(goldLocation, this.Device.Player.Avatar.GetResourceCount(goldLocation)-td.RearmCost[t.UpgradeLevel]);
                    ((TriggerComponent) go.GetComponent(8)).RepairTrap();
                }
            }
        }

        public int m_vBuildingId;
        public uint m_vUnknown1;
        public uint m_vUnknown2;
        public uint m_vUnknown3;
        public List<int> buildings = new List<int>();
    }
}
using System.Threading;
using UCS.Helpers.Binary;
using UCS.Logic;

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
            ((ConstructionItem)go).FinishConstruction(this.m_vBuildingId);
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
    }
}
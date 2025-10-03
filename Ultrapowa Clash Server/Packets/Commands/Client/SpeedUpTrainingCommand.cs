using System.Collections.Generic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 513
    internal class SpeedUpTrainingCommand : Command
    {
        public int m_vBuildingId;
        public bool spells;

        public SpeedUpTrainingCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            m_vBuildingId = this.Reader.ReadInt32();
            spells = this.Reader.ReadBoolean();
            var unknow3 = this.Reader.ReadInt32();
            base.Decode();
        }

        internal override void Process()
        {
            if (spells)
            {
                List<GameObject> buildings = this.Device.Player.GameObjectManager.GetAllGameObjects()[0];
                List<GameObject> factories = new List<GameObject>();
                foreach (GameObject gameObject in buildings)
                {
                    if (gameObject.GetData().GetGlobalID() == 1000020)
                    {
                        factories.Add(gameObject);
                    }
                }
                UnitProductionComponent factory = (UnitProductionComponent)factories[0].GetComponent(3, false);
                factory.SpeedUp();
            }
            else
            {
                UnitProductionComponent barrack = (UnitProductionComponent)this.Device.Player.GameObjectManager.GetGameObjectByID(500000010).GetComponent(3, false);
                barrack.SpeedUp();
            }
        }
    }
}
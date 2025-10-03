using System.Collections.Generic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    internal class BoostBarracksCommand : Command
    {
        public BoostBarracksCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Tick = this.Reader.ReadInt64();
        }

        public long Tick;

        internal override void Process()
        {
            List<GameObject> buildings = this.Device.Player.GameObjectManager.GetAllGameObjects()[0];
            List<GameObject> barracks = new List<GameObject>();
            foreach (GameObject gameObject in buildings)
            {
                if (gameObject.GetData().GetGlobalID() == 1000006)
                {
                    barracks.Add(gameObject);
                }
            }

            foreach (GameObject barrack in barracks)
            {
                Building boost = (Building)barrack;
                if(!boost.IsBoosted)
                {
                    boost.BoostBuilding();
                }
            }
        }
    }
}

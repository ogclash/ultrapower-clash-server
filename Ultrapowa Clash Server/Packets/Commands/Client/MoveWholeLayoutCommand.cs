using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 501
    internal class MoveWholeLayoutCommand : Command
    {
        public MoveWholeLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.Layout = this.Reader.ReadInt32();
            this.Reader.ReadInt32();
        }

        internal override void Process()
        {
            var avatar = this.Device.Player.Avatar;
            var buildings = avatar.getBuildings();
            bool buildingFound = false;
            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (building[1] == Layout)
                {
                    buildings[i][2] += X;
                    buildings[i][3] += Y;
                }
            }
        }

        public int X;
        public int Y;
        public int Layout;
    }
}
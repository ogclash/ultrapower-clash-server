using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 546
    internal class EditVillageLayoutCommand : Command
    {
        internal int X;
        internal int Y;
        internal int BuildingID;
        internal int Layout;

        public EditVillageLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.BuildingID = this.Reader.ReadInt32();
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
                if (building[0] == BuildingID && building[1] == Layout)
                {
                    buildings[i][2] = X;
                    buildings[i][3] = Y;
                    buildingFound = true;
                    break;
                }
            }

            if (!buildingFound)
            {
                buildings.Add(new int[4] { BuildingID, Layout, X, Y });
            }
            
            avatar.setBuidlings(buildings);
        }

    }
}

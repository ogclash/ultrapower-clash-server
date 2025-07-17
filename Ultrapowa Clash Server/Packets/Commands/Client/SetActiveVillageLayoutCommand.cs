using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 567
    internal class SetActiveVillageLayoutCommand : Command
    {
        private int Layout;
        public SetActiveVillageLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            //Layout = br.ReadInt32();
            //Console.WriteLine(br.ReadInt32());
            //Console.WriteLine(br.ReadInt32());
        }
        internal override void Decode()
        {
            //this.Layout = this.Reader.ReadInt32();
            var avatar = this.Device.Player.Avatar;
            var buildings = avatar.getBuildings();
            var Layout0 = this.Reader.ReadInt32();
            bool hasInvalidPosition = false;
            
            foreach (var building in buildings)
            {
                int layout = building[1];
                int x = building[2];
                int y = building[3];

                if (layout == Layout0 && (x == -1 || y == -1))
                {
                    hasInvalidPosition = true;
                    break;
                }
            }
            
            if (!hasInvalidPosition)
            {
                foreach (var building in buildings)
                {
                    int id = building[0];
                    int layout = building[1];
                    int x = building[2];
                    int y = building[3];

                    if (layout == Layout0)
                    {
                        var go = this.Device.Player.GameObjectManager.GetGameObjectByID(id);
                        if (go != null)
                        {
                            go.SetPositionXY(x, y, Layout);
                        }
                    }
                }
                avatar.m_vActiveLayout = Layout0;
            }
        }
    }
}

using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 552
    internal class SaveVillageLayoutCommand : Command
    {
        public SaveVillageLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            var unknown = this.Reader.ReadInt32();
            var unknown1 = this.Reader.ReadInt32();
            var unknown2 = this.Reader.ReadInt32();
            this.Reader.Read();
            var avatar = this.Device.Player.Avatar;
            var buildings = avatar.getBuildings();
            var layout = avatar.m_vActiveLayout;
            if (unknown == layout && unknown2 >= 10000)
            {
                foreach (var building in buildings)
                {
                    if (building[1] == layout)
                    {
                        var go = this.Device.Player.GameObjectManager.GetGameObjectByID(building[0]);
                        go.SetPositionXY(building[2], building[3], unknown);
                    }
                }
            }
            //avatar.m_vActiveLayout = layout;
        }
    }
}

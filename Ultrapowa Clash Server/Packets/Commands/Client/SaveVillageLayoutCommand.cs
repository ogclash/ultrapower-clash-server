using UCS.Helpers.Binary;

namespace UCS.Packets.Commands.Client
{
    // Packet 552
    internal class SaveVillageLayoutCommand : Command
    {
        private int layoutId;
        private int Unknown;
        public SaveVillageLayoutCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.layoutId = this.Reader.ReadInt32();
            var unknown1 = this.Reader.ReadInt32();
            this.Unknown = this.Reader.ReadInt32();
            this.Reader.Read();
        }

        internal override void Process()
        {
            var avatar = this.Device.Player.Avatar;
            var buildings = avatar.getBuildings();
            var layout = avatar.m_vActiveLayout;
            if (this.layoutId == layout && this.Unknown >= 10000)
            {
                foreach (var building in buildings)
                {
                    if (building[1] == layout)
                    {
                        var go = this.Device.Player.GameObjectManager.GetGameObjectByID(building[0]);
                        go.SetPositionXY(building[2], building[3], this.layoutId);
                    }
                }
            }
        }
    }
}

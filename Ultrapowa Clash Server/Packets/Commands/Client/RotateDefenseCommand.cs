using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 554
    internal class RotateDefenseCommand : Command
    {
        public RotateDefenseCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.BuildingID = this.Reader.ReadInt32();
            this.layoutId = Reader.ReadInt32();
            if (Reader.BaseStream.Length-Reader.BaseStream.Position > 0)
                this.Reader.ReadBytes((int)(Reader.BaseStream.Length - Reader.BaseStream.Position));
        }

        internal override void Process()
        {
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID((int)BuildingID);
            if (layoutId == Device.Player.Avatar.m_vActiveLayout && go?.GetComponent(1, true) != null)
            {
                ((CombatComponent) go.GetComponent(1, true)).rotateSweeper();
            }
        }

        public int BuildingID { get; set; }
        public int layoutId { get; set; }

    }
}

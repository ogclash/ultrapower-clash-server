using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 524
    internal class ToggleAttackModeCommand : Command
    {
        public ToggleAttackModeCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            
            BuildingId = reader.ReadUInt32(); //buildingId - 0x1DCD6500;
            Unknown1 = reader.ReadByte();
            Unknown2 = reader.ReadUInt32();
            Unknown3 = reader.ReadUInt32();
            var go = this.Device.Player.GameObjectManager.GetGameObjectByID((int)BuildingId);
            if (go?.GetComponent(1, true) != null)
                ((CombatComponent) go.GetComponent(1, true)).toggleMode();
            /*var go = this.Device.Player.GameObjectManager.GetGameObjectByID((int)BuildingId);
            var test = "test";*/

        }

        public uint BuildingId { get; set; }
        public byte Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
    }
}
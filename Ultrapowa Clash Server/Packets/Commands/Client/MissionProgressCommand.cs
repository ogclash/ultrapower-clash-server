using System.Collections.Generic;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 519
    internal class MissionProgressCommand : Command
    {
        public MissionProgressCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.MissionID = this.Reader.ReadUInt32();
            this.Tick = this.Reader.ReadUInt32();
        }

        public uint MissionID;

        public uint Tick;

        internal override void Process()
        {
            if (this.MissionID == 21000001)
            {
                CombatItemData _TroopData = (CombatItemData)CSVManager.DataTables.GetDataById(4000006);
                List<DataSlot> _PlayerUnits = this.Device.Player.Avatar.GetUnits();
                DataSlot _DataSlot = _PlayerUnits.Find(t => t.Data.GetGlobalID() == _TroopData.GetGlobalID());
                if (_DataSlot != null)
                {
                    _DataSlot.Value += 5;
                }
                else
                {
                    DataSlot ds = new DataSlot(_TroopData, 5);
                    _PlayerUnits.Add(ds);
                }
                this.Device.Player.Avatar.TutorialStepsCount = 3;
            }
        }
    }
}
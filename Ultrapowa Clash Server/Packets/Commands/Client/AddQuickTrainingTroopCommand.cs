using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic;

namespace UCS.Packets.Commands.Client
{
    // Packet 558
    internal class AddQuicKTrainingTroopCommand : Command
    {
        public int Database;
        public int Tick;
        public int TroopType;
        public List<UnitToAdd> UnitsToAdd { get; set; }
        public List<SpellToAdd> SpellsToAdd { get; set; }
        public AddQuicKTrainingTroopCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
            this.UnitsToAdd = new List<UnitToAdd>();
            this.SpellsToAdd = new List<SpellToAdd>();
        }

        internal override void Decode()
        {
            this.Database = this.Reader.ReadInt32();
            this.TroopType = this.Reader.ReadInt32();
            for (int i = 0; i < TroopType; i++)
            {
                var unit_id = this.Reader.ReadInt32();
                var unit_count = this.Reader.ReadInt32();
                try
                {
                    this.UnitsToAdd.Add(new UnitToAdd
                        { Data = (CharacterData)this.Reader.ReadDataReference(unit_id), Count = unit_count });
                }catch (Exception)
                {
                    this.SpellsToAdd.Add(new SpellToAdd()
                        { Data = (SpellData)this.Reader.ReadDataReference(unit_id), Count = unit_count });
                }
            }
            this.Tick = this.Reader.ReadInt32();

        }
        internal override void Process()
        {
            var defaultdatbase = this.Device.Player.Avatar.QuickTrain1;
            switch (Database)
            {
                case 1:
                    break;
                case 2:
                    defaultdatbase = this.Device.Player.Avatar.QuickTrain2;
                    break;
                case 3:
                    defaultdatbase = this.Device.Player.Avatar.QuickTrain3;
                    break;
                default:
                    throw new NullReferenceException("Unknown Database Detected");
            }

            defaultdatbase.Clear();
            defaultdatbase.AddRange(UnitsToAdd.Select(i => new DataSlot(i.Data, i.Count)));
            defaultdatbase.AddRange(SpellsToAdd.Select(i => new DataSlot(i.Data, i.Count)));
        }
        internal class UnitToAdd
        {
            public int Count { get; set; }
            public CharacterData Data { get; set; }
        }
        internal class SpellToAdd
        {
            public int Count { get; set; }
            public SpellData Data { get; set; }
        }
    }
}

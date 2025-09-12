using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Helpers.Binary;
using UCS.Logic.Enums;

namespace UCS.Packets.Commands.Client
{
    // Packet 605
    internal class PlaceHeroCommand : Command
    {
        public PlaceHeroCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }


        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.HeroID = this.Reader.ReadInt32();
            HeroData hero = (HeroData)CSVManager.DataTables.GetDataById(HeroID);
            // add new unit if not found
            JArray unitInfo = new JArray
            {
                HeroID,
                1
            };
            JArray unitLevel = new JArray
            {
                HeroID,
                this.Device.Player.Avatar.GetUnitUpgradeLevel(hero)
            };
            this.Device.Player.Avatar.battle.levels.Add(unitLevel);
            this.Device.Player.Avatar.battle.units.Add(unitInfo);
            this.Tick = this.Reader.ReadInt32();
            this.Device.PlayerState = State.IN_BATTLE;
            if (this.Device.AttackInfo == null)
            {
                this.Device.AttackInfo = "multiplayer";
            }

            if (this.Device.AttackInfo == "npc")
            {
                this.Device.NpcAttacked = true;
            }
        }

        public int X;

        public int Y;

        public int Tick;

        public int HeroID;
    }
}
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
            this.Tick = this.Reader.ReadInt32();
            this.Device.PlayerState = State.IN_BATTLE;
            if (this.Device.AttackInfo == null)
            {
                this.Device.AttackInfo = "multiplayer";
            }
        }

        public int X;

        public int Y;

        public int Tick;

        public int HeroID;
    }
}
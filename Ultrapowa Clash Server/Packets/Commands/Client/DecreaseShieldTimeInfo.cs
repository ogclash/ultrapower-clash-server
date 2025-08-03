using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;


namespace UCS.Packets.Commands.Client
{
    internal class DecreaseShieldTimeInfo : Command
    {
        public DecreaseShieldTimeInfo(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Process()
        {
            this.Device.ShieldInfo = true;
        }
    }
}

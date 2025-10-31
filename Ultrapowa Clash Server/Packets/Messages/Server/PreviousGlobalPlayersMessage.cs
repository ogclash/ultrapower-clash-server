using System;
using System.IO;
using UCS.Core;
using UCS.Helpers.List;

namespace UCS.Packets.Messages.Server
{
    internal class PreviousGlobalPlayersMessage : Message
    {
        public PreviousGlobalPlayersMessage(Device client) : base(client)
        {
            this.Identifier = 24405;
        }

        internal override async void Encode()
        {
            try
            {
                if (!File.Exists(ObjectManager.filePathPrevius))
                    return;
                this.Data.AddInt(Convert.ToInt32(File.ReadAllText(ObjectManager.filePathPreviusNumber)));
                this.Data.AddRange(File.ReadAllBytes(ObjectManager.filePathPrevius));
                this.Data.AddInt(DateTime.Now.Month - 1);
                this.Data.AddInt(DateTime.Now.Year);
            }
            catch (Exception) { }
        }
    }
}

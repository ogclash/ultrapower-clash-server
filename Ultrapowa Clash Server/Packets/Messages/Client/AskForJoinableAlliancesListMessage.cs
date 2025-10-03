using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14303
    internal class AskForJoinableAlliancesListMessage : Message
    {
        const int m_vAllianceLimit = 40;

        public AskForJoinableAlliancesListMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Process()
        {
            List<Alliance> alliances = ObjectManager.GetInMemoryAlliances();
            List<Alliance> joinableAlliances = new List<Alliance>();
            int i = 0;
            int j = 0;
            while (j < m_vAllianceLimit && i < alliances.Count)
            {
                if (alliances[i].GetAllianceMembers().Count != 0 && !alliances[i].IsAllianceFull())
                {
                    joinableAlliances.Add(alliances[i]);
                    j++;
                }
                i++;
            }
            joinableAlliances = joinableAlliances.ToList();

            new JoinableAllianceListMessage(Device) {Alliances = joinableAlliances}.Send();
        }
    }
}

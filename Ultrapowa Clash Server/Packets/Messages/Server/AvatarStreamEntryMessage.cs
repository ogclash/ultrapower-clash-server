using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;

namespace UCS.Packets.Messages.Server
{
    // Packet 24412
    internal class AvatarStreamEntryMessage : Message
    {
        AvatarStreamEntry m_vAvatarStreamEntry;
        Level targetAccount;

        public AvatarStreamEntryMessage(Device client) : base(client)
        {
            this.Identifier = 24412;
        }

        internal override void Encode()
        { 
            this.Data.AddRange(m_vAvatarStreamEntry.Encode());
        }

        public void SetTargetAcc(Level acc)
        {
            targetAccount = acc;
        }

        public void SetAvatarStreamEntry(AvatarStreamEntry entry, bool save = true)
        {
            if (save)
            {
                if (this.Device == null)
                {
                    if (targetAccount != null)
                    {
                        entry.wasOnline = false;
                        targetAccount.Avatar.messages.Add(entry);
                    }
                }
                else
                {
                    if (this.Device.Connected)
                    {
                        entry.wasOnline = true;
                    }
                    this.Device.Player.Avatar.messages.Add(entry);
                }
            }
            m_vAvatarStreamEntry = entry;
        }
    }
}
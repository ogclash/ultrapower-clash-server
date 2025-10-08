using System;
using UCS.Helpers.List;
using UCS.Logic.Enums;

namespace UCS.Packets.Messages.Server
{
    internal class EventMessage : Message
    {
        public EventMessage(Device client) : base(client)
        {
            this.Device.PlayerState = State.LOGGED;
        }

        internal override void Encode()
        {
            int now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Number of events
            this.Data.AddInt(1);

            // --- Halloween event ---
            /*this.Data.AddInt(3); // Event ID
            this.Data.AddInt(now - 86400); // Start timestamp
            this.Data.AddInt(now + 86400 * 7); // End timestamp
            this.Data.AddString("TID_EVENT_3_TITLE"); // Title
            this.Data.AddString("TID_EVENT_3_INFO"); // Info
            this.Data.AddString("sc/characters.sc,wizard1_run1_3"); // Asset
            this.Data.AddString("TID_NEWS_BUTTON_GO"); // Button text
            this.Data.AddString("OpenWeb"); // Button action
            this.Data.AddString("http://www.clashofclans.com"); // Button URL*/

            // --- Christmas event ---
            this.Data.AddInt(1); // Event ID
            this.Data.AddInt(now - 86400); // Start timestamp
            this.Data.AddInt(now + 86400 * 30); // End timestamp
            this.Data.AddString("TID_EVENT_CHRISTMAS"); // Title
            this.Data.AddString("TID_EVENT_CHRISTMAS_INFO"); // Info
            this.Data.AddString("sc/ui.sc,icon_unit_iceWizard"); // Asset
            this.Data.AddString(null); // No button
            this.Data.AddString(null); // No action
            this.Data.AddString(null); // No URL
        }
    }
}
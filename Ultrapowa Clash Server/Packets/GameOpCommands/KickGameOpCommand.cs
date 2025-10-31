using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Core.Settings;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class KickGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public KickGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(2);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Constants.SuperAdmin)
            {
                if (m_vArgs.Length >= 2)
                {
                    try
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (ResourcesManager.IsPlayerOnline(l))
                        {
                            Processor.Send(new OutOfSyncMessage(l.Client));
                        }
                        else
                        {
                            Console.WriteLine("Kick failed: id " + id + " not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Kick failed with error: " + ex);
                    }
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}

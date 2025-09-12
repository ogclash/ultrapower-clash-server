using System;
using UCS.Core;
using UCS.Logic;

namespace UCS.Packets.GameOpCommands
{
    internal class ResetAccGameOpCommand: GameOpCommand
    {
        readonly string[] m_vArgs;

        public ResetAccGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override async void Execute(Level level)
        {
            try {
                if (GetRequiredAccountPrivileges())
                {
                    if (m_vArgs.Length >= 1)
                    {
                        if (level.Avatar.old_account == level.Avatar.UserId)
                        {
                            return;
                        }
                        Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                        Logger.Say("Account with id: "+oldplayer.Avatar.UserId+" was reset");
                        if (oldplayer.Client != null)
                            ResourcesManager.DisconnectClient(oldplayer.Client);
                        ResourcesManager.LoadLevel(oldplayer);
                        oldplayer.Avatar.account_switch = 0;
                        ResourcesManager.DisconnectClient(level.Client);
                    }
                }
                else
                {
                    SendCommandFailedMessage(level.Client);
                }
            } catch (Exception) {}
        }
    }
}

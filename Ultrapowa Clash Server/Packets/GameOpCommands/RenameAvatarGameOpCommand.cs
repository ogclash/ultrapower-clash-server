using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class RenameAvatarGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public RenameAvatarGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }

        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                if (m_vArgs.Length >= 3)
                {
                    try
                    {
                        var id = Convert.ToInt64(m_vArgs[1]);
                        var l = await ResourcesManager.GetPlayer(id);
                        if (l != null)
                        {
                            l.Avatar.SetName(Convert.ToString(m_vArgs[2]));
                            if (ResourcesManager.IsPlayerOnline(l))
                            {
                                var p = new AvatarNameChangeOkMessage(l.Client) {AvatarName = m_vArgs[2]};
                                Processor.Send(p);
                            }
                        }
                    }
                    catch (Exception)
                    {
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

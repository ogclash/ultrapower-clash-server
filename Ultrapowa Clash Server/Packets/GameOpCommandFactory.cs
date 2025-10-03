using System;
using System.Collections.Generic;
using UCS.Packets.GameOpCommands;

namespace UCS.Packets
{
    internal static class GameOpCommandFactory
    {

        static readonly Dictionary<string, Type> m_vCommands;

        static GameOpCommandFactory()
        {
            try {
                m_vCommands = new Dictionary<string, Type>();
                m_vCommands.Add("/ban", typeof(BanGameOpCommand));
                m_vCommands.Add("/kick", typeof(KickGameOpCommand));
                m_vCommands.Add("/rename", typeof(RenameAvatarGameOpCommand));
                m_vCommands.Add("/addgems", typeof(AddGemsOpCommand));
                m_vCommands.Add("/op", typeof(SetPrivilegesGameOpCommand));
                m_vCommands.Add("/shutdown", typeof(ShutdownServerGameOpCommand));
                m_vCommands.Add("/unban", typeof(UnbanGameOpCommand));
                m_vCommands.Add("/visit", typeof(VisitGameOpCommand));
                m_vCommands.Add("/sysmsg", typeof(SystemMessageGameOpCommand));
                m_vCommands.Add("/id", typeof(GetIdGameopCommand));
                m_vCommands.Add("/max", typeof(MaxRessourcesCommand));
                m_vCommands.Add("/min", typeof(MinRessourcesCommand));
                m_vCommands.Add("/maxbase", typeof(MaxBaseGameOpCommand)); // just for testing!
                m_vCommands.Add("/reload", typeof(SaveAccountGameOpCommand));
                m_vCommands.Add("/becomeleader", typeof(BecomeLeaderGameOpCommand));
                m_vCommands.Add("/status", typeof(ServerStatusGameOpCommand));
                m_vCommands.Add("/help", typeof(HelpGameOpCommand));
                m_vCommands.Add("/switchacc", typeof(SwichAccGameOpCommand));
                m_vCommands.Add("/newaccount", typeof(NewAccountGameOpCommand));
                m_vCommands.Add("/setpassword", typeof(SetPasswordGameOpCommand));
                m_vCommands.Add("/resetacc", typeof(ResetAccGameOpCommand));
                m_vCommands.Add("/clearinbox", typeof(ClearInboxGameOpCommand));
                m_vCommands.Add("/clearbattles", typeof(ClearBattleLogGameOpCommand));
                m_vCommands.Add("/accinfo", typeof(AccountInformationGameOpCommand));
            } catch (Exception) {}
        }

        public static object Parse(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return null;

            var commandArgs = command.ToLower().Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            if (commandArgs.Length == 0 || string.IsNullOrWhiteSpace(commandArgs[0]))
                return null;

            if (m_vCommands.ContainsKey(commandArgs[0]))
            {
                var type = m_vCommands[commandArgs[0]];
                var ctor = type.GetConstructor(new[] { typeof(string[]) });
                if (ctor != null)
                {
                    return ctor.Invoke(new object[] { commandArgs });
                }
            }

            return null;
        }


    }
}

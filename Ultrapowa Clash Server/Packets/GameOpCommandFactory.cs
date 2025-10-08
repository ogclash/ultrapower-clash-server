using System;
using System.Collections.Generic;
using UCS.Core.Settings;
using UCS.Packets.GameOpCommands;

namespace UCS.Packets
{
    internal static class GameOpCommandFactory
    {

        static readonly Dictionary<string, Type> m_vCommands;
        static readonly Dictionary<string, Type> m_vCommandsDeveloper;

        static GameOpCommandFactory()
        {
            try {
                m_vCommands = new Dictionary<string, Type>();
                m_vCommands.Add("/rename", typeof(RenameAvatarGameOpCommand));
                m_vCommands.Add("/visit", typeof(VisitGameOpCommand));
                m_vCommands.Add("/id", typeof(GetIdGameopCommand));
                m_vCommands.Add("/help", typeof(HelpGameOpCommand));
                m_vCommands.Add("/switchacc", typeof(SwichAccGameOpCommand));
                m_vCommands.Add("/newaccount", typeof(NewAccountGameOpCommand));
                m_vCommands.Add("/setpassword", typeof(SetPasswordGameOpCommand));
                m_vCommands.Add("/resetacc", typeof(ResetAccGameOpCommand));
                m_vCommands.Add("/clearinbox", typeof(ClearInboxGameOpCommand));
                m_vCommands.Add("/clearbattles", typeof(ClearBattleLogGameOpCommand));
                m_vCommands.Add("/accinfo", typeof(AccountInformationGameOpCommand));
                
                m_vCommandsDeveloper = new Dictionary<string, Type>();
                m_vCommandsDeveloper.Add("/ban", typeof(BanGameOpCommand));
                m_vCommandsDeveloper.Add("/kick", typeof(KickGameOpCommand));
                m_vCommandsDeveloper.Add("/addgems", typeof(AddGemsOpCommand));
                m_vCommandsDeveloper.Add("/op", typeof(SetPrivilegesGameOpCommand));
                m_vCommandsDeveloper.Add("/shutdown", typeof(ShutdownServerGameOpCommand));
                m_vCommandsDeveloper.Add("/unban", typeof(UnbanGameOpCommand));
                m_vCommandsDeveloper.Add("/sysmsg", typeof(SystemMessageGameOpCommand));
                m_vCommandsDeveloper.Add("/max", typeof(MaxRessourcesCommand));
                m_vCommandsDeveloper.Add("/min", typeof(MinRessourcesCommand));
                m_vCommandsDeveloper.Add("/maxbase", typeof(MaxBaseGameOpCommand)); // just for testing!
                m_vCommandsDeveloper.Add("/reload", typeof(SaveAccountGameOpCommand));
                m_vCommandsDeveloper.Add("/becomeleader", typeof(BecomeLeaderGameOpCommand));
                m_vCommandsDeveloper.Add("/status", typeof(ServerStatusGameOpCommand));
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
                return getCommand(m_vCommands[commandArgs[0]], commandArgs);
                
            } 
            if (Constants.DeveloperBuild && m_vCommandsDeveloper.ContainsKey(commandArgs[0]))
            {
                return getCommand(m_vCommandsDeveloper[commandArgs[0]], commandArgs);
            }

            return null;
        }

        private static object getCommand(Type type, string[] commandArgs)
        {
            var ctor = type.GetConstructor(new[] { typeof(string[]) });
            if (ctor != null)
            {
                return ctor.Invoke(new object[] { commandArgs });
            }
            return null;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UCS.Logic;
using UCS.Old.Helpers;

namespace UCS.PacketProcessing
{
    static class GameOpCommandFactory
    {
        private static Dictionary<string, Type> m_vCommands;

        static GameOpCommandFactory()
        {
            m_vCommands = new Dictionary<string, Type>();
        }

        public static object Parse(string command)
        {
            string[] commandArgs = command.Split(' ');
            object result = null;
            if(commandArgs.Length > 0)
            {
                if (m_vCommands.ContainsKey(commandArgs[0]))
                {
                    Type type = m_vCommands[commandArgs[0]];
                    ConstructorInfo ctor = type.GetConstructor(new[] { typeof(string[]) });
                    result = ctor.Invoke(new object[] { commandArgs });
                }
            }
            return result;
        }
    }
}

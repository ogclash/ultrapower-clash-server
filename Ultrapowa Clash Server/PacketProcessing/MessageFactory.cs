using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UCS.Logic;
using UCS.Old.Helpers;
using UCS.Packets;

namespace UCS.PacketProcessing
{
    //Command list: LogicCommand::createCommand
    static class MessageFactory
    {
        private static Dictionary<int, Type> m_vMessages;

        static MessageFactory()
        {
            m_vMessages = new Dictionary<int, Type>();
            m_vMessages.Add(10101, typeof(LoginMessage));
            m_vMessages.Add(10108, typeof(KeepAliveMessage));
            m_vMessages.Add(14102, typeof(ExecuteCommandsMessage));
        }

        public static object Read(Device c, BinaryReader br, int packetType)
        {
            if (m_vMessages.ContainsKey(packetType))
            {
                return Activator.CreateInstance(m_vMessages[packetType], c, br);
            }
            else
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("U");
                Console.ResetColor();
                Console.WriteLine("] " + packetType.ToString() + " Unhandled Message (ignored)");
                return null;
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UCP
{
    internal class Proxy
    {
        public const string hostname = "192.168.56.1";
        public const int port = 9338;
        public static Stopwatch _Stopwatch = new Stopwatch();

        private static void Main()
        {
            int GWL_EXSTYLE = -20;
            int WS_EX_LAYERED = 0x80000;
            uint LWA_ALPHA = 0x2;
            IntPtr Handle = GetConsoleWindow();
            SetWindowLong(Handle, GWL_EXSTYLE, (int)GetWindowLong(Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            SetLayeredWindowAttributes(Handle, 0, 227, LWA_ALPHA);

            Console.Title = "Ultrapowa Clash of Clans Proxy v1.0.1";

            if (!Directory.Exists("Packets"))
            {
                Directory.CreateDirectory("Packets");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                @"
      ____ ___.__   __                                              
     |    |   \  |_/  |_____________  ______   ______  _  _______   
     |    |   /  |\   __\_  __ \__  \ \____ \ /  _ \ \/ \/ /\__  \  
     |    |  /|  |_|  |  |  | \// __ \|  |_> >  <_> )     /  / __ \_
     |______/ |____/__|  |__|  (____  /   __/ \____/ \/\_/  (____  /
                                    \/|__|                       \/");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("                                                            Clash of Clans");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Proxy");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[PROXY]");
            Console.ResetColor();
            Console.WriteLine("    -> This Program is made by the Ultrapowa Network Developer Team!");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[PROXY]");
            Console.ResetColor();
            Console.WriteLine("    -> You can find the source at www.ultrapowa.com.");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[PROXY]");
            Console.ResetColor();
            Console.WriteLine("    -> Don't forget to visit www.ultrapowa.com daily for news update !");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[PROXY]");
            Console.ResetColor();
            Console.WriteLine("    -> Clash of Clans Proxy is now starting...");
            _Stopwatch.Start();

            Console.WriteLine();

            try
            {
                Server server = new Server(port);
                server.StartServer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetConsoleWindow();
    }
}

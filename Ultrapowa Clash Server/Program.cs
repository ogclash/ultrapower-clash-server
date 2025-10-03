using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using UCS.Core;
using UCS.Core.Settings;
using UCS.Core.Web;
using UCS.Helpers;
using static UCS.Core.Logger;

namespace UCS
{
    internal class Program
    {
        internal static int OP = 0;
        internal static string Title = $"Ultrapower Clash Server v{Constants.Version} Build: {Constants.Build} - ©2025 | Online Players: ";
        public static Stopwatch _Stopwatch = new Stopwatch();

        internal static void Main()
        {
            int GWL_EXSTYLE = -20;
            int WS_EX_LAYERED = 0x80000;
            uint LWA_ALPHA = 0x2;
            IntPtr Handle = GetConsoleWindow();
            SetWindowLong(Handle, GWL_EXSTYLE, (int)GetWindowLong(Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);

            if (Utils.ParseConfigBoolean("Animation"))
            {

                new Thread(() =>
                {
                    for (int i = 20; i < 227; i++)
                    {
                        if (i < 100)
                        {
                            SetLayeredWindowAttributes(Handle, 0, (byte)i, LWA_ALPHA);
                            Thread.Sleep(5);
                        }
                        else
                        {
                            SetLayeredWindowAttributes(Handle, 0, (byte)i, LWA_ALPHA);
                            Thread.Sleep(15);
                        }
                    }
                }).Start();
            }
            else
            {
                SetLayeredWindowAttributes(Handle, 0, 227, LWA_ALPHA);
            }

            Console.Title = Title + 0;

            Say();

            Console.ForegroundColor = ConsoleColor.Blue;
            Logger.WriteCenter("            ");
            Logger.WriteCenter(" _   _ _ _                                         ");
            Logger.WriteCenter(" | | | | | |_ _ __ __ _ _ __   _____      _____ _ __ ");
            Logger.WriteCenter(" | | | | | __| '__/ _` | '_ \\ / _ \\ \\ /\\ / / _ \\ '__|");
            Logger.WriteCenter(" | |_| | | |_| | | (_| | |_) | (_) \\ V  V /  __/ |   ");
            Logger.WriteCenter("  \\___/|_|\\__|_|  \\__,_| .__/ \\___/ \\_/\\_/ \\___|_|   ");
            Logger.WriteCenter("                       |_|                           ");
            Logger.WriteCenter("            ");
            Console.ResetColor();
            
            Logger.WriteCenter("+-------------------------------------------------------+");
            Logger.WriteCenter("|  This program is not affiliated to \"Supercell, Oy\"  |");
            Logger.WriteCenter("+-------------------------------------------------------+");

            Say();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("[UCS]  ");

            _Stopwatch.Start();
            Version currentVersion = new Version(Constants.Version);
            Version requiredVersion = new Version(VersionChecker.GetVersionString());
            
            if (currentVersion >= requiredVersion)
            {
                Console.WriteLine($"> UCS is up-to-date: {Constants.Version}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Green;
                Say("Preparing Server...\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"> UCS is not up-to-date! New Version: {requiredVersion}. Aborting...");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            Resources.Initialize();
            ResourcesManager.loadAllResources();
            Say("Resources were successfully loaded");
            Say($"IP Address: {Dns.GetHostByName(Dns.GetHostName()).AddressList[0]}\n");
        }
        

        public static void TitleU()
        {
            Console.Title = Title + Convert.ToString(ResourcesManager.m_vOnlinePlayers.Count);
        }

        public static void TitleD()
        {
            Console.Title = Title + Convert.ToString(ResourcesManager.m_vOnlinePlayers.Count);
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

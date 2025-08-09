using System.Reflection;
using UCS.Helpers;

namespace UCS.Core.Settings
{
    internal class Constants
    {
        public static string Version                 = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string Build                   = "Community";
        
        public static readonly bool UseCacheServer   = Utils.ParseConfigBoolean("CacheServer");

        public const int CleanInterval               = 6000;
        public static int MaxOnlinePlayers           = Utils.ParseConfigInt("MaxOnlinePlayers");

        public static readonly bool DebugMode = Utils.ParseConfigBoolean("DebugMode");

        internal const int SendBuffer = 2048;
        internal const int ReceiveBuffer = 2048;
    }
}

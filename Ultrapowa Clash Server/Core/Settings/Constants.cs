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

        public static readonly int SuperAdmin = Utils.ParseConfigInt("AdminAccount");
        
        public static readonly bool DebugMode = Utils.ParseConfigBoolean("DebugMode");
        public static readonly bool DeveloperBuild = Utils.ParseConfigBoolean("DeveloperBuild");
        public static readonly bool ProxyProtocolV1 = Utils.ParseConfigBoolean("ProxyProtocolV1");

        internal const int SendBuffer = 2048;
        internal const int ReceiveBuffer = 2048;
    }
}

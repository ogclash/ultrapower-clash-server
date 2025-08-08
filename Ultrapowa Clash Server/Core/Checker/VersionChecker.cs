using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using UCS.Core.Settings;
using UCS.Helpers;

namespace UCS.Core.Web
{
    internal class VersionChecker
    {
        public void DownloadUpdater()
        {
            WebClient client = new WebClient();
            client.DownloadFile(Utils.ParseConfigString("VersionUrl")+ "UCS_Updater.dat", @"Tools/Updater.exe");
            Thread.Sleep(1000);
            Process.Start(@"Tools/Updater.exe");
            Environment.Exit(0);
        }

        public static string GetVersionString()
        {
            WebClient client = new WebClient();
            string latest_version = Constants.Version;
            try {
                latest_version = client.DownloadString(Utils.ParseConfigString("VersionUrl")+ "version");
            } catch (Exception) {}
            
            return latest_version;
        }

        public static string LatestSupportedVersion()
        {
            return "8.709.16";
        }
    }
}

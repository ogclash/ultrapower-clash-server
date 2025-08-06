using System;
using System.Diagnostics;
using System.IO.Compression;
using Ionic.Zip;
using Ionic.Zlib;
using System.IO;
using UCS.Core.Threading;
using static UCS.Core.Logger;
using System.Net;
using System.Threading;
using System.Reflection;
using UCS.Core.Settings;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace UCS.Core.Web
{
    internal class VersionChecker
    {
        public static void DownloadUpdater()
        {
            WebClient client = new WebClient();
            client.DownloadFile("https://ucs-up.000webhostapp.com/UCS_Updater.dat", @"Tools/Updater.exe");
            Thread.Sleep(1000);
            Process.Start(@"Tools/Updater.exe");
            Environment.Exit(0);
        }

        public static string GetVersionString()
        {
            try
            {
                string Version = "1.0.0.0";
                return (Version);
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public static string LatestSupportedVersion()
        {
            return "8.709.16";
        }
    }
}

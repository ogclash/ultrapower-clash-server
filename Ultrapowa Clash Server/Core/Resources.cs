using System.Threading.Tasks;
using UCS.Core.Events;

namespace UCS.Core
{
    using System;
    using UCS.Core.Network.TCP;

    internal class Resources
    {
        internal static Random Random;
        internal static Gateway Gateway;
        internal static DatabaseManager DatabaseManager;
        internal static Loader Loader;
        internal static Region Region;
        internal static Task ContentServerTask;
        internal static ContentServer ContentServer;

        internal static void Initialize()
        {
            Loader = new Loader();
            Random = new Random();
            DatabaseManager = new DatabaseManager();
            Region = new Region();
            //ContentServer = new ContentServer(); // create instance
            //ContentServerTask = StartContentServerAsync();
            Gateway = new Gateway();
        }

        internal static async Task StartContentServerAsync()
        {
            if (ContentServer != null)
            {
                await ContentServer.StartAsync();
            }
        }
    }

}

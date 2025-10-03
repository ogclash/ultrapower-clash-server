using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCS.Core;
using UCS.Helpers;

namespace UCS.WebAPI
{
    internal class API
    {
        private static readonly string IP = "localhost"; // Use localhost for testing
        private static int Port = GetPortFromConfig();
        private static readonly string URL = $"http://{IP}:{Port}/";
        private static HttpListener Listener;

        public API()
        {
            new Thread(() =>
            {
                try
                {
                    Logger.Say($"Attempting to start WebAPI with URL: {URL}");

                    if (!HttpListener.IsSupported)
                    {
                        Logger.Say("The current system doesn't support the WebAPI.");
                        return;
                    }

                    if (!Uri.IsWellFormedUriString(URL, UriKind.Absolute))
                    {
                        Logger.Say($"Invalid URL format: {URL}");
                        return;
                    }

                    Listener = new HttpListener();
                    Listener.Prefixes.Add(URL);
                    Listener.Prefixes.Add($"{URL}api/");
                    Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                    Listener.Start();

                    Logger.Say($"The WebAPI has been started on port '{Port}'.");

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        while (Listener.IsListening)
                        {
                            try
                            {
                                var context = Listener.GetContext();
                                ThreadPool.QueueUserWorkItem(c => HandleRequest((HttpListenerContext)c), context);
                            }
                            catch (Exception ex)
                            {
                                Logger.Say($"Error handling request: {ex.Message}");
                            }
                        }
                    });
                }
                catch (HttpListenerException ex)
                {
                    Logger.Say($"Failed to start the WebAPI: {ex.Message} (Error Code: {ex.ErrorCode}). Please check if the port '{Port}' is already in use.");
                }
                catch (Exception ex)
                {
                    Logger.Say($"Unexpected error: {ex.Message}");
                }
            }).Start();
        }

        public static void Stop()
        {
            try
            {
                Listener?.Stop();
                Logger.Say("The WebAPI has been stopped.");
            }
            catch (Exception ex)
            {
                Logger.Say($"Error stopping the WebAPI: {ex.Message}");
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                string responseText;

                if (context.Request.Url.ToString().EndsWith("api/"))
                {
                    responseText = GetjsonAPI();
                }
                else
                {
                    responseText = GetStatisticHTML();
                }

                byte[] responseBuf = Encoding.UTF8.GetBytes(responseText);
                context.Response.ContentLength64 = responseBuf.Length;
                context.Response.OutputStream.Write(responseBuf, 0, responseBuf.Length);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Say($"Error processing request: {ex.Message}");
                context.Response.StatusCode = 500;
                byte[] errorBuf = Encoding.UTF8.GetBytes("Internal Server Error");
                context.Response.ContentLength64 = errorBuf.Length;
                context.Response.OutputStream.Write(errorBuf, 0, errorBuf.Length);
                context.Response.OutputStream.Close();
            }
        }

        private static int GetPortFromConfig()
        {
            try
            {
                return Utils.ParseConfigInt("APIPort");
            }
            catch (Exception ex)
            {
                Logger.Say($"Error reading port from config: {ex.Message}. Using default port 88.");
                return 88; // Default port
            }
        }

        public static string GetStatisticHTML()
        {
            try
            {
                return HTML()
                    .Replace("%ONLINEPLAYERS%", ResourcesManager.m_vOnlinePlayers.Count.ToString())
                    .Replace("%INMEMORYPLAYERS%", ResourcesManager.m_vInMemoryLevels.Count.ToString())
                    .Replace("%INMEMORYALLIANCES%", ResourcesManager.GetInMemoryAlliances().Count.ToString())
                    .Replace("%TOTALCONNECTIONS%", ResourcesManager.GetConnectedClients().Count.ToString());
            }
            catch (Exception ex)
            {
                Logger.Say($"Error generating statistics HTML: {ex.Message}");
                return "The server encountered an internal error or misconfiguration and was unable to complete your request. (500)";
            }
        }

        public static string GetjsonAPI()
        {
            try
            {
                JObject data = new JObject
                {
                    {"online_players", ResourcesManager.m_vOnlinePlayers.Count.ToString()},
                    {"in_mem_players", ResourcesManager.m_vInMemoryLevels.Count.ToString()},
                    {"in_mem_alliances", ResourcesManager.GetInMemoryAlliances().Count.ToString()},
                    {"connected_sockets", ResourcesManager.GetConnectedClients().Count.ToString()},
                    {"all_players", ObjectManager.GetMaxPlayerID()},
                    {"all_clans", ObjectManager.GetMaxAllianceID()}
                };
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Logger.Say($"Error generating JSON API: {ex.Message}");
                return JsonConvert.SerializeObject(new { error = "Internal Server Error" }, Formatting.Indented);
            }
        }

        public static string HTML()
        {
            try
            {
                using (StreamReader sr = new StreamReader("WebAPI/HTML/Statistics.html"))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger.Say($"HTML file not found: {ex.Message}");
                return "File not Found";
            }
            catch (Exception ex)
            {
                Logger.Say($"Error reading HTML file: {ex.Message}");
                return "An error occurred while loading the HTML file.";
            }
        }
    }
}


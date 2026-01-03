using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UCS.Core
{
    public class ContentServer
    {
        private static TcpListener _server;

        public static async Task StartAsync(int port = 9340)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            Console.WriteLine($"ContentServer listening on port {port}...");

            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                var _ = HandleClientAsync(client);
            }
        }

        private static Task HandleClientAsync(TcpClient client)
        {
            return Task.Run(() =>
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream, true);

                    string requestLine = reader.ReadLine();
                    if (!string.IsNullOrEmpty(requestLine))
                    {
                        string[] tokens = requestLine.Split(' ');
                        if (tokens.Length >= 2 && tokens[0] == "GET")
                        {
                            string relativePath = tokens[1].TrimStart('/').Replace("/", "\\");
                            string filePath = Path.Combine("Gamefiles/update", relativePath);

                            Console.WriteLine("Requested file: " + filePath);

                            if (File.Exists(filePath))
                            {
                                byte[] content = File.ReadAllBytes(filePath);
                                string header = "HTTP/1.1 200 OK\r\n" +
                                                "Content-Length: " + content.Length + "\r\n" +
                                                "Content-Type: application/octet-stream\r\n\r\n";
                                byte[] headerBytes = Encoding.ASCII.GetBytes(header);

                                stream.Write(headerBytes, 0, headerBytes.Length);
                                stream.Write(content, 0, content.Length);
                            }
                            else
                            {
                                string notFound = "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\n\r\n";
                                byte[] notFoundBytes = Encoding.ASCII.GetBytes(notFound);
                                stream.Write(notFoundBytes, 0, notFoundBytes.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error handling client: " + ex.Message);
                }
                finally
                {
                    client.Close();
                }
            });
        }


        public static void Stop()
        {
            _server?.Stop();
            Console.WriteLine("ContentServer stopped.");
        }
    }
}
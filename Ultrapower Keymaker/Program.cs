using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;

namespace Ultrapower_Keymaker
{
    class Program
    {
        private static string key;
        private static string FilePath;
        private static string Encrypted_Key;
        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                DisplayBanner();

                Console.WriteLine("Command line options:");
                Console.WriteLine("====================");
                Console.WriteLine("-h | --help       Displays this help menu");
                Console.WriteLine("-g | --generate   Generate a License Key (select a type: lite, pro, ultra)");
                Console.WriteLine("Type 'exit' to close the program.");
                Console.Write("\nEnter your command: ");

                string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

                if (input == "exit")
                {
                    Console.WriteLine("Exiting program. Goodbye!");
                    break;
                }
                else if (input == "-h" || input == "--help" || input == "h")
                {
                    DisplayHelp();
                }
                else if (input == "-g" || input == "--generate" || input == "g")
                {
                    Console.Write("\nSpecify license type (lite, pro, ultra): ");
                    string licenseType = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

                    switch (licenseType)
                    {
                        case "lite":
                        case "l":
                            Console.WriteLine("Generating LITE License key...");
                            GenerateKey("lite");
                            break;
                        case "pro":
                        case "p":
                            Console.WriteLine("Generating PRO License key...");
                            GenerateKey("pro");
                            break;
                        case "ultra":
                        case "u":
                            Console.WriteLine("Generating ULTRA License key...");
                            GenerateKey("ultra");
                            break;
                        default:
                            Console.WriteLine("Invalid license type. Defaulting to LITE License key...");
                            GenerateKey("lite");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Unrecognized option. Please select a valid argument.");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }



        private static void DisplayBanner()
        {
            var arr = new[]
            {
                @" __",
                @"/o \_____",
                @"\__/-==`",
                @"",
                @"Ultrapower Key Maker v0.7.3.2 By Antz"
            };

            foreach (string line in arr)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\nUltrapower Key Maker Console Interface");
            Console.WriteLine("=======================================");
            Console.WriteLine("Usage: Ultrapower Keymaker [arguments]");
            Console.WriteLine("-h | --help       Displays this help menu");
            Console.WriteLine("-g | --generate   Generate a License Key (types: lite, pro, ultra)");
            Console.WriteLine("License Types:");
            Console.WriteLine("[l/lite]   Lite License (default, limits to 350 players, no GUI)");
            Console.WriteLine("[p/pro]    Pro License (limits to 700 players, no GUI)");
            Console.WriteLine("[u/ultra]  Ultra License (no limits)");
        }

        private static void GenerateKey(string input)
        {
            if (input.Contains("l") || input.Contains("lite"))
            {
                Console.WriteLine("Generating LITE License key...");
                key = "1" + RandomString(31);
            }
            else if (input.Contains("p") || input.Contains("pro"))
            {
                Console.WriteLine("Generating PRO License key...");
                key = "2" + RandomString(31);
            }
            else if (input.Contains("u") || input.Contains("ultra"))
            {
                Console.WriteLine("Generating ULTRA License key...");
                key = "3" + RandomString(31);
            }
            else
            {
                Console.WriteLine("No option specified. Generating LITE License key...");
                key = "1" + RandomString(31);
            }

            WriteKeyToFile(key);
            Console.WriteLine($"Key generated: {key}");
            Console.WriteLine("You can either use the key above to authenticate your license or move 'Ky01.lic' into Ultrapower's directory.");
        }

        private static void WriteKeyToFile(string KeyToWrite)
        {
            FilePath = "Ky01.lic";
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            Encrypted_Key = ToHexString(KeyToWrite);
            using (StreamWriter writer = new StreamWriter(FilePath))
            {
                writer.Write(Encrypted_Key);
            }
        }

        private static string ToHexString(string str)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}

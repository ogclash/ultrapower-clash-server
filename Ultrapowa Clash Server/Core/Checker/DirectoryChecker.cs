using System.Collections.Generic;
using System.IO;

namespace UCS.Core.Checker
{
    internal class DirectoryChecker
    {
        public DirectoryChecker()
        {
            Directorys();
            Files();
            LoadFilter();
        }

        public static List<string> badwords = new List<string>();

        public static void LoadFilter()
        {
            if (File.Exists("filter.ucs"))
            {
                StreamReader sr = new StreamReader(@"filter.ucs");
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    badwords.Add(line);
                }
            }
        }

        public static void Directorys()
        {
            string[] Dirs = 
            {
                "Logs",
                "Patch",
                "Tools",
                "Library",
                "Gamefiles",
                "Gamefiles/update"
            };

            foreach (string Dir in Dirs)
            {
                if (!Directory.Exists(@Dir))
                {
                    Directory.CreateDirectory(@Dir);
                }
            }
        }

        public static void Files()
        {
			if (!File.Exists("filter.ucs"))
            {
                using (var s = new StreamWriter("filter.ucs"))
                {
                    s.WriteLine("./savegame");
                }
            }
        }
    }
}

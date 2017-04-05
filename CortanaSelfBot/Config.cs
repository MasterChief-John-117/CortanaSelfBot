using System;
using System.IO;

namespace CortanaSelfBot
{
    public class Config
    {
        public string Token()
        {
            return File.ReadAllLines("config.txt")[0].Substring(6).Trim().Trim('"').Trim();
        }
        public string Prefix()
        {
            return File.ReadAllLines("config.txt")[1].Substring(7).TrimStart().Trim('"').TrimStart();
        }

        public int CacheSize()
        {
            Console.WriteLine(Convert.ToInt32(File.ReadAllLines("config.txt")[2].Substring(10).Trim().Trim('"').Trim()));
            return Convert.ToInt32(File.ReadAllLines("config.txt")[2].Substring(10).Trim().Trim('"').Trim());
        }
    }
}

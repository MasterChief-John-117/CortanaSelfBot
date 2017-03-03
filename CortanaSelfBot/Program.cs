
using System.IO;

namespace CortanaSelfBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("short.json"))
            {
                File.Create("short.json");
            }
            var mainBot = new MainBot();
        }
    }
}

using System.IO;

namespace CortanaSelfBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("notes.json"))
            {
                File.Create("notes.json");
            }
            var mainBot = new MainBot();
        }
    }
}

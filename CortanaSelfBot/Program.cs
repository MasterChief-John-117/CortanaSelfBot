
using System.IO;
using Discord.Commands.Permissions.Userlist;

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
            while (true)
            {
                var mainBot = new MainBot();
            }
        }
    }
}

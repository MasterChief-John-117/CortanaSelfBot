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
            return File.ReadAllLines("config.txt")[1].Substring(7).TrimStart().TrimStart('"').TrimStart();
        }

    }
}

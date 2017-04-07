using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace CortanaSelfBot
{
    public class FindUser
    {
        public static Stopwatch Stopwatch = new Stopwatch();
        public static Dictionary<ulong, string> allUs = new Dictionary<ulong, string>();

        public static string Global(string name, CommandEventArgs e)
        {

            Stopwatch.Restart();
            IEnumerable<User> users = e.Message.Client.Servers.SelectMany(s => s.Users).OrderBy(s => s.Id);
            int count = 0;
            string people = "";
            LinkedList<string> names = new LinkedList<string>();
            string rgx = name;
            LinkedList<ulong> ids = new LinkedList<ulong>();
            foreach (User user in users)
            {
                if (!String.IsNullOrEmpty(user.Nickname) && Regex.IsMatch(user.Nickname, rgx, RegexOptions.IgnoreCase))
                {
                    count++;
                    if (count < 25 && !ids.Contains(user.Id))
                    {
                        ids.AddFirst(user.Id);
                        people += user.Name + " (" + user.Nickname + ") : `" + user.Id.ToString() + "`\n";
                    }
                }
                if (!String.IsNullOrEmpty(user.Name) && Regex.IsMatch(user.Name, rgx, RegexOptions.IgnoreCase))
                {
                    count++;
                    if (count < 25 && !ids.Contains(user.Id))
                    {
                        ids.AddFirst(user.Id);
                        people += user.Name + ": `" + user.Id.ToString() + "`\n";
                    }
                }
            }
            if (count > 20)
            {
                Stopwatch.Stop();
                return
                    $"I found `{count}` users! Please use more strict parameters.. :sweat_smile: (Search took `{Stopwatch.ElapsedMilliseconds}`ms)";
            }
            else if (count >= 1 && count <= 20)
            {
                Stopwatch.Stop();
                return $"{people}(Search took `{Stopwatch.ElapsedMilliseconds}`ms)";
            }
            else
            {
                Stopwatch.Stop();
                return $"It took me `{Stopwatch.ElapsedMilliseconds}`ms to find no one :(";
            }
        }
    }
}

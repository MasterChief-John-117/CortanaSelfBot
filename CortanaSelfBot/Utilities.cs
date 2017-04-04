using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;

namespace CortanaSelfBot
{
    public class Utilities
    {

        public LinkedList<User> GetUsers(CommandEventArgs e)
        {
            LinkedList<User> users = new LinkedList<User>();
            string msg = e.Message.Text;

            foreach (var u in e.Message.MentionedUsers)
            {
                users.AddFirst(u);
            }

            try
            {
                IEnumerable<User> allusers = e.Message.Client.Servers.SelectMany(u => u.Users);
                MatchCollection matches = Regex.Matches(msg, "[0-9]{18}");
                foreach (var match in matches)
                {
                    foreach (User u in allusers) if (u.Id.ToString() == match.ToString() && !users.Contains(u)) users.AddFirst(u);
                }
            }
            catch (Exception exception)
            {
            }

            return users;
        }
    }
}

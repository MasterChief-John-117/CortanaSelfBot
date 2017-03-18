using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace CortanaSelfBot
{
  public class FindUser
  {
    public static Stopwatch Stopwatch = new Stopwatch();
    public static Dictionary<ulong, string> allUs = new Dictionary<ulong, string>();

    public static string Global(string name, CommandEventArgs e)
    {
      FindUser.Stopwatch.Restart();
      IEnumerable<User> users = (IEnumerable<User>) e.Message.Client.Servers.SelectMany<Server, User>((Func<Server, IEnumerable<User>>) (s => s.Users)).OrderBy<User, ulong>((Func<User, ulong>) (s => s.Id));
      int num = 0;
      string str = "";
      LinkedList<string> linkedList1 = new LinkedList<string>();
      string pattern = name;
      LinkedList<ulong> linkedList2 = new LinkedList<ulong>();
      foreach (User user in users)
      {
        if (!string.IsNullOrEmpty(user.Nickname) && Regex.IsMatch(user.Nickname, pattern, RegexOptions.IgnoreCase))
        {
          Console.WriteLine(user.Name);
          ++num;
          if (num < 25 && !linkedList1.Contains(user.Name))
          {
            linkedList1.AddFirst(user.Name);
            str = str + user.Name + " (" + user.Nickname + ") : `" + user.Id.ToString() + "`\n";
          }
          linkedList2.AddFirst(user.Id);
        }
        if (!string.IsNullOrEmpty(user.Name) && Regex.IsMatch(user.Name, pattern, RegexOptions.IgnoreCase))
        {
          Console.WriteLine(user.Name);
          ++num;
          if (num < 25 && !linkedList1.Contains(user.Name))
          {
            linkedList1.AddFirst(user.Name);
            str = str + user.Name + ": `" + user.Id.ToString() + "`\n";
          }
          linkedList2.AddFirst(user.Id);
        }
      }
      if (num > 20)
      {
        FindUser.Stopwatch.Stop();
        return string.Format("I found `{0}` users! Please use more strict parameters.. :sweat_smile: (Search took `{1}`ms)", (object) num, (object) FindUser.Stopwatch.ElapsedMilliseconds);
      }
      if (num >= 1 && num <= 20)
      {
        FindUser.Stopwatch.Stop();
        return string.Format("{0}(Search took `{1}`ms)", (object) str, (object) FindUser.Stopwatch.ElapsedMilliseconds);
      }
      FindUser.Stopwatch.Stop();
      return string.Format("It took me `{0}`ms to find no one :(", (object) FindUser.Stopwatch.ElapsedMilliseconds);
    }
  }
}

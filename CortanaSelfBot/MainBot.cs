using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CortanaSelfBot
{
  public class MainBot
  {
    private static Random rand = new Random();
    public DiscordClient Discord;
    public string AwayReason;
    public bool Isaway;
    private Dictionary<string, string> notes;
    private Dictionary<string, string> shortstring;

    public MainBot()
    {
      this.notes = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText("notes.json"));
      this.shortstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText("short.json"));
      try
      {
        foreach (KeyValuePair<string, string> note in this.notes)
          Console.WriteLine(note.Key + " : " + note.Value);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      try
      {
        foreach (KeyValuePair<string, string> keyValuePair in this.shortstring)
          Console.WriteLine(keyValuePair.Key + " : " + keyValuePair.Value);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      this.Discord = new DiscordClient((Action<DiscordConfigBuilder>) (x =>
      {
        x.LogLevel = LogSeverity.Error;
        x.LogHandler = new EventHandler<LogMessageEventArgs>(this.Log);
      }));
      this.Discord.UsingCommands((Action<CommandServiceConfigBuilder>) (x =>
      {
        x.PrefixChar = "\\";
        x.AllowMentionPrefix = true;
        x.HelpMode = HelpMode.Public;
        x.IsSelfBot = true;
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("ping").Description("Just used to check if the bot is up").Do((Func<CommandEventArgs, Task>) (async e =>
      {
        TimeSpan ptime = e.Message.Timestamp.ToUniversalTime() - DateTime.UtcNow;
        await e.Message.Edit(string.Format("Pong! It took `{0}`ms for the command to reach me", (object) ptime.TotalMilliseconds));
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("ban").Description("Uploads the VeraBan image").Do((Func<CommandEventArgs, Task>) (async e =>
      {
        await e.Message.Delete();
        Message message = await e.Channel.SendFile("img\\ban.png");
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("img").Description("Uploads an image given the name from bot's img folder").Parameter("name", ParameterType.Required).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        await e.Message.Delete();
        if (System.IO.File.Exists(string.Format("img\\{0}", (object) e.GetArg("name"))))
        {
          Message message = await e.Channel.SendFile(string.Format("img\\{0}", (object) e.GetArg("name")));
        }
        else
        {
          await e.Message.Edit("Sorry, couldn't find any image named " + e.GetArg("name"));
          await Task.Delay(TimeSpan.FromSeconds(2.0));
          await e.Message.Delete();
        }
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("game").Description("Changes current game. Note: Not visible to client").Alias("setgame").Parameter("game", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        if (!string.IsNullOrEmpty(e.GetArg("game")))
        {
          this.Discord.SetGame(e.GetArg("game"));
          await e.Message.Edit(string.Format("Playing status set to `{0}`", (object) e.GetArg("game")));
        }
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("clear").Alias("c").Description("Clears `n` messages, limits to and defaults to 100").Parameter("number", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        int count = !string.IsNullOrEmpty(e.GetArg("number")) ? Convert.ToInt32(e.GetArg("number")) : 99;
        int deleted = 0;
        IEnumerable<Message> cachedMsgs = e.Channel.Messages;
        IEnumerable<Message> msgs;
        if (cachedMsgs.Count<Message>() < count)
        {
          Message[] messageArray = await e.Channel.DownloadMessages(count + 1, new ulong?(), Relative.Before, true);
          msgs = (IEnumerable<Message>) messageArray;
          messageArray = (Message[]) null;
        }
        else
          msgs = e.Channel.Messages.OrderByDescending<Message, DateTime>((Func<Message, DateTime>) (x => x.Timestamp)).Take<Message>(count + 1);
        foreach (Message message1 in msgs)
        {
          Message msg = message1;
          if (msg.IsAuthor)
          {
            await msg.Delete();
            ++deleted;
          }
          msg = (Message) null;
        }
        Message message = await e.Channel.SendMessage(string.Format("Deleted `{0}` messages", (object) (deleted - 1)));
        Message msgsnt = message;
        message = (Message) null;
        await Task.Delay(TimeSpan.FromSeconds(2.0));
        await msgsnt.Delete();
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("nuke").Description("Removes **ALL** messages up to and defaulting to 100, if user has `Manage Messages`").Parameter("number", ParameterType.Optional).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        ServerPermissions serverPermissions = e.User.ServerPermissions;
        if (serverPermissions.ManageMessages)
        {
          int deleted = 0;
          IEnumerable<Message> cachedMsgs = e.Channel.Messages;
          int count = 0;
          IEnumerable<Message> msgs;
          if (e.GetArg("number") == "total")
          {
            Message[] messageArray1 = await e.Channel.DownloadMessages(100, new ulong?(), Relative.Before, true);
            msgs = (IEnumerable<Message>) messageArray1;
            messageArray1 = (Message[]) null;
            while (msgs.Count<Message>() > 1)
            {
              cachedMsgs = e.Channel.Messages;
              foreach (Message message1 in msgs)
              {
                Message msg = message1;
                await msg.Delete();
                ++deleted;
                msg = (Message) null;
              }
              Message[] messageArray2 = await e.Channel.DownloadMessages(100, new ulong?(), Relative.Before, true);
              msgs = (IEnumerable<Message>) messageArray2;
              messageArray2 = (Message[]) null;
            }
          }
          else
            count = !string.IsNullOrEmpty(e.GetArg("number")) ? Convert.ToInt32(e.GetArg("number")) : 99;
          deleted = 0;
          cachedMsgs = e.Channel.Messages;
          if (cachedMsgs.Count<Message>() < count)
          {
            Message[] messageArray = await e.Channel.DownloadMessages(count + 1, new ulong?(), Relative.Before, true);
            msgs = (IEnumerable<Message>) messageArray;
            messageArray = (Message[]) null;
          }
          else
            msgs = e.Channel.Messages.OrderByDescending<Message, DateTime>((Func<Message, DateTime>) (x => x.Timestamp)).Take<Message>(count + 1);
          foreach (Message message1 in msgs)
          {
            Message msg = message1;
            await msg.Delete();
            ++deleted;
            msg = (Message) null;
          }
          Message message = await e.Channel.SendMessage(string.Format("Deleted `{0}` messages", (object) (deleted - 1)));
          Message msgsnt = message;
          message = (Message) null;
          await Task.Delay(TimeSpan.FromSeconds(2.0));
          await msgsnt.Delete();
          msgs = (IEnumerable<Message>) null;
          cachedMsgs = (IEnumerable<Message>) null;
          msgsnt = (Message) null;
        }
        else
        {
          serverPermissions = e.User.ServerPermissions;
          if (!serverPermissions.ManageMessages)
            await e.Message.Edit("Sorry, I don't have permissions to delete messages here");
        }
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("away").Description("Sets the game as away and changes the status to Idle").Parameter("reason", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        string reason = e.GetArg("reason");
        if (!this.Isaway)
        {
          this.Isaway = true;
          this.Discord.SetStatus(UserStatus.Idle);
          this.Discord.SetGame(reason);
          await e.Message.Edit(string.Format("You have been set to away with the reason `{0}`", (object) this.AwayReason));
        }
        else if (this.Isaway || string.IsNullOrEmpty(reason))
        {
          this.Isaway = false;
          this.Discord.SetStatus(UserStatus.Online);
          this.Discord.SetGame((string) null);
          await e.Message.Edit("Your away status has been cleared.");
        }
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("emoji").Alias("e").Description("Converts input text to emoji").Parameter("text", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        string message = "";
        char[] input = e.GetArg("text").ToCharArray();
        char[] chArray = input;
        for (int index = 0; index < chArray.Length; ++index)
        {
          char letter = chArray[index];
          if (char.IsPunctuation(letter))
            message += string.Format("**{0}**", (object) letter);
          else if (char.IsWhiteSpace(letter))
            message += "  ";
          else if (char.IsLetter(letter))
            message += string.Format(":regional_indicator_{0}:", (object) letter.ToString().ToLower());
          else if (char.IsDigit(letter))
            message += this.ToWord(letter);
        }
        chArray = (char[]) null;
        await e.Message.Edit(message);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("find").Alias("f").Description("Find a user's ID from name or nick").Parameter("name", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        string name = e.GetArg("name");
        string message = FindUser.Global(name, e);
        await e.Message.Edit(message);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("user").Description("Returns detailed info about a user from their ID").Alias("u").Parameter("id", ParameterType.Required).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        ulong id = Convert.ToUInt64(e.GetArg("id"));
        string allroles = "";
        User user = this.Discord.Servers.SelectMany<Server, User>((Func<Server, IEnumerable<User>>) (s => s.Users)).FirstOrDefault<User>((Func<User, bool>) (s => (long) s.Id == (long) id));
        bool onServer = e.Server.FindUsers(user.Name, false).Any<User>();
        if (onServer)
        {
          IEnumerable<Role> roles = e.Server.Users.FirstOrDefault<User>((Func<User, bool>) (u => (long) u.Id == (long) id)).Roles;
          foreach (Role role1 in roles)
          {
            Role role = role1;
            allroles = allroles + role.Name + ", ";
            role = (Role) null;
          }
          allroles = allroles.Substring(0, allroles.Length - 13);
          roles = (IEnumerable<Role>) null;
        }
        else
          allroles = "Not On Server";
        TimeSpan userCreatedAt = TimeSpan.FromMilliseconds((double) (ulong) ((long) (id / 4194304UL) + 1420070400000L + 62135596800000L));
        DateTime zero = new DateTime(1, 1, 1, 0, 0, 0, 0);
        DateTime newUser = zero + userCreatedAt;
        string discrim = user.Discriminator.ToString();
        while (discrim.Length < 4)
          discrim = "0" + discrim;
        string message = string.Format("```UserName: {0}#{1}\n", (object) user.Name, (object) discrim);
        message += string.Format("User ID: {0}\n", (object) user.Id);
        message += string.Format("User Created: {0}\n", (object) newUser);
        message += string.Format("User is Bot?: {0}\n", (object) user.IsBot);
        message += string.Format("User Status: {0}\n", (object) user.Status.Value);
        if (e.Server.FindUsers(user.Name, false).Any<User>())
          message += string.Format("User Joined at: {0}\n", (object) user.JoinedAt);
        message += string.Format("User Roles: {0}\n", (object) allroles);
        if (this.notes.ContainsKey(user.Id.ToString()))
          message += string.Format("User Notes: {0}\n", (object) this.notes[user.Id.ToString()].Trim());
        if (message.Length < 1900)
          message += string.Format("User Avatar: {0}", (object) user.AvatarUrl);
        if (message.Length > 1995)
          message = message.Substring(0, 1995);
        message += "\n```";
        await e.Message.Edit(message);
        UserData.export(user);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("server").Do((Func<CommandEventArgs, Task>) (async e =>
      {
        Server srv = e.Server;
        string message = "```\n";
        bool features = srv.Features.Any<string>();
        string discrim = srv.Owner.Discriminator.ToString();
        while (discrim.Length < 4)
          discrim = "0" + discrim;
        TimeSpan srvCreatedAt = TimeSpan.FromMilliseconds((double) (ulong) ((long) (srv.Id / 4194304UL) + 1420070400000L + 62135596800000L));
        DateTime zero = new DateTime(1, 1, 1, 0, 0, 0, 0);
        DateTime ServerDate = zero + srvCreatedAt;
        message += string.Format("Server Name: {0}({1})\n", (object) srv.Name, (object) srv.Id);
        message += string.Format("Server Owner: {0}#{1}({2})\n", (object) srv.Owner.Name, (object) discrim, (object) srv.Owner.Id);
        message += string.Format("Server Created: {0}\n", (object) ServerDate);
        message += string.Format("Server Roles: {0}\n", (object) srv.RoleCount);
        message += string.Format("Server Members: {0}\n", (object) srv.UserCount);
        message += string.Format("Server Text Channels: {0}\n", (object) srv.TextChannels.Count<Channel>());
        message += string.Format("Server Voice Channels: {0}\n", (object) srv.VoiceChannels.Count<Channel>());
        message += string.Format("Server Location: {0}\n", (object) srv.Region.Name);
        message += string.Format("Server Partnered: {0}\n", (object) features);
        message += "```";
        await e.Message.Edit(message);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateGroup("note", (Action<CommandGroupBuilder>) (cgb =>
      {
        cgb.CreateCommand("add").Parameter("uid", ParameterType.Required).Parameter("note", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
        {
          string uid = e.GetArg("uid");
          Console.WriteLine(uid);
          string note = e.GetArg("note");
          if (this.notes.ContainsKey(uid))
            note = note + " : " + this.notes[uid];
          this.notes.Remove(uid);
          Console.WriteLine(note);
          Console.WriteLine("Trying to add");
          try
          {
            this.notes.Add(uid, note);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
          Console.WriteLine("added");
          System.IO.File.WriteAllText("notes.json", JsonConvert.SerializeObject((object) this.notes, Formatting.Indented));
          await e.Message.Edit("Note Successfully added");
          CommandLogger.log(e);
        }));
        cgb.CreateCommand("remove").Parameter("uid", ParameterType.Required).Do((Func<CommandEventArgs, Task>) (async e =>
        {
          string uid = e.GetArg("uid");
          Console.WriteLine(uid);
          if (this.notes.ContainsKey(uid))
            this.notes.Remove(uid);
          System.IO.File.WriteAllText("notes.json", JsonConvert.SerializeObject((object) this.notes, Formatting.Indented));
          await e.Message.Edit("Note Successfully removed");
          CommandLogger.log(e);
        }));
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("eyes").Parameter("times", ParameterType.Required).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        int times = Convert.ToInt32(e.GetArg("times"));
        for (int i = times; i > 0; --i)
        {
          await e.Message.Edit("http://i.imgur.com/ySGyNE7.png");
          await Task.Delay(TimeSpan.FromSeconds(1.0));
          await e.Message.Edit("http://i.imgur.com/BFn1JJJ.png");
          await Task.Delay(TimeSpan.FromSeconds(1.0));
        }
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("short").Alias("s").Parameter("req", ParameterType.Required).Parameter("name", ParameterType.Optional).Parameter("input", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        if (e.GetArg("req").ToLower().Equals("add"))
        {
          this.shortstring.Add(e.GetArg("name").ToLower(), e.GetArg("input"));
          Console.WriteLine(string.Format("added {0}, {1}", (object) e.GetArg("name"), (object) e.GetArg("input")));
          System.IO.File.WriteAllText("short.json", JsonConvert.SerializeObject((object) this.shortstring, Formatting.Indented));
          await e.Message.Edit("Done adding");
        }
        if (e.GetArg("req").ToLower().Equals("remove"))
        {
          this.shortstring.Remove(e.GetArg("name"));
          Console.WriteLine(string.Format("removed {0}, {1}", (object) e.GetArg("name"), (object) e.GetArg("input")));
          System.IO.File.WriteAllText("short.json", JsonConvert.SerializeObject((object) this.shortstring, Formatting.Indented));
          await e.Message.Edit("Removed :frowning:");
        }
        if (e.GetArg("req").ToLower().Equals("list"))
        {
          string message = "```\n";
          foreach (string key1 in this.shortstring.Keys)
          {
            string key = key1;
            message = message + key + "\n";
            key = (string) null;
          }
          Dictionary<string, string>.KeyCollection.Enumerator enumerator = new Dictionary<string, string>.KeyCollection.Enumerator();
          message += "```";
          await e.Message.Edit(message);
          message = (string) null;
        }
        if (this.shortstring.ContainsKey(e.GetArg("req").ToLower()))
          await e.Message.Edit(this.shortstring[e.GetArg("req").ToLower()]);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("max").Parameter("msg", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        await e.Message.Delete();
        string msg = e.GetArg("msg");
        while (msg.Length < 1999)
          msg += msg.Substring(msg.Length - 1);
        Message message = await e.Channel.SendMessage(msg);
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("del").Parameter("time", ParameterType.Required).Parameter("message", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        await e.Message.Delete();
        e.GetArg("messgage");
        Message message = await e.Message.Channel.SendMessage("beep");
        message = (Message) null;
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("restart").Do((Func<CommandEventArgs, Task>) (async e =>
      {
        await e.Message.Delete();
        Process.Start("CortanaSelfBot.exe");
        await this.Discord.Disconnect();
        CommandLogger.log(e);
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("getcommand").Parameter("cmdId", ParameterType.Required).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        CommandLogger.log(e);
        int num = 0;
        object obj;
        try
        {
          await e.Message.Edit(CommandLogger.toString(e.GetArg("cmdId")));
        }
        catch (Exception ex)
        {
          obj = (object) ex;
          num = 1;
        }
        if (num == 1)
        {
          Exception ex = (Exception) obj;
          await e.Message.Edit(ex.Message + "\n" + ex.StackTrace);
          ex = (Exception) null;
        }
        obj = (object) null;
      }));
      this.Discord.GetService<CommandService>(true).CreateCommand("role").Parameter("name", ParameterType.Unparsed).Do((Func<CommandEventArgs, Task>) (async e =>
      {
        Role role = e.Server.FindRoles(e.GetArg("name"), false).FirstOrDefault<Role>();
        await e.Message.Edit(string.Format("Provided role `{0}` best matches `{1}`(`{2}`)", (object) e.GetArg("name"), (object) role.Name, (object) role.Id));
        CommandLogger.log(e);
      }));
      this.Discord.ExecuteAndWait((Func<Task>) (async () => await this.Discord.Connect(System.IO.File.ReadAllText("token.txt"), Discord.TokenType.User)));
    }

    public void Log(object sender, LogMessageEventArgs e)
    {
      Console.WriteLine(DateTime.Now.ToString() + ": " + e.Message);
    }

    public string ToWord(char letter)
    {
      if ((int) letter == 48)
        return ":zero:";
      if ((int) letter == 49)
        return ":one:";
      if ((int) letter == 50)
        return ":two:";
      if ((int) letter == 51)
        return ":three:";
      if ((int) letter == 52)
        return ":four:";
      if ((int) letter == 53)
        return ":five:";
      if ((int) letter == 54)
        return ":six:";
      if ((int) letter == 55)
        return ":seven:";
      if ((int) letter == 56)
        return ":eight:";
      return (int) letter == 57 ? ":nine:" : "";
    }
  }
}

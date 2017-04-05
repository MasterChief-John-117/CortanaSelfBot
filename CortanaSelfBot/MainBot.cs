using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Security;
using Discord;
using Discord.Commands;
using Message = Discord.Message;
using ParameterType = Discord.Commands.ParameterType;
using Newtonsoft.Json;


namespace CortanaSelfBot
{
    public class MainBot
    {
        public DiscordClient Discord;
        public String AwayReason;
        public bool Isaway;
        private  static Random rand = new Random();

        Dictionary<string, string> notes;
        Dictionary<string, string> shortstring;


        Utilities util = new Utilities();
        Config config = new Config();


        private string[] logServers = File.ReadAllLines("logServers.txt");
        public Dictionary<ulong, Discord.Message> messageCache;

        public MainBot()
        {
            notes = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("notes.json"));
            shortstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("short.json"));

            messageCache = new Dictionary<ulong, Message>();
            int cachesize = config.CacheSize();


            Discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Verbose;
                x.LogHandler = Log;
            });

            Discord.UsingCommands(x =>
            {
                x.PrefixChar = config.Prefix();
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.IsSelfBot = true;
            });


            Discord.GetService<CommandService>().CreateCommand("ping")
                .Description("Just used to check if the bot is up")
                .Do(async (e) =>
                {
                    TimeSpan ptime = e.Message.Timestamp.ToUniversalTime() - DateTime.UtcNow;
                    /*var then = e.Message.Timestamp.ToUniversalTime();
                    var now = DateTime.Now;
                    var ptime = then - now;
                    double time = ptime.TotalMilliseconds;
                    await e.Channel.SendMessage($"Message time: `{then}`, Time Now: `{now}`");*/
                    await e.Message.Edit($"Pong! It took `{ptime.TotalMilliseconds}`ms for the command to reach me");
                });
            /*Discord.GetService<CommandService>().CreateCommand("ban")
            .Description("Uploads the VeraBan image")
            .Do(async (e) =>
                {
                    await e.Message.Delete();
                    await e.Channel.SendFile("img\\ban.png");
                });*/
            Discord.GetService<CommandService>().CreateCommand("img")
                .Alias("image")
                .Description("Tries to upload an image given the name from bot's img folder")
                .Parameter("name")
                .Do(async (e) =>
                {
                    await e.Message.Delete();
                    if (File.Exists($"img\\{e.GetArg("name")}"))
                    {
                        await e.Channel.SendFile($"img\\{e.GetArg("name")}");
                    }
                    else
                    {
                        await e.Message.Edit("Sorry, couldn't find any image named " + e.GetArg("name"));
                        await Task.Delay(2000);
                        await e.Message.Delete();
                    }
                });
            Discord.GetService<CommandService>().CreateCommand("game")
                .Description("Changes current game. Note: Not visible to client")
                .Alias("setgame")
                .Parameter("game", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    if (!String.IsNullOrEmpty(e.GetArg("game")))
                    {
                        Discord.SetGame(e.GetArg("game"));
                        await e.Message.Edit($"Playing status set to `{e.GetArg("game")}`");
                    }
                });
            Discord.GetService<CommandService>().CreateCommand("clear")
                .Alias("c")
                .Description("Clears `n` messages, limited to and defaulting to 100")
                .Parameter("number", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    int count;
                    if (String.IsNullOrEmpty(e.GetArg("number")))
                    {
                        count = 99;
                    }
                    else
                    {
                        count = Convert.ToInt32(e.GetArg("number"));
                    }
                    IEnumerable<Discord.Message> msgs;
                    int deleted = 0;
                    var cachedMsgs = e.Channel.Messages;
                    if (cachedMsgs.Count() < count)
                        msgs = (await e.Channel.DownloadMessages(count + 1));
                    else
                        msgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(count + 1);
                    foreach (var msg in msgs)
                    {
                        if (msg.IsAuthor)
                        {
                            await msg.Delete();
                            deleted++;
                        }
                    }
                    Discord.Message msgsnt = await e.Channel.SendMessage($"Deleted `{deleted - 1}` messages");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await msgsnt.Delete();
                });
            Discord.GetService<CommandService>().CreateCommand("emoji")
                .Alias("e")
                .Description("Converts input text to emoji")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string message = "";
                    char[] input = e.GetArg("text").ToCharArray();
                    foreach (char letter in input)
                    {
                        if (char.IsPunctuation(letter))
                        {
                            message += $"**{letter}**";
                        }
                        else if (char.IsWhiteSpace(letter)) message += "  ";
                        else if (char.IsLetter(letter))
                        {
                            message += $":regional_indicator_{letter.ToString().ToLower()}:";
                        }
                        else if (char.IsDigit(letter))
                        {
                            message += ToWord(letter);
                        }
                    }
                    await e.Message.Edit(message);
                });
            Discord.GetService<CommandService>().CreateCommand("find")
                .Alias("f")
                .Description("Find a user's ID from name or nick")
                .Parameter("name", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string name = e.GetArg("name");
                    string message = FindUser.Global(name, e);
                    await e.Message.Edit(message);
                });
            Discord.GetService<CommandService>()
                .CreateCommand("user")
                .Description("Returns detailed info about a user from their ID or Mention")
                .Alias("u")
                .Parameter("id")
                .Do(async (e) =>
                {
                    var id = Convert.ToUInt64(e.GetArg("id"));
                    string allroles = "";
                    User user = util.GetUsers(e).FirstOrDefault();
                    bool onServer = e.Server.FindUsers(user.Name).Any();
                    if (onServer)
                    {
                        var roles = e.Server.Users.FirstOrDefault(u => u.Id == id).Roles;
                        foreach (var role in roles)
                        {
                            allroles += role.Name + ", ";
                        }
                        allroles = allroles.Substring(0, allroles.Length - 13);
                    }
                    else allroles = "Not On Server";
                    TimeSpan userCreatedAt = TimeSpan.FromMilliseconds(((id / 4194304) + 1420070400000) + 62135596800000);
                    DateTime zero = new DateTime(0001, 01, 01, 00, 00, 00, 00);
                    var newUser = zero + userCreatedAt;
                    string discrim = user.Discriminator.ToString();
                    while (discrim.Length < 4) {discrim = "0" + discrim;}


                    string message = $"```UserName: {user.Name}#{discrim}\n";
                    message += $"User ID: {user.Id}\n";
                    message += $"User Created: {newUser}\n";
                    message += $"User is Bot?: {user.IsBot}\n";
                    message += $"User Status: {user.Status.Value}\n";
                    if (e.Server.FindUsers(user.Name).Any())
                        message += $"User Joined at: {user.JoinedAt}\n";
                    message += $"User Roles: {allroles}\n";
                    if (notes.ContainsKey(user.Id.ToString()))
                        message += $"User Notes: {notes[user.Id.ToString()].Trim()}\n";
                    if(message.Length < 1900)
                        message += $"User Avatar: {user.AvatarUrl}";
                    if (message.Length > 1995) message = message.Substring(0, 1995);
                    message += "\n```";
                    await e.Message.Edit(message);
                });
            Discord.GetService<CommandService>().CreateCommand("server")
                .Description("Returns information on the server the command is used in")
                .Do(async (e) =>
                {
                    Server srv = e.Server;
                    string message = "```\n";
                    var features = srv.Features.Any();
                    string discrim = srv.Owner.Discriminator.ToString();
                    while (discrim.Length < 4) {discrim = "0" + discrim;}
                    TimeSpan srvCreatedAt = TimeSpan.FromMilliseconds(((srv.Id / 4194304) + 1420070400000) + 62135596800000);
                    DateTime zero = new DateTime(0001, 01, 01, 00, 00, 00, 00);
                    var serverDate = zero + srvCreatedAt;

                    message += $"Server Name: {srv.Name}({srv.Id})\n";
                    message += $"Server Owner: {srv.Owner.Name}#{discrim}({srv.Owner.Id})\n";
                    message += $"Server Created: {serverDate}\n";
                    message += $"Server Roles: {srv.RoleCount}\n";
                    message += $"Server Members: {srv.UserCount}\n";
                    message += $"Server Text Channels: {srv.TextChannels.Count()}\n";
                    message += $"Server Voice Channels: {srv.VoiceChannels.Count()}\n";
                    message += $"Server Location: {srv.Region.Name}\n";
                    message += $"Server Partnered: {features}\n";
                    message += "```";

                    await e.Message.Edit(message);
                });
            Discord.GetService<CommandService>()
                .CreateGroup("note", cgb =>
                {
                    cgb.CreateCommand("add")
                        .Parameter("uid")
                        .Parameter("note", ParameterType.Unparsed)
                        .Do(async (e) =>
                        {
                            string uid = e.GetArg("uid");
                            string note = e.GetArg("note");
                            if (notes.ContainsKey(uid))
                                note += " : " + notes[uid];
                            notes.Remove(uid);
                            try
                            {
                                notes.Add(uid, note);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            File.WriteAllText("notes.json", JsonConvert.SerializeObject(notes, Formatting.Indented));
                            await e.Message.Edit("Note Successfully added");
                        });
                    cgb.CreateCommand("remove")
                        .Parameter("uid")
                        .Do(async (e) =>
                        {
                            string uid = e.GetArg("uid");
                            if (notes.ContainsKey(uid))
                                notes.Remove(uid);
                            File.WriteAllText("notes.json", JsonConvert.SerializeObject(notes, Formatting.Indented));
                            await e.Message.Edit("Note Successfully removed");
                        });
                });
            /*Discord.GetService<CommandService>().CreateCommand("eyes")
                .Parameter("times")
                .Do(async (e) =>
                {
                    int times = Convert.ToInt32(e.GetArg("times"));
                    for (int i = times; i > 0; i--)
                    {
                        await e.Message.Edit(":eyes:");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        await e.Message.Edit(":eyesFlipped:");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                });*/
            Discord.GetService<CommandService>().CreateCommand("short")
                .Description("Allows user to store strings and use them at a later date")
                .Alias("s")
                .Parameter("req")
                .Parameter("name", ParameterType.Optional)
                .Parameter("input", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    //await e.Message.Edit($"Params: {e.GetArg("req")} : {e.GetArg("name")} : {e.GetArg("input")}");
                    if (e.GetArg("req").ToLower().Equals("add"))
                    {
                        shortstring.Add(e.GetArg("name").ToLower(), e.GetArg("input"));
                        Console.WriteLine($"added {e.GetArg("name")}, {e.GetArg("input")}");
                        File.WriteAllText("short.json", JsonConvert.SerializeObject(shortstring, Formatting.Indented));
                        await e.Message.Edit($"Done adding");
                    }
                    if (e.GetArg("req").ToLower().Equals("remove"))
                    {
                        shortstring.Remove(e.GetArg("name"));
                        Console.WriteLine($"removed {e.GetArg("name")}, {e.GetArg("input")}");
                        File.WriteAllText("short.json", JsonConvert.SerializeObject(shortstring, Formatting.Indented));
                        await e.Message.Edit($"Removed :frowning:");
                    }
                    if (e.GetArg("req").ToLower().Equals("list"))
                    {
                        await e.Message.Delete();
                        int i = 1;
                        string message = "```\n";
                        foreach (string key in shortstring.Keys)
                        {
                            if (i % 4 == 0&& i != 0) message += key + "\n";
                            else
                            {
                                message += key;
                                for (int j = key.Length; j < 15; j++) message += " ";
                            }
                            i++;
                            if (message.Length > 1900)
                            {
                                message += "```";
                                await e.Channel.SendMessage(message);
                                message = "";
                            }
                        }
                        await e.Channel.SendMessage(message + "```");
                    }
                    if (shortstring.ContainsKey(e.GetArg("req").ToLower()))
                    {
                        await e.Message.Edit(shortstring[e.GetArg("req").ToLower()]);
                    }
                });
            Discord.GetService<CommandService>()
                .CreateCommand("max")
                .Description("Just don't. Sends the input with the last character repeated to the 2k character limit.")
                .Parameter("msg", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    await e.Message.Delete();
                    string msg = e.GetArg("msg");
                    while (msg.Length < 1999)
                    {
                        msg += msg.Substring(msg.Length - 1);
                    }
                    await e.Channel.SendMessage(msg);
                });
            Discord.GetService<CommandService>().CreateCommand("restart")
                .Do(async (e) =>
                {
                    await e.Message.Delete();
                    Process.Start("CortanaSelfBot.exe");
                    Application.Exit();
                    await Discord.Disconnect();
                });
            Discord.GetService<CommandService>().CreateCommand("role")
                .Description("attempts to return info for a given role")
                .Parameter("name", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var role = e.Server.FindRoles(e.GetArg("name"), exactMatch:false).FirstOrDefault();
                    await e.Message.Edit($"Provided role `{e.GetArg("name")}` best matches `{role.Name}`(`{role.Id}`)"+
                                         $"\nMembers: {role.Members.Count()} \nColor: {role.Color}");
                });
            Discord.GetService<CommandService>()
                .CreateCommand("clap")
                .Description("Adds claps into spaces from text")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    await e.Message.Delete();
                    char[] textar = e.GetArg("text").Trim().ToCharArray();
                    string text = "";
                    foreach (char chr in textar)
                    {
                        if (chr == ' ') text += " :clap: ";
                        else text += chr;
                    }
                    await e.Channel.SendMessage(text);
                });

            Discord.GetService<CommandService>().CreateCommand("google")
                .Description("Helps you google a phrase for someone")
                .Parameter("phrase", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    char[] phrase = e.GetArg("phrase").Trim().ToCharArray();
                    string msg = "http://lmgtfy.com/?q=";
                    foreach (char c in phrase)
                    {
                        if(char.IsLetterOrDigit(c))
                        {
                            msg += c;
                        }
                        else if (c == ' ') msg += "+";
                    }
                    await e.Message.Edit($"Your result for `{e.GetArg("phrase")}`: {msg}");
                });
            Discord.GetService<CommandService>().CreateCommand("steal")
                .Description("Takes a user's avatar and sets it as your own")
                .Parameter("shrug") //because I need a parameter for this to work, but it's never used. ¯\_(ツ)_/¯
                .Do(async (e) =>
                {
                    await e.Message.Delete();
                    Directory.CreateDirectory("temp");
                    User u = util.GetUsers(e).FirstOrDefault();
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(u.AvatarUrl, $"temp/{u.AvatarId}.png");
                    }
                    using( System.IO.Stream stream = new System.IO.FileStream($"temp/{u.AvatarId}.png", System.IO.FileMode.Open, System.IO.FileAccess.Read) )
                    {
                        await e.Message.Client.CurrentUser.Edit(avatar: stream, avatarType: ImageType.Png);
                        stream.Close();
                        stream.Dispose();
                    }
                    File.Delete($"temp/{u.AvatarId}.png");
                });

            Discord.GetService<CommandService>().CreateCommand("Avatar")
                            .Description("Gets an avatar from a URL")
                            .Parameter("url") //because I need a parameter for this to work, but it's never used. ¯\_(ツ)_/¯
                            .Do(async (e) =>
                            {
                                await e.Message.Delete();
                                Directory.CreateDirectory("temp");
                                using (var client = new WebClient())
                                {
                                    client.DownloadFile(e.GetArg("url"), $"temp/Avatar.png");
                                }
                                using (System.IO.Stream stream = new System.IO.FileStream($"temp/Avatar.png", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                                {
                                    await e.Message.Client.CurrentUser.Edit(avatar: stream, avatarType: ImageType.Png);
                                    stream.Close();
                                    stream.Dispose();
                                }
                                File.Delete($"temp/Avatar.png");
                            });



            Discord.GetService<CommandService>().CreateGroup("cache", cgb =>
            {
                cgb.CreateCommand("info")
                    .Do(async (e) =>
                    {
                        TimeSpan thisMessage = TimeSpan.FromMilliseconds(
                            ((messageCache.Max(x => x.Key) / 4194304) + 1420070400000) + 62135596800000);
                        TimeSpan firstMessage =
                            TimeSpan.FromMilliseconds(((messageCache.Min(x => x.Key) / 4194304) + 1420070400000) +
                                                      62135596800000);
                        var messageAge = thisMessage - firstMessage;

                        await e.Message.Edit($"Max cache size: `{cachesize}`\n`{messageCache.Count}` messages in cache " +
                                             $"\nCache age: {messageAge}");

                    });
                cgb.CreateCommand("clear")
                    .Do(async (e) =>
                    {
                        await e.Message.Edit($"Clearing {messageCache.Count} messages from cache");
                        messageCache.Clear();
                    });
                cgb.CreateCommand("size")
                    .Parameter("size")
                    .Do(async (e) =>
                    {
                        int oldSize = cachesize;
                        cachesize = Convert.ToInt32(e.GetArg("size"));
                        await e.Message.Edit($"Cache size changed from `{oldSize}` to `{cachesize}`");
                    });
            });

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Discord.MessageReceived += (async (s, m) =>
            {
                try
                {
                    TimeSpan thisMessage = TimeSpan.FromMilliseconds(
                        ((messageCache.Max(x => x.Key) / 4194304) + 1420070400000) + 62135596800000);
                    TimeSpan firstMessage =
                        TimeSpan.FromMilliseconds(((messageCache.Min(x => x.Key) / 4194304) + 1420070400000) +
                                                  62135596800000);
                    var messageAge = thisMessage - firstMessage;
                    double minuteAge = messageAge.TotalSeconds;
                    while (minuteAge > cachesize * 60)
                    {
                        messageCache.Remove(messageCache.Min(x => x.Key));
                        thisMessage = TimeSpan.FromMilliseconds(
                            ((messageCache.Max(x => x.Key) / 4194304) + 1420070400000) + 62135596800000);
                        firstMessage =
                            TimeSpan.FromMilliseconds(((messageCache.Min(x => x.Key) / 4194304) + 1420070400000) +
                                                      62135596800000);
                        messageAge = thisMessage - firstMessage;
                        minuteAge = messageAge.TotalSeconds;
                    }
                }
                catch (Exception ex)
                {
                }

                messageCache.Add(m.Message.Id, m.Message);

            });
            Discord.MessageUpdated += (async (s, m) =>
            {
                try
                {
                    TimeSpan thisMessage = TimeSpan.FromMilliseconds(
                        ((messageCache.Max(x => x.Key) / 4194304) + 1420070400000) + 62135596800000);
                    TimeSpan firstMessage =
                        TimeSpan.FromMilliseconds(((messageCache.Min(x => x.Key) / 4194304) + 1420070400000) +
                                                  62135596800000);
                    var messageAge = thisMessage - firstMessage;
                    double minuteAge = messageAge.TotalSeconds;
                    while (minuteAge > cachesize * 60)
                    {
                        thisMessage = TimeSpan.FromMilliseconds(
                            ((messageCache.Max(x => x.Key) / 4194304) + 1420070400000) + 62135596800000);
                        firstMessage =
                            TimeSpan.FromMilliseconds(((messageCache.Min(x => x.Key) / 4194304) + 1420070400000) +
                                                      62135596800000);
                        messageAge = thisMessage - firstMessage;
                        minuteAge = messageAge.TotalSeconds;
                        messageCache.Remove(messageCache.Min(x => x.Key));
                    }
                }
                catch (Exception ex)
                {
                }
                messageCache.Remove(m.After.Id);
                messageCache.Add(m.After.Id, m.After);

            });
            Discord.MessageUpdated += (async (s, m) =>
            {
                try
                {
                    if ((m.Channel.IsPrivate || logServers.Contains(m.Server.Id.ToString()))
                        && m.Before.RawText != m.After.RawText
                        && !m.User.IsBot)
                    {
                        Console.WriteLine("---------MESSAGE EDITED------------");
                        try
                        {
                            Console.WriteLine(m.Server.Name + " " + m.Channel.Name);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("PM: " + m.Channel.Name);
                        }
                        try
                        {
                            Console.WriteLine(m.User.Name + m.User.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("User not found");
                        }
                        Console.WriteLine(m.Before.RawText);
                        Console.WriteLine(m.After.RawText);
                    }
                }
                catch (Exception ex)
                {
                }
            });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Discord.MessageDeleted += (async (s, m) =>
            {
                try
                {
                    if (m.Channel.IsPrivate || logServers.Contains(m.Server.Id.ToString())
                        && !m.User.IsBot)
                    {
                        Console.WriteLine("---------MESSAGE DELETED------------");
                        try
                        {
                            Console.WriteLine(m.Server.Name + " " + m.Channel.Name);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("PM: " + m.Channel.Name);
                        }
                        try
                        {
                            Message thisMsg = messageCache[m.Message.Id];
                            Console.WriteLine(thisMsg.User.Name + thisMsg.User.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("User not found");
                        }
                        Console.WriteLine(m.Message.RawText);
                    }
                }
                catch (Exception ex)
                {
                }
            });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously




            Discord.ExecuteAndWait(async () =>{
                await Discord.Connect(config.Token(), TokenType.User);
                await Task.Delay(1000);

            });
        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            if (e.Message.Contains("CHUNK")) Console.WriteLine(DateTime.Now + ": " + e.Message);
            if (e.Message.ToLower().Contains("connect")) Console.WriteLine(DateTime.Now + ": " + e.Message);
        }

        public string ToWord(char letter)
        {
            if (letter == '0') return ":zero:";
            if (letter == '1') return ":one:";
            if (letter == '2') return ":two:";
            if (letter == '3') return ":three:";
            if (letter == '4') return ":four:";
            if (letter == '5') return ":five:";
            if (letter == '6') return ":six:";
            if (letter == '7') return ":seven:";
            if (letter == '8') return ":eight:";
            if (letter == '9') return ":nine:";
            return "";
        }
    }
}

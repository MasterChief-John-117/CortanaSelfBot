using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Security;
using Discord;
using Discord.Commands;
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

        public MainBot()
        {
            notes = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("notes.json"));
            shortstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("short.json"));


            try{foreach (KeyValuePair<string, string> kvp in notes)
            {
                Console.WriteLine(kvp.Key + " : " + kvp.Value);
            }}
            catch(Exception ex){Console.WriteLine(ex.Message);}
            try{foreach (KeyValuePair<string, string> kvp in shortstring)
            {
                Console.WriteLine(kvp.Key + " : " + kvp.Value);
            }}
            catch(Exception ex){Console.WriteLine(ex.Message);}
            Discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Error;
                x.LogHandler = Log;
            });

            Discord.UsingCommands(x =>
            {
                x.PrefixChar = "\\";
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
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("ban")
            .Description("Uploads the VeraBan image")
            .Do(async (e) =>
                {
                    await e.Message.Delete();
                    await e.Channel.SendFile("img\\ban.png");
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("img")
            .Description("Uploads an image given the name from bot's img folder")
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
                        await Task.Delay(TimeSpan.FromSeconds(2f));
                        await e.Message.Delete();
                    }
                    CommandLogger.log(e);
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
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("clear")
            .Alias("c")
            .Description("Clears `n` messages, limits to and defaults to 100")
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
                    IEnumerable<Message> msgs;
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
                    Message msgsnt = await e.Channel.SendMessage($"Deleted `{deleted - 1}` messages");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await msgsnt.Delete();
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("nuke")
            .Description("Removes **ALL** messages up to and defaulting to 100, if user has `Manage Messages`")
                .Parameter("number", ParameterType.Optional)
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.ManageMessages)
                    {
                        IEnumerable<Message> msgs;
                        int deleted = 0;
                        var cachedMsgs = e.Channel.Messages;
                        int count = 0;
                        if (e.GetArg("number") == "total")
                        {
                            msgs = await e.Channel.DownloadMessages();
                            while (msgs.Count() > 1)
                            {
                                cachedMsgs = e.Channel.Messages;
                                foreach (var msg in msgs)
                                {
                                    await msg.Delete();
                                    deleted++;
                                }
                                msgs = await e.Channel.DownloadMessages();
                            }
                        }
                        else if (String.IsNullOrEmpty(e.GetArg("number")))
                        {
                            count = 99;
                        }
                        else
                        {
                            count = Convert.ToInt32(e.GetArg("number"));
                        }
                        deleted = 0;
                        cachedMsgs = e.Channel.Messages;
                        if (cachedMsgs.Count() < count)
                            msgs = (await e.Channel.DownloadMessages(count + 1));
                        else
                            msgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(count + 1);
                        foreach (var msg in msgs)
                        {
                            await msg.Delete();
                            deleted++;
                        }
                        Message msgsnt = await e.Channel.SendMessage($"Deleted `{deleted - 1}` messages");
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        await msgsnt.Delete();
                    }
                    else if (!e.User.ServerPermissions.ManageMessages)
                    {
                        await e.Message.Edit("Sorry, I don't have permissions to delete messages here");
                    }
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("away")
            .Description("Sets the game as away and changes the status to Idle")
            .Parameter("reason", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    string reason = e.GetArg("reason");

                    if (!Isaway)
                    {
                        Isaway = true;
                        Discord.SetStatus(UserStatus.Idle);
                        Discord.SetGame(reason);
                        await e.Message.Edit($"You have been set to away with the reason `{AwayReason}`");
                    }
                    else if (Isaway || String.IsNullOrEmpty(reason))
                    {
                        Isaway = false;
                        Discord.SetStatus(UserStatus.Online);
                        Discord.SetGame(null);
                        await e.Message.Edit("Your away status has been cleared.");
                    }
                    CommandLogger.log(e);
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
                    CommandLogger.log(e);
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
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>()
                .CreateCommand("user")
            .Description("Returns detailed info about a user from their ID")
            .Alias("u")
                .Parameter("id")
                .Do(async (e) =>
                {
                    var id = Convert.ToUInt64(e.GetArg("id"));
                    string allroles = "";
                    User user = Discord.Servers.SelectMany(s => s.Users).FirstOrDefault(s => s.Id == id);
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
                    UserData.export(user);
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("server")
            .Do(async (e) =>
                {
                    Server srv = e.Server;
                    string message = "```\n";
                    var features = srv.Features.Any();
                    string discrim = srv.Owner.Discriminator.ToString();
                    while (discrim.Length < 4) {discrim = "0" + discrim;}
                    TimeSpan srvCreatedAt = TimeSpan.FromMilliseconds(((srv.Id / 4194304) + 1420070400000) + 62135596800000);
                    DateTime zero = new DateTime(0001, 01, 01, 00, 00, 00, 00);
                    var ServerDate = zero + srvCreatedAt;

                    message += $"Server Name: {srv.Name}({srv.Id})\n";
                    message += $"Server Owner: {srv.Owner.Name}#{discrim}({srv.Owner.Id})\n";
                    message += $"Server Created: {ServerDate}\n";
                    message += $"Server Roles: {srv.RoleCount}\n";
                    message += $"Server Members: {srv.UserCount}\n";
                    message += $"Server Text Channels: {srv.TextChannels.Count()}\n";
                    message += $"Server Voice Channels: {srv.VoiceChannels.Count()}\n";
                    message += $"Server Location: {srv.Region.Name}\n";
                    message += $"Server Partnered: {features}\n";
                    message += "```";

                    await e.Message.Edit(message);
                    CommandLogger.log(e);
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
                            Console.WriteLine(uid);
                            string note = e.GetArg("note");
                            if (notes.ContainsKey(uid))
                                note += " : " + notes[uid];
                            notes.Remove(uid);
                            Console.WriteLine(note);
                            Console.WriteLine("Trying to add");
                            try
                            {
                                notes.Add(uid, note);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            Console.WriteLine("added");
                            File.WriteAllText("notes.json", JsonConvert.SerializeObject(notes, Formatting.Indented));
                            await e.Message.Edit("Note Successfully added");
                            CommandLogger.log(e);
                        });
                    cgb.CreateCommand("remove")
                        .Parameter("uid")
                        .Do(async (e) =>
                        {
                            string uid = e.GetArg("uid");
                            Console.WriteLine(uid);
                            if (notes.ContainsKey(uid))
                            notes.Remove(uid);
                            File.WriteAllText("notes.json", JsonConvert.SerializeObject(notes, Formatting.Indented));
                            await e.Message.Edit("Note Successfully removed");
                            CommandLogger.log(e);
                        });
                });
            Discord.GetService<CommandService>().CreateCommand("eyes")
                .Parameter("times")
                .Do(async (e) =>
                {
                    int times = Convert.ToInt32(e.GetArg("times"));
                    for (int i = times; i > 0; i--)
                    {
                        await e.Message.Edit("http://i.imgur.com/ySGyNE7.png");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        await e.Message.Edit("http://i.imgur.com/BFn1JJJ.png");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    CommandLogger.log(e);

                });
            Discord.GetService<CommandService>().CreateCommand("short")
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
                        string message = "```\n";
                        foreach (string key in shortstring.Keys)
                        {
                            message += key + "\n";
                        }
                        message += "```";
                        await e.Message.Edit(message);
                    }
                    if (shortstring.ContainsKey(e.GetArg("req").ToLower()))
                    {
                        await e.Message.Edit(shortstring[e.GetArg("req").ToLower()]);
                    }
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>()
                .CreateCommand("max")
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
                    CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("del")
            .Parameter("time")
            .Parameter("message", ParameterType.Unparsed)
            .Do(
                async (e) =>
                {
                    await e.Message.Delete();
                    //double tf = Convert.ToDouble(e.GetArg("time"));
                    string message = e.GetArg("messgage");
                    Message msgsnt = await e.Message.Channel.SendMessage("beep");
                    //await Task.Delay(TimeSpan.FromSeconds(2));
                    //await msgsnt.Delete();
                    CommandLogger.log(e);

                });
                Discord.GetService<CommandService>().CreateCommand("restart")
                .Do(async (e) =>
                    {
                        await e.Message.Delete();
                        Process.Start("CortanaSelfBot.exe");
                    await Discord.Disconnect();
                        CommandLogger.log(e);
                });
            Discord.GetService<CommandService>().CreateCommand("getcommand")
            .Parameter("cmdId")
            .Do(async (e) =>
                {
                    CommandLogger.log(e);
                    try
                    {
                        await e.Message.Edit(CommandLogger.toString(e.GetArg("cmdId")));
                    }
                    catch (Exception ex)
                    {
                        await e.Message.Edit(ex.Message + "\n" + ex.StackTrace);
                    }
                });
            Discord.GetService<CommandService>().CreateCommand("role")
            .Parameter("name", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    var role = e.Server.FindRoles(e.GetArg("name"), exactMatch:false).FirstOrDefault();
                    await e.Message.Edit($"Provided role `{e.GetArg("name")}` best matches `{role.Name}`(`{role.Id}`)");
                    CommandLogger.log(e);
                });

            Discord.ExecuteAndWait(async () =>
            {
                await Discord.Connect(File.ReadAllText("token.txt"), TokenType.User);
            });
        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(DateTime.Now + ": " + e.Message);
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

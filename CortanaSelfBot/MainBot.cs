using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ParameterType = Discord.Commands.ParameterType;
using Newtonsoft;
using Newtonsoft.Json;
using RestSharp.Deserializers;

namespace CortanaSelfBot
{
    public class MainBot
    {
        public DiscordClient Discord;
        public String AwayReason;
        public bool Isaway;
        private  static Random rand = new Random();

        Dictionary<string, string> shortstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("short.json"));

        public MainBot()


        {
            Discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Verbose;
                x.LogHandler = Log;
            });

            Discord.UsingCommands(x =>
            {
                x.PrefixChar = "c.";
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.IsSelfBot = true;

            });

            Discord.GetService<CommandService>().CreateCommand("ping")
            .Description("Just used to check if the bot is up")
            .Do(async (e) =>
                {
                    await e.Message.Edit("Pong!");
                });
            Discord.GetService<CommandService>().CreateCommand("short")
            .Parameter("req")
            .Parameter("name", ParameterType.Optional)
            .Parameter("input", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    await e.Message.Edit($"Params: {e.GetArg("req")} : {e.GetArg("name")} : {e.GetArg("input")}");
                    if (e.GetArg("req").Equals("add"))
                    {
                        shortstring.Add(e.GetArg("name"), e.GetArg("input"));
                        Console.WriteLine($"added {e.GetArg("name")}, {e.GetArg("input")}");
                        //File.WriteAllText("short.json", JsonConvert.SerializeObject(shortstring));
                        await e.Message.Edit($"Done adding");
                    }
                    if (shortstring.ContainsKey(e.GetArg("req")))
                    {
                        Console.WriteLine($"requested {e.GetArg("string")}");
                        await e.Message.Edit(shortstring[e.GetArg("string")]);
                    }
                    //await e.Message.Edit($"`{e.GetArg("req")}`");

                });
            Discord.GetService<CommandService>().CreateCommand("ban")
            .Description("Uploads the VeraBan image")
            .Do(async (e) =>
                {
                    await e.Message.Delete();
                    await e.Channel.SendFile("img\\ban.png");
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

                });
            Discord.GetService<CommandService>().CreateCommand("nuke")
            .Description("Removes **ALL** messages up to and defaulting to 100, if user has `Manage Messages`")
                //.Parameter("code")
                .Parameter("number", ParameterType.Optional)
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.ManageMessages /* && e.GetArg("code") == nukey*/)
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
                    else
                    {
                        await e.Message.Edit("The key used is incorrect");
                    }
                });
            Discord.GetService<CommandService>().CreateCommand("away")
            .Description("Sets a reason for being away that is returned if the user is mentioned")
            .Parameter("reason", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    string reason = e.GetArg("reason");

                    if (!Isaway)
                    {
                        Isaway = true;
                        AwayReason = reason;
                        await e.Message.Edit($"You have been set to away with the reason `{AwayReason}`");
                    }
                    else if (Isaway || String.IsNullOrEmpty(reason))
                    {
                        Isaway = false;
                        AwayReason = "";
                        await e.Message.Edit("Your away status has been cleared.");
                    }
                });
            Discord.GetService<CommandService>().CreateCommand("emoji")
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
            .Description("Find a user's ID from name or nick")
            .Parameter("name", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    string name = e.GetArg("name");
                    string message = FindUser.Global(name, e);
                    await e.Message.Edit(message);
                });
        Discord.GetService<CommandService>().CreateCommand("eyes")
            .Parameter("times", ParameterType.Optional)
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

            });
            /*Discord.GetService<CommandService>().CreateCommand("restart")
            .Do(async (e) =>
                {

                });*/
            Discord.GetService<CommandService>().CreateCommand("role")
            .Parameter("name", ParameterType.Unparsed)
            .Do(async (e) =>
                {
                    var role = e.Server.FindRoles(e.GetArg("name"), exactMatch:false).FirstOrDefault();
                    await e.Message.Edit($"Provided role `{e.GetArg("name")}` best matches `{role.Name}`(`{role.Id}`)");
                });

            Discord.ExecuteAndWait(async () =>
            {
                await Discord.Connect("mfa.RYMMRQDvNsbghV9QfwKcLlKsGtRlFetOY3_zHDCISCaHlEs2yLFFQWCQbE8nBx5rgb83HI9Pw4hjSJKGVXi3", TokenType.User);
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

        private static string getkey()
        {
            return rand.Next(10000, 99999).ToString();
        }
    }
}

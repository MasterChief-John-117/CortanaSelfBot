using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ParameterType = Discord.Commands.ParameterType;

namespace CortanaSelfBot
{
    public class MainBot
    {
        public DiscordClient Discord;

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
            .Parameter("number", ParameterType.Optional)
            .Do(async (e) =>
                {
                    var count = Convert.ToInt32(e.GetArg("number"));
                    if (String.IsNullOrEmpty(e.GetArg("number"))) count = 99;
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
                    await e.Channel.SendMessage($"Deleted `{deleted}` messages");
                });
            Discord.GetService<CommandService>().CreateCommand("nuke")
            .Description("Removes **ALL** messages up to and defaulting to 100, if user has `Manage Messages`")
                .Parameter("number", ParameterType.Optional)
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.ManageMessages)
                    {
                        int count = 0;
                        count = Convert.ToInt32(e.GetArg("number"));
                        if (count < 1) count = 99;
                        IEnumerable<Message> msgs;
                        int deleted = 0;
                        var cachedMsgs = e.Channel.Messages;
                        if (cachedMsgs.Count() < count)
                            msgs = (await e.Channel.DownloadMessages(count + 1));
                        else
                            msgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(count + 1);
                        foreach (var msg in msgs)
                        {
                            await msg.Delete();
                            deleted++;
                        }
                        await e.Channel.SendMessage($"Deleted `{deleted}` messages");
                    }
                    else await e.Message.Edit("Sorry, I don't have permissions to delete messages here");
                });
            Discord.ExecuteAndWait(async () =>
            {
                await Discord.Connect("MTY5OTE4OTkwMzEzODQ4ODMy.C4pDFQ.Lu9Jk7pRuLQtPrGWf4qMeMptOIM", TokenType.User);
            });
        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(DateTime.Now + ": " + e.Message);
        }

    }
}

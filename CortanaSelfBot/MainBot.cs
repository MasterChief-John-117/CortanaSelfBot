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

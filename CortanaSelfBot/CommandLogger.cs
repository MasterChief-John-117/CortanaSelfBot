using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Security;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;


namespace CortanaSelfBot
{
    public class CommandLogger
    {
        public string hash;
        public string fullmsg;
        public string time;
        public string author;
        public string message;
        public string server;
        public string channel;


        public static Dictionary<string, CommandLogger> UsedCommands = new Dictionary<string, CommandLogger>();

        public CommandLogger(Message msg)
        {
            this.hash = cmdHasher(fullmsg + msg.Id.ToString());
            this.fullmsg = msg.RawText;
            this.time = msg.Timestamp.ToLongDateString() + " " + msg.Timestamp.ToLongTimeString();
            this.author = msg.User.Name + " " + msg.User.Id;
            this.message = msg.Id.ToString();
            this.server = msg.Server.Name + " " + msg.Server.Id;
            this.channel = msg.Channel.Name + " " + msg.Channel.Id;
        }



        public static string cmdHasher(string text)
        {
            return (FormsAuthentication.HashPasswordForStoringInConfigFile(text, "md5").ToLower());
        }

        public static void log(CommandEventArgs e)
        {
            try
            {
                var cmd = new CommandLogger(e.Message);
                //Console.WriteLine(cmd.hash);
                string alreadyin = File.ReadAllText("log.json");
                File.WriteAllText("log.json", JsonConvert.SerializeObject(cmd, Formatting.Indented) + "\n");
                File.AppendAllText("log.json", alreadyin);
                UsedCommands.Add(cmd.hash, cmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string toString(string cmdId)
        {
            try
            {
                CommandLogger cmd = UsedCommands[cmdId];
                String msgToReturn = "";
                msgToReturn += cmd.hash +"\n";
                msgToReturn += cmd.fullmsg +"\n";
                msgToReturn += cmd.time +"\n";
                msgToReturn += cmd.author +"\n";
                msgToReturn += cmd.message +"\n";
                msgToReturn += cmd.server +"\n";
                msgToReturn += cmd.channel +"\n";

                return msgToReturn;
            }
            catch(Exception ex)
            {
                return ex.Message + "\n" + ex.StackTrace;
            }
        }
    }
}

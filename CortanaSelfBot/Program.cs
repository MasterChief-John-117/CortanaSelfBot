
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Discord.Commands.Permissions.Userlist;

namespace CortanaSelfBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (File.Exists("readme.txt"))
            {
                Console.WriteLine(File.ReadAllText("readme.txt"));
                string str = Console.ReadLine();
                if (str.ToLower() != "i agree")
                {
                    Application.Exit();
                }
                else
                {
                    File.Delete("readme.txt");
                    try
                    {
                        var mainBot = new MainBot();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        Process.Start("CortanaSelfBot.exe");
                        Application.Exit();
                        MessageBox.Show(ex.Message, "Connection failed");

                    }
                }
            }
            else
            {
                try
                {
                    var mainBot = new MainBot();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    MessageBox.Show(ex.Message);
                    Process.Start("CortanaSelfBot.exe");
                    Application.Exit();
                }
            }


        }
    }
}


/*
This self-bot was made by MasterChief_John-117#1911 and may not be distributed without his written consent.
Source code can be found at https://github.com/MasterChief-John-117/CortanaSelfBot. To make code edits, fork the repository and make a branch with a descriptive name, then make a push request to be reviewed.

Use of this bot indicates that you are aware of all risks and Terms Of Service put forth by Discord. The maker of this bot is not responsible for any consequenses that may occur from the use of this bot. To indicate that you agree with the above terms, type "I Agree" in the form below.
(This message will only show on the first run of this bot)
*/

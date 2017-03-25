
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
            if (!File.Exists("notes.json"))
            {
                File.Create("notes.json");
            }
            try
            {
                var mainBot = new MainBot();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.Start("CortanaSelfBot.exe");
                Application.Exit();
            }

        }
    }
}

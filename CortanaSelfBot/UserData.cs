using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Newtonsoft.Json;

namespace CortanaSelfBot
{
    public class UserData
    {
        public ulong Id;

        public string Name;
        public string Discrim;
        public string notes;


        //private static string AllUsersIn = File.ReadAllText("users.json");
        //public static Dictionary<ulong, UserData> AllUsers =
          // new Dictionary<ulong, UserData>(JsonConvert.DeserializeObject<Dictionary<ulong, UserData>>(AllUsersIn));
        public static Dictionary<ulong, UserData> AllUsers = new Dictionary<ulong, UserData>();

        public UserData(User u)
        {
            this.Id = u.Id;
            this.Name = u.Name;
            this.Discrim = u.Discriminator.ToString();
            while (Discrim.Length < 4) {Discrim = "0" + Discrim;}

        }

        public static void export(User u)
        {
            try
            {
                var user = new UserData(u);
                string alreadyin = File.ReadAllText("users.json");
                File.WriteAllText("users.json", JsonConvert.SerializeObject(user, Formatting.Indented) + "\n");
                File.AppendAllText("users.json", alreadyin);
                AllUsers.Add(user.Id, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}

using Discord;
using Discord.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBot
{
    class MyBot
    {
        DiscordClient discord;
        public MyBot()
        {

            var rnd = new Random();

            discord = new DiscordClient();

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            // -- COMMANDS --

            var commands = discord.GetService<CommandService>();

            // HELLO : greets user in the same language
            String[] engHiArr = new String[] { "Hi", "Hello", "Hey", "Howdy", "Yo", "Sup", "Hiya", "'ello", "Greetings" };
            String[] japHiArr = new String[] { "Konnichiwa", "Ohayō gozaimasu", "Ohayo gozaimasu", "Ohayō", "Ohayo" };
            String[] polHiArr = new String[] { "Cześć", "Czesc", "Dzień dobry", "Dzien dobry" };
            commands.CreateCommand("Hi")
                .Alias(engHiArr.Concat(japHiArr).Concat(polHiArr).ToArray())
                .Do(async (e) =>
                {
                    var greeting = "Hi";
                    //find language bot was greeted in, and choose random greeting from matching language
                    if (engHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = engHiArr.ElementAt((rnd.Next(0, engHiArr.Length)));
                    }
                    else if (japHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = japHiArr.ElementAt((rnd.Next(0, japHiArr.Length)));
                    }
                    else if (polHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = polHiArr.ElementAt((rnd.Next(0, polHiArr.Length)));
                    }

                    await e.Channel.SendMessage(greeting + " " + e.User.Name);
                });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect({bot-token}, TokenType.Bot);
            });
        }

        
    }
}

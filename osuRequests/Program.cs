using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace osuRequests
{
    class Program
    {
        private static string CleanInput(string message)
        {
            Console.Write($"{message}: ");

            string input = Console.ReadLine();

            return input.Trim();
        }

        static void Main(string[] args)
        {
            Configuration config = new Configuration()
            {
                Info = new Info(),
                Credentials = new Credentials()
                {
                    OsuCredentials = new OsuCredentials(),
                    TwitchCredentials = new TwitchCredentials()
                }
            };

            if (args.Length == 0 && !File.Exists("config.json"))
            {
                Console.WriteLine("config.json not found, and no custom config sent as argument, let's generate a new one!");

                config.Info.TwitchChat = CleanInput("Enter the Twitch user whose chat the bot should monitor");
                config.Info.TargetUser = CleanInput("Enter the osu! user who the requests will be sent to");

                config.Credentials.OsuCredentials.Username = CleanInput("Enter your osu! user name");
                config.Credentials.OsuCredentials.Password = CleanInput("Enter your osu! IRC password");
                config.Credentials.OsuCredentials.ApiKey = CleanInput("Enter your osu! api key (not required, some features will be missing)");

                config.Credentials.TwitchCredentials.Username = CleanInput("Enter your Twitch.tv user name");
                config.Credentials.TwitchCredentials.Password = CleanInput("Enter your Twitch.tv oauth token");

                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));

            }
            else
            {
                if (args.Length != 0)
                {
                    Console.WriteLine($"Reading {args[0]}...");

                    if (!File.Exists(args[0]))
                    {
                        Console.WriteLine($"{args[0]} not found. Exiting.");
                        Environment.Exit(1);
                    }

                    config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(args[0]));
                }
                else
                {
                    Console.WriteLine($"Reading config.json...");
                    config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
                }
            }

            IrcBot bot = new IrcBot(config.Credentials, config.Info.TwitchChat, config.Info.TargetUser);
            bot.Connect(BotType.Osu);
            bot.Connect(BotType.Twitch);

            Globals.OsuApiHelper = new OsuApiHelper(config.Credentials.OsuCredentials.ApiKey);


            while (true)
            {
                Thread.Sleep(50);
            }
        }
    }
}

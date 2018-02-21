using System;
using System.Collections.Generic;
using System.Text;
using IrcDotNet;

namespace osuRequests
{
    class IrcBot
    {
        private string m_targetUser;
        private string m_twitchChat;

        private StandardIrcClient m_osuClient;
        private TwitchIrcClient m_twitchClient;

        private Credentials m_credentials;

        public IrcBot(Credentials creds, string twitchChat, string targetUser)
        {
            m_credentials = creds;
            m_targetUser = targetUser;
            m_twitchChat = twitchChat;
        }

        private string ComposeRawMessage(BotType target, string message)
        {
            switch (target)
            {
                case BotType.Osu:
                    {
                        return $"PRIVMSG {m_targetUser} :{message}\r\n";
                    }
                case BotType.Twitch:
                    {
                        return $":{m_twitchClient.LocalUser.NickName}!{m_twitchClient.LocalUser.UserName}@{m_twitchClient.LocalUser.HostName} PRIVMSG #{m_twitchChat} :{message}\r\n";
                    }
                default:
                    return ""; // wat
            }
        }

        public bool Connect(BotType target)
        {
            switch (target)
            {
                case BotType.Osu:
                    {
                        IrcUserRegistrationInfo reg = new IrcUserRegistrationInfo()
                        {
                            NickName = m_credentials.OsuCredentials.Username,
                            UserName = m_credentials.OsuCredentials.Username,
                            RealName = m_credentials.OsuCredentials.Username,
                            Password = m_credentials.OsuCredentials.Password,
                        };

                        try
                        {
                            m_osuClient = new StandardIrcClient();

                            m_osuClient.Connected += (o, e) =>
                            {
                                Console.WriteLine("Connected to irc.ppy.sh");
                            };

                            m_osuClient.ConnectFailed += (o, e) =>
                            {
                                Console.WriteLine("Failed connecting to irc.ppy.sh");
                            };

                            Console.WriteLine("Connecting to irc.ppy.sh...");

                            m_osuClient.RawMessageReceived += m_osuClient_RawMessageReceived;
                            m_osuClient.Disconnected += (o, e) =>
                            {
                                m_osuClient.Disconnect();
                                Console.WriteLine("Got disconnected from irc.ppy.sh, reconnecting...");
                                m_osuClient.Connect("irc.ppy.sh", 6667, false, reg);
                            };

                            m_osuClient.Connect("irc.ppy.sh", 6667, false, reg);
                            m_osuClient.SendRawMessage($"PASS {m_credentials.OsuCredentials.Password}\r\n");
                            m_osuClient.SendRawMessage($"NICK {m_credentials.OsuCredentials.Username}\r\n");

                            return true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Something happened while trying to connect to irc.ppy.sh, {e.Message}");
                            return false;
                        }
                    }
                case BotType.Twitch:
                    {
                        {
                            IrcUserRegistrationInfo reg = new IrcUserRegistrationInfo()
                            {
                                NickName = m_credentials.TwitchCredentials.Username,
                                UserName = m_credentials.TwitchCredentials.Username,
                                RealName = m_credentials.TwitchCredentials.Username,
                                Password = m_credentials.TwitchCredentials.Password
                            };

                            try
                            {
                                m_twitchClient = new TwitchIrcClient();
                                
                                m_twitchClient.Connected += (o, e) =>
                                {
                                    Console.WriteLine("Connected to irc.twitch.tv");
                                };

                                m_twitchClient.ConnectFailed += (o, e) =>
                                {
                                    Console.WriteLine("Failed connecting to irc.twitch.tv");
                                };

                                Console.WriteLine("Connecting to irc.twitch.tv...");

                                m_twitchClient.RawMessageReceived += m_twitchClient_RawMessageReceived;
                                m_twitchClient.Disconnected += (o, e) =>
                                {
                                    Console.WriteLine("Got disconnected from irc.twitch.tv, reconnecting...");
                                    m_twitchClient.Connect("irc.twitch.tv", 6667, false, reg);
                                };

                                m_twitchClient.Connect("irc.twitch.tv", 6667, false, reg);

                                return true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Something happened while trying to connect to irc.twitch.tv, {e.Message}");

                                return false;
                            }
                        }
                    }
                default:
                    return false; // wat
            }
        }

        private void m_osuClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            ParseMessage(BotType.Osu, e);
        }

        private void m_twitchClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            ParseMessage(BotType.Twitch, e);
        }

        private void CommandLog(BotType target, string message)
        {
            Console.WriteLine($"{target.ToString()}: {message}");
        }

        private async void ParseMessage(BotType target, IrcRawMessageEventArgs e)
        {
            switch (e.Message.Command)
            {
                // messages to ignore
                case "002": // twitch
                case "003":
                case "004":
                case "375":
                case "376":
                case "353":
                case "366":
                case "372": // osu! motd
                case "QUIT":
                    break;

                case "PRIVMSG":
#if DEBUG
                    Console.WriteLine($"{target.ToString()}: {e.Message.Source.Name}: {e.Message.Parameters[1]}");
#endif

                    if (target == BotType.Twitch)
                    {
                        string source = e.Message.Source.Name;
                        string message = e.Message.Parameters[1];
                        string[] request = message.Split(' ');

                        if (message.ToLowerInvariant().StartsWith("!request") || message.ToLowerInvariant().StartsWith("!req"))
                        {
                            if (!request[1].Contains("osu.ppy.sh/b/") && !request[1].Contains("osu.ppy.sh/s/"))
                            {
                                SendMessage(BotType.Twitch, $"@{source} that is an invalid beatmap.");
                                return;
                            }

                            string[] map = await Globals.OsuApiHelper.GetBeatmap(request[1]);

                            if (map[0] == "404")
                            {
                                SendMessage(BotType.Twitch, $"@{source} map not found.");
                            }

                            SendMessage(BotType.Osu, $"{source} requests {map[0]} to be played!");
                            SendMessage(BotType.Twitch, $"@{source} sent request for {map[1]} to be played");
                        }
                    }
                    break;

                case "PING":
                    {
                        switch (target)
                        {
                            case BotType.Osu:
                                m_osuClient.SendRawMessage("PONG");
                                break;
                            case BotType.Twitch:
                                m_twitchClient.SendRawMessage("PONG");
                                break;
                        }
                    }
                    break;

                case "JOIN":
                    if (target == BotType.Twitch)
                    {
                        CommandLog(target, $"Joined channel #{m_twitchChat}");
                    }
                    break;

                case "001":
                    CommandLog(target, "Auth Success");

                    if (target == BotType.Twitch)
                    {
                        m_twitchClient.SendRawMessage($"JOIN #{m_twitchChat}\r\n");
                    }

                    break;
                case "464":
                    CommandLog(target, "Auth Fail");
                    break;
                default:
#if DEBUG
                    CommandLog(target, $"{e.Message.Source.Name}: {e.Message.Command} - {String.Join(", ", e.Message.Parameters)}");
#endif
                    break;
            }
        }

        public void SendMessage(BotType target, string message)
        {
            switch (target)
            {
                case BotType.Osu:
                    {
                        string composed = ComposeRawMessage(target, message);
                        m_osuClient.SendRawMessage(composed);                        
                    }
                    break;
                case BotType.Twitch:
                    {
                        string composed = ComposeRawMessage(target, message);
                        m_twitchClient.SendRawMessage(composed);
                    }
                    break;
            }

#if DEBUG
            Console.WriteLine($"{target.ToString()} sent: {message}");
#endif
        }
    }
}

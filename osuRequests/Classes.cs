using System;
using System.Collections.Generic;
using System.Text;

namespace osuRequests
{
    class OsuCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
    }

    class TwitchCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class Credentials
    {
        public OsuCredentials OsuCredentials { get; set; }
        public TwitchCredentials TwitchCredentials { get; set; }
    }

    class Info
    {
        public string TwitchChat { get; set; }
        public string TargetUser { get; set; }
    }

    class Configuration
    {
        public Info Info { get; set; }
        public Credentials Credentials {get; set;}
    }

    static class Globals
    {
        public static OsuApiHelper OsuApiHelper { get; set; }
    }
}

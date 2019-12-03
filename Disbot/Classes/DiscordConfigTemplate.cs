using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Disbot.Classes
{
    public class DiscordConfigTemplate
    {
        public string AccessToken { get; set; }
        public ulong BotChannelID { get; set; }
        public ulong UsersChannelID { get; set; }
    }
}

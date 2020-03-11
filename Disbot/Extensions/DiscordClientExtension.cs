using Disbot.Configurations;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Disbot.Extensions
{
    public static class DiscordClientExtension
    {
        private static DiscordChannel DefaultChannel, DefaultVoiceChannel;
        /// <summary>
        /// Get default channel accord ot appsettings.json.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task<DiscordChannel> GetDefaultChannelAsync(this DiscordClient client)
        {
            if (DefaultChannel == null)
            {
                var channel = await client.GetChannelAsync(AppConfiguration.Content.Discord.UsersChannelID);
                DefaultChannel = channel;
            }
            return DefaultChannel;
        }
        public static async Task<DiscordChannel> GetDefaultVoiceChannelAsync(this DiscordClient client)
        {
            if (DefaultVoiceChannel == null)
            {
                var channel = await client.GetChannelAsync(AppConfiguration.Content.Discord.ActiveVoiceChannelId);
                DefaultVoiceChannel = channel;
            }
            return DefaultVoiceChannel;
        }
    }
}

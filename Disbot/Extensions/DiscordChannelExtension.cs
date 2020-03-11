using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Disbot.Extensions
{
    public static class DiscordChannelExtension
    {
        /// <summary>
        /// Send message that will automatically delete after timeout period.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="deleteInMs"></param>
        /// <returns></returns>
        public static async Task SendDisposableMessageAsync(this DiscordChannel channel, string message, int deleteInMs = 10000)
        {
            var sentMessage = await channel.SendMessageAsync(message);
            Task.Delay(deleteInMs).ContinueWith(async (arg) =>
            {
                await sentMessage.DeleteAsync();
            });

        }
    }
}

using Disbot.Configurations;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disbot
{
    public class Commands
    {
        [Command("clean")]
        [Description("Delete UNWANTED message on-demand.")]
        //[RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Clean(CommandContext context, byte range = 1, int limit = 100)
        {
            await context.RespondAsync($"[{DateTime.Now}] Cleaning top {100} messages in the past {range} hour(s).");
            await Task.Delay(3000);
            var ch = await context.Client.GetChannelAsync(AppConfiguration.Content.Discord.UsersChannelID);
            var unfilteredMessage = await ch.GetMessagesAsync(limit: limit);
            var messages = unfilteredMessage.Where(x => x.CreationTimestamp >= DateTime.Now.AddHours(-range));
            foreach (var message in messages)
            {
                await message.DeleteAsync();
                await Task.Delay(100);
            }
        }
        [Command("search")]
        [Description("Search music on spotify!")]
        public async Task Search(CommandContext context, string q, int top = 3)
        {
            try
            {
                var tracks = await SpotifyConfiguration.Context.Client.SearchItemsAsync(q, SpotifyAPI.Web.Enums.SearchType.Track, limit: top);
                var tracksString = tracks?.Tracks?.Items?.Select(x => $"{x.Name} - {x.Album.Name} by {x.Artists.FirstOrDefault()?.Name} @{x.ExternUrls.First().Value}");
                foreach (var track in tracksString)
                {
                    await context.RespondAsync(track);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}

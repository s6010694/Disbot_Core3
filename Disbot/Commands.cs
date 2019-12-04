using Disbot.Configurations;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disbot
{
    public class Commands
    {
        VoiceNextConnection connectedChannel { get; set; } = null;
        [Command("clean")]
        [Description("Delete UNWANTED message on-demand.")]
        //[RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Clean(CommandContext context, byte range = 1, int limit = 100)
        {
            await context.RespondAsync($"[{DateTime.Now}] Cleaning top {limit} messages in the past {range} hour(s).");
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
                if (tracks.Tracks.Items.Count() == 0)
                {
                    await context.RespondAsync($"{q} is not match with any music on Spotify.");
                    return;
                }
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
        [Command("join"), Description("Joins a voice channel!")]
        public async Task Join(CommandContext context, DiscordChannel channel = null)
        {
            var vnext = context.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await context.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var vcon = vnext.GetConnection(context.Guild);
            if (vcon != null)
            {
                await context.RespondAsync("Already connected!");
                return;
            }
            var vstate = context.Member?.VoiceState;
            if (vstate?.Channel == null && channel == null)
            {
                await context.RespondAsync("You're not in a voice channel!");
                return;
            }
            if (channel == null)
            {
                channel = vstate.Channel;
            }
            connectedChannel = await vnext.ConnectAsync(channel);
        }
        [Command("leave"), Description("Leaves a voice channel!")]
        public async Task Leave(CommandContext ctx)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we are connected
            if (this.connectedChannel == null)
            {
                // not connected
                return;
            }

            // disconnect
            connectedChannel.Disconnect();
            connectedChannel = null;
            //await ctx.RespondAsync("Disconnected");
        }
    }
}

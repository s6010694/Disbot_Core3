using Disbot.Configurations;
using Disbot.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Shared;

namespace Disbot
{
    public class Commands
    {
        [Command("clean")]
        [Description("Delete UNWANTED message on-demand.")]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Clean(CommandContext context, byte range = 1, int limit = 100, string FORCED = "")
        {
            try
            {
                if (0 < limit || limit < 1001)
                {
                    await context.RespondAsync($"กำลังลบ {limit} ข้อความที่เกี่ยวกับระบบใน {range} ชั่วโมงที่ผ่านมา");
                    await Task.Delay(3000);
                    var ch = await context.Client.GetChannelAsync(AppConfiguration.Content.Discord.UsersChannelID);
                    //var after = (Int32)(DateTime.UtcNow.AddHours(-range).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    var unfilteredMessage = await ch.GetMessagesAsync(limit: limit);
                    var messages = unfilteredMessage.Where(x => x.CreationTimestamp >= DateTime.Now.AddHours(-range)
                                   &&
                                   (
                                   x.Author.Id == context.Client.CurrentUser.Id ||
                                   x.Content.StartsWith(AppConfiguration.Content.CommandPrefix) ||
                                   x.Content.Contains(context.Client.CurrentUser.Id.ToString()) ||
                                   FORCED == "--force"
                                   ));
                    foreach (var message in messages)
                    {
                        await message.DeleteAsync();
                        await Task.Delay(100);
                    }
                    await context.Channel.SendDisposableMessageAsync("ลบข้อความเรียบร้อยแล้ว");
                }
            }
            catch (Exception ex)
            {
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog("clean", ex));
            }
        }
        [Command("search")]
        [Description("Search music on spotify!")]
        public async Task Search(CommandContext context, [RemainingText]string q)
        {
            try
            {
                var top = 3;
                var tracks = await SpotifyConfiguration.Context.Client.SearchItemsAsync(q, SpotifyAPI.Web.Enums.SearchType.Track, limit: top);
                if (tracks.Tracks.Items.Count() == 0)
                {
                    await context.RespondAsync($"ไม่พบผลการค้นหาสำหรับ '{q}' บน Spotify.");
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
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog(nameof(Search), ex));
                await context.RespondAsync("เกิดข้อผิดพลาดในการทำงานขึ้น แต่ได้บันทึกข้อผิดพลาดไว้สำหรับตรวจสอบแก้ไขแล้ว");
            }
        }
        [Command("trends")]
        public async Task News(CommandContext context)
        {
            try
            {
                var auth = new ApplicationOnlyAuthorizer()
                {
                    CredentialStore = AppConfiguration.Content.Authorization.Twitter
                };
                await auth.AuthorizeAsync();
                using var session = new TwitterContext(auth);
                var locations = session.Trends.Where(x => x.Type == TrendType.Available).FirstOrDefault().Locations;
                var country = locations.First(x => x.CountryCode == "TH" && x.Name == "Bangkok");
                var topTrends = session.Trends.Where(x => x.Type == TrendType.Place && x.WoeID == country.WoeID).Take(10).Select((x, idx) => $"{idx + 1}). {x.Name}");
                await context.RespondAsync("10 อันดับ trends ยอดนิยมขณะนี้คือ");
                foreach (var trend in topTrends)
                {
                    await context.RespondAsync(trend);
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog(nameof(News), ex));
                await context.RespondAsync("เกิดข้อผิดพลาดในการทำงานขึ้น แต่ได้บันทึกข้อผิดพลาดไว้สำหรับตรวจสอบแก้ไขแล้ว");
            }
        }
        [Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
                chn = vstat.Channel;

            // connect
            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }
        [Command("leave")]
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
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }
        [Command("play")]
        public async Task Play(CommandContext ctx)
        {
            try
            {
                var file = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.Combine("Assets", "levelup.mp3"));
                var vnext = ctx.Client.GetVoiceNextClient();
                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null)
                    throw new InvalidOperationException("Not connected in this guild.");

                if (!File.Exists(file))
                    throw new FileNotFoundException("File was not found.");

                await ctx.RespondAsync("👌");
                await vnc.SendSpeakingAsync(true); // send a speaking indicator

                var psi = new ProcessStartInfo
                {
                    FileName = @"D:\Downloads\ffmpeg_win32_x64\ffmpeg.exe",
                    Arguments = $@" -i ""{file}"" -ac 2 -f s16le -ar 48000",// pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var buff = new byte[3840];
                var br = 0;
                while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
                {
                    if (br < buff.Length) // not a full sample, mute the rest
                        for (var i = br; i < buff.Length; i++)
                            buff[i] = 0;

                    await vnc.SendAsync(buff, 20);
                }

                await vnc.SendSpeakingAsync(false); // we're not speaking anymore
            }
            catch (Exception ex)
            {
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog("play", ex));
            }
        }
        //[Command("level")]
        //public async Task Level(CommandContext context)
        //{
        //    var requestMember = context.Member;
        //    var dbMember = Service.Context.Member.Query(x => x.ID == (long)requestMember.Id).First();
        //    var currentLevel = dbMember.Level;
        //    var baseExp = Math.Pow(currentLevel - 1, 2);
        //    var currentExp = Math.Abs(Service.Context.MessageHistory.Count(x => x.MemberID == (long)requestMember.Id) - baseExp);
        //    var nextExp = Math.Abs(Math.Pow(currentLevel, 2) - baseExp);
        //    await context.Channel.SendDisposableMessageAsync($"{requestMember.Mention} ขณะนี้เลเวล {currentLevel}[{Math.Round(currentExp / nextExp, 2)}%]");
        //}
    }
}

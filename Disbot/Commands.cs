﻿using Disbot.Configurations;
using Disbot.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using LinqToTwitter;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Disbot
{
    public class Commands : BaseCommandModule
    {
        private async Task<bool> Validate(CommandContext context)
        {
            var member = Service.Context.Member.Query(x => x.ID == (long)context.User.Id).First();
            if (member.Level < 6)
            {
                await context.RespondAsync($"ต้องมีเลเวลอย่างน้อย 6 ถึงสามารถใช้คำสั่งได้นะ (ปัจจุบันเลเวล {member.Level})");
                return false;
            }
            return true;
        }
        [Command("clean")]
        [Description("Delete UNWANTED message on-demand.")]
        //[RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Clean(CommandContext context, byte range = 1, int limit = 100, string FORCED = "")
        {
            try
            {
                //if (!await Validate(context)) return;
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
                if (!await Validate(context)) return;
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
                if (!await Validate(context)) return;
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
            var vnext = ctx.Client.GetVoiceNext();
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
            //await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }
        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
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
            //await ctx.RespondAsync("Disconnected");
        }
        //[Command("play")]
        //public async Task Play(CommandContext ctx)
        //{
        //    try
        //    {
        //        var file = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.Combine("Assets", "levelup.mp3"));
        //        var vnext = ctx.Client.GetVoiceNext();
        //        var vnc = vnext.GetConnection(ctx.Guild);
        //        if (vnc == null)
        //            throw new InvalidOperationException("Not connected in this guild.");

        //        if (!File.Exists(file))
        //            throw new FileNotFoundException("File was not found.");

        //        await vnc.SendSpeakingAsync(true); // send a speaking indicator
        //        var psi = new ProcessStartInfo
        //        {
        //            FileName = @"D:\Downloads\ffmpeg_win32_x64\ffmpeg.exe",
        //            Arguments = $@" -i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
        //            RedirectStandardOutput = true,
        //            UseShellExecute = false
        //        };
        //        var ffmpeg = Process.Start(psi);
        //        var ffout = ffmpeg.StandardOutput.BaseStream;

        //        var buff = new byte[3840];
        //        var br = 0;
        //        var transmitter = vnc.GetTransmitStream();
        //        transmitter.VolumeModifier = 1;
        //        while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
        //        {
        //            if (br < buff.Length) // not a full sample, mute the rest
        //                for (var i = br; i < buff.Length; i++)
        //                    buff[i] = 0;

        //            //await transmitter.WriteAsync(buff);
        //        }
        //        await transmitter.WriteAsync(buff);
        //        await vnc.SendSpeakingAsync(false); // we're not speaking anymore
        //        await ctx.RespondAsync("👌");
        //    }
        //    catch (Exception ex)
        //    {
        //        await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog("play", ex));
        //    }
        //}
        [Command("level")]
        public async Task Level(CommandContext context)
        {

            var requestMember = context.Member;
            var dbMember = Service.Context.Member.Query(x => x.ID == (long)requestMember.Id).First();
            await context.Channel.SendMessageAsync($"{requestMember.Mention} เลเวล {dbMember.Level} {Math.Round(dbMember.Exp / dbMember.NextExp, 2) * 100}%");
            var avatarPath = Etc.MemberEtc.GetLevelupAvatar(context.Member.AvatarUrl, dbMember.Level);
            await context.Channel.SendFileAsync(avatarPath);
            //await context.Channel.SendFileAsync(avatarPath);
            File.Delete(avatarPath);

        }
        [Command("covid")]
        public async Task CovidReport(CommandContext context, string country = "thailand")
        {
            var sb = new StringBuilder();
            string title;
            try
            {
                using var client = new Utilities.HttpRequest("https://coronavirus-19-api.herokuapp.com");
                if (country.ToLower() == "all")
                {
                    title = "สถานการณ์ทั่วโลก ณ ปัจจุบัน";
                    var (StatusCode, result) = await client.GetAsync<Classes.CovidReportModel>("all");
                    sb.AppendLine("🌎 สถานการณ์ทั่วโลก ณ ปัจจุบัน");
                    sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบทั้งสิ้น {result.Cases}");
                    sb.AppendLine($"💀 จำนวนผู้เสียชีวิตทั้งสิ้น {result.Deaths}");
                    sb.AppendLine($"👌 จำนวนผู้ป่วยที่รักษาหายแล้วทั้งสิ้น {result.Recovered}");
                }
                else
                {
                    var (code, result) = await client.GetAsync<Classes.CovidReportModel>($"countries/{country}");
                    title = $"สถานการณ์ของ {result.Country} ณ ปัจจุบัน";
                    sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบวันนี้ {result.TodayCases}");
                    sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบทั้งสิ้น {result.Cases}");
                    sb.AppendLine($"💀 จำนวนผู้เสียชีวิตวันนี้ {result.TodayDeaths}");
                    sb.AppendLine($"💀 จำนวนผู้เสียชีวิตทั้งสิ้น {result.Deaths}");
                    sb.AppendLine($"🏨 จำนวนผู้ป่วยอาการวิกฤตทั้งสิ้น {result.Critical}");
                    sb.AppendLine($"🏨 จำนวนผู้ป่วยที่กำลังรักษาตัว {result.Active}");
                    sb.AppendLine($"👌 จำนวนผู้ป่วยที่รักษาหายแล้วทั้งสิ้น {result.Recovered}");
                }
                var embed = new DiscordEmbedBuilder()
                {
                    Title = title,
                    Description = sb.ToString()
                };
                await context.RespondAsync("", embed: embed);
            }
            catch (Exception ex)
            {
                await context.RespondAsync(ex.ToString());
            }
        }
        private static Image Resize(Image img, int outputWidth, int outputHeight)
        {

            if (img == null || (img.Width == outputWidth && img.Height == outputHeight)) return img;
            Bitmap outputImage;
            Graphics graphics;
            try
            {
                outputImage = new Bitmap(outputWidth, outputHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                graphics = Graphics.FromImage(outputImage);
                graphics.DrawImage(img, new Rectangle(0, 0, outputWidth, outputHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                return outputImage;
            }
            catch
            {
                throw;
            }
        }
    }
}

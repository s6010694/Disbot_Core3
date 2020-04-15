using Disbot.Configurations;
using Disbot.Extensions;
using Disbot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using LinqToTwitter;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private static bool cancellationActivate;
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
                    cancellationActivate = false;
                    foreach (var message in messages)
                    {
                        if (cancellationActivate) break;
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
        [Command("stop")]
        public async Task Stop(CommandContext context)
        {
            if (cancellationActivate)
            {
                return;
            }
            cancellationActivate = true;
            await context.RespondAsync("ยกเลิกการลบข้อความแล้ว");
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
                var content = new DiscordEmbedBuilder()
                {
                    Title = $"10 อันดับ trends ยอดนิยมขณะนี้คือ",
                    Description = string.Join("\n", topTrends)
                };
                await context.RespondAsync("", embed: content);
            }
            catch (Exception ex)
            {
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog(nameof(News), ex));
                await context.RespondAsync("เกิดข้อผิดพลาดในการทำงานขึ้น แต่ได้บันทึกข้อผิดพลาดไว้สำหรับตรวจสอบแก้ไขแล้ว");
            }
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
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog("covid", ex));
            }
        }
        [Command("info")]
        public async Task Information(CommandContext context)
        {
            var compiledDate = Assembly.GetExecutingAssembly().GetCreationTime();
            var compiledDateStr = compiledDate.ToString("yyyy.MM.dd");
            var info = new DiscordEmbedBuilder()
            {
                Title = "Server Information",
                Description =
                $"Exp rate : x{AppConfiguration.Content.GetUpdatedMultiplyRate()}\n" +
                $"Spotify API Available : {(SpotifyConfiguration.Context.IsAvailable ? "Yes" : "No")}\n" +
                $"Commands Available : Yes",
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Created by Exception, v.{compiledDateStr}"
                }
            };
            await context.RespondAsync("", false, info);
        }
        [Command("rate")]
        [RequireOwner]
        public async Task SetRate(CommandContext context, float rate)
        {
            var currentRate = AppConfiguration.Content.GetUpdatedMultiplyRate();
            AppConfiguration.Content.SetMultiplyRate(rate);
            await context.RespondAsync($"ดำเนินการปรับอัตราคูณ Exp จาก {currentRate * 100}% เป็น {rate}% แล้ว");
        }
        [Command("getfile")]
        [RequireOwner]
        public async Task GetDatabase(CommandContext context, string fileName)
        {
            try
            {
                var file = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fileName);
                if (File.Exists(file))
                {
                    var copyFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "copy_of_" + fileName);
                    File.Copy(file, copyFileName, true);
                    var ownerChannel = await context.Member.CreateDmChannelAsync();
                    await ownerChannel.SendFileAsync(copyFileName);
                    File.Delete(copyFileName);
                }
            }
            catch
            {
                await context.RespondAsync($"ไฟล์นี้กำลังถูกใช้งานอยู่ในขณะนี้ ไม่สามารถส่งมาให้คุณได้!");
            }
        }
    }
}

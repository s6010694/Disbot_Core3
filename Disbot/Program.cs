using Disbot.AI;
using Disbot.Classes;
using Disbot.Configurations;
using Disbot.Connector;
using Disbot.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Disbot.Marshal.ConsoleEvent;

namespace Disbot
{
    class Program
    {
        private static readonly SentimentClassifier.SentimentClassifier Classifier = new SentimentClassifier.SentimentClassifier();
        public static DiscordClient DiscordClient { get; private set; }
        private static CommandsNextModule Commands { get; set; }
        private static VoiceNextClient Voice { get; set; }
        private static ConsoleEventDelegate handler;
        static async Task Main(string[] args)
        {
            //AdDICT.Container.Register<SentimentClassifier.SentimentClassifier>();
            // Task.Run(ActionListener);
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            await InitializeDiscordConnector();
            await Task.Delay(-1);
        }

        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                DiscordClient.DisconnectAsync();
            }
            return false;
        }

        static void ActionListener()
        {
            while (true)
            {
                var action = Console.ReadKey().Key;
                switch (action)
                {
                    case ConsoleKey.H:
                        break;
                    case ConsoleKey.O:
                        var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = dir,
                            UseShellExecute = true
                        });
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
                //await Task.Delay(100);
            }
        }
        private static async Task InitializeDiscordConnector()
        {
            DiscordClient = new DiscordClient(new DiscordConfiguration()
            {
                Token = AppConfiguration.Content.Discord.AccessToken,
                TokenType = TokenType.Bot,
                LogLevel = AppConfiguration.Content.LogLevel,
                UseInternalLogHandler = true,
                AutoReconnect = true
            });
            DiscordClient.ClientErrored += Discord_ClientErrored;
            DiscordClient.VoiceStateUpdated += Discord_VoiceStateUpdated;
            DiscordClient.Ready += Discord_Ready;
            DiscordClient.Heartbeated += Discord_HeartBeated;
            DiscordClient.MessageCreated += Discord_MessageCreated;
            DiscordClient.UserUpdated += Discord_UserUpdated;
            DiscordClient.GuildAvailable += Discord_GuildAvailable;
            DiscordClient.MessageReactionAdded += Discord_ReactionAdded;
            DiscordClient.MessageReactionRemoved += Discord_ReactionRemoved;
            Commands = DiscordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = AppConfiguration.Content.CommandPrefix,
                EnableDms = false,
                EnableMentionPrefix = true
            });
            Commands.RegisterCommands<Commands>();
            Commands.CommandErrored += OnCommandErrored;
            Voice = DiscordClient.UseVoiceNext(new VoiceNextConfiguration()
            {
                VoiceApplication = DSharpPlus.VoiceNext.Codec.VoiceApplication.Music
            });
            await DiscordClient.ConnectAsync();
        }
        private static async Task Discord_ReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            var author = message.Author;
            if (author.Id == e.User.Id)
            {
                return;
            }
            var dbUser = Service.Context.Member.FirstOrDefault(x => x.ID == (long)author.Id);
            if (dbUser != null)
            {
                var exp = 1;
                dbUser.Exp -= exp;
                Service.Context.Member.Update(dbUser);
            }
        }
        private static async Task Discord_ReactionAdded(MessageReactionAddEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            var author = message.Author;
            if (author.Id == e.User.Id)
            {
                return;
            }
            var dbUser = Service.Context.Member.FirstOrDefault(x => x.ID == (long)author.Id);
            if (dbUser != null)
            {
                var exp = 1;
                dbUser.Exp += exp;
                Service.Context.Member.Update(dbUser);
                await channel.SendDisposableMessageAsync($"{author.Mention} ได้รับ {exp} EXP จากการถูก reaction ข้อความ!");
            }
        }

        private static Task Discord_GuildAvailable(GuildCreateEventArgs e)
        {
            //var test = Voice.GetConnection(e.Guild);
            return Task.CompletedTask;
        }

        private static async Task OnCommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is DSharpPlus.CommandsNext.Exceptions.ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }

        private static async Task Discord_VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
#if DEBUG
            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("VoiceServerUpdated");
#endif
        }

        private static async Task Discord_PresenceUpdated(PresenceUpdateEventArgs e)
        {
#if DEBUG

            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("PresenceUpdated");
#endif
        }

        private static async Task Discord_GuildUpdated(ChannelUpdateEventArgs e)
        {
#if DEBUG

            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("GuildUpdated");
#endif
        }

        private static async Task Discord_ChannelUpdated(ChannelUpdateEventArgs e)
        {
#if DEBUG

            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("ChannelUpdated");
#endif
        }

        private static async Task Discord_UserSettingsUpdated(UserSettingsUpdateEventArgs e)
        {
#if DEBUG

            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("UserSettingsUpdated");
#endif
        }

        private static async Task Discord_UserUpdated(UserUpdateEventArgs e)
        {
#if DEBUG

            var ch = await DiscordClient.GetDefaultChannelAsync();
            await ch.SendMessageAsync("UserUpdated");
#endif
        }

        private static async Task Discord_ClientErrored(ClientErrorEventArgs e)
        {
            await DiscordClient.ReconnectAsync();
            await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog(nameof(Discord_ClientErrored), e.Exception));
        }

        private static async Task Discord_MessageCreated(MessageCreateEventArgs e)
        {
            try
            {
                var message = e.Message;
                var ch = await DiscordClient.GetDefaultChannelAsync();
                if (!message.Content.StartsWith(AppConfiguration.Content.CommandPrefix))
                {
                    //var ssense = await NectecService.CallSSenseService(message.Content);
                    var prediction = Classifier.Predict(message.Content);
                    if (prediction?.ClassifyLabel == SentimentClassifier.Enum.SentimentClassficationResult.Negative)
                    {
                        await ch.SendDisposableMessageAsync($"สุภาพหน่อยขอรับ {message.Author.Mention}!");
                        await ch.SendDisposableFileAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.Combine("Assets", "language.jpg")));
                    }
                }
                await Service.Context.MessageHistory.InsertMessageAsync(message);
                if (e.Author.Id != DiscordClient.CurrentUser.Id && Service.Context.Member.CalculateIsLevelUp((long)e.Author.Id, out var level))
                {
                    PlayLevelupSound(e.Guild);
                    await ch.SendMessageAsync($"🎉🎉🎉 🥂{e.Author.Mention}🥂 ได้อัพเลเวลเป็น {level}! 🎉🎉🎉 ");
                    var avatarPath = Etc.MemberEtc.GetLevelupAvatar(e.Author.AvatarUrl, level);
                    await ch.SendFileAsync(avatarPath);
                }
                Console.WriteLine($"[MessageCreated] : {message.Content}");
            }
            catch (Exception ex)
            {
                await Service.Context.ExceptionLog.InsertAsync(new Models.ExceptionLog(nameof(Discord_MessageCreated), ex));
            }
        }

        private static async Task Discord_HeartBeated(HeartbeatEventArgs e)
        {

        }

        private static async Task Discord_Ready(ReadyEventArgs e)
        {

            var guild = e.Client.Guilds.First().Value;
            var existingUsers = Service.Context.Member.Select(x => x.ID).ToArray();
            var users = await guild.GetAllMembersAsync();
            var newUsers = users.Where(x => !existingUsers.Contains((long)x.Id));
            Service.Context.Member.InsertDiscordMember(newUsers);
            var channel = await DiscordClient.GetDefaultChannelAsync();
            await DiscordClient.UpdateStatusAsync(DiscordGameExtension.GetRandomActivity());
            //VoiceNextClient vnext = DiscordClient.GetVoiceNextClient();
            //await vnext.ConnectAsync(await DiscordClient.GetDefaultVoiceChannelAsync());
            await channel.SendDisposableMessageAsync($@"{e.Client.CurrentUser.Mention} พร้อมรับใช้ขอรับ");

        }
        private static readonly List<ulong> currentlyOnVoiceChannelUsers = new List<ulong>();
        private static async Task Discord_VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            try
            {
                if (e.Channel == null)
                {
                    currentlyOnVoiceChannelUsers.Remove(e.User.Id);
                    var ch = await DiscordClient.GetDefaultChannelAsync();
                    await ch.SendDisposableMessageAsync($"{e.User.Mention} ออกจากห้องแล้ว");
                }
                else if (e.Channel != null && !currentlyOnVoiceChannelUsers.Contains(e.User.Id))
                {
                    currentlyOnVoiceChannelUsers.Add(e.User.Id);
                    var ch = await DiscordClient.GetDefaultChannelAsync();
                    var voiceCh = ch.Guild.Channels.FirstOrDefault(x => x.Type == ChannelType.Voice && x.Id == AppConfiguration.Content.Discord.ActiveVoiceChannelId);
                    if (e.Channel?.Id == voiceCh?.Id)
                    {
                        {
                            await ch.SendDisposableMessageAsync($@"ยินดีต้อนรับกลับมาขอรับ {e.User.Mention}!");
                        }
                    }
                }
                if (currentlyOnVoiceChannelUsers.Count > 0)
                {
                    await DiscordClient.UpdateStatusAsync(new DiscordGame()
                    {
                        Name = $"กำลังดูแลแขก {currentlyOnVoiceChannelUsers.Count} ท่านในขณะนี้"
                    }); ;
                }
                else
                {
                    await DiscordClient.UpdateStatusAsync(DiscordGameExtension.GetRandomActivity());
                }
                Console.WriteLine($"[VoiceState] : Trigger by {e.User.Id}");
            }
            catch (Exception ex)
            {
                Service.Context.ExceptionLog.Insert(new Models.ExceptionLog(nameof(Discord_VoiceStateUpdated), ex));
            }
        }
        private static async void PlayLevelupSound(DiscordGuild guild)
        {
            //var vnext = DiscordClient.GetVoiceNextClient();
            //var vnc = vnext.GetConnection(guild);
            //await vnc.SendSpeakingAsync(true);
            //await Task.Delay(3000);
            //await vnc.SendSpeakingAsync(false);
        }
    }
}

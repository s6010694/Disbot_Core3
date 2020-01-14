using Disbot.AI;
using Disbot.Configurations;
using Disbot.Connector;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Disbot
{
    class Program
    {
        private static DiscordClient discordConnector { get; set; }
        private static CommandsNextModule commands { get; set; }
        private static VoiceNextClient Voice { get; set; }
        private readonly static SentimentClassifier.SentimentClassifier classifier = new SentimentClassifier.SentimentClassifier();
        static async Task Main(string[] args)
        {
            await InitializeDiscordConnector();
        }

        private static async Task InitializeDiscordConnector()
        {
            discordConnector = new DiscordClient(new DiscordConfiguration()
            {
                Token = AppConfiguration.Content.Discord.AccessToken,
                TokenType = TokenType.Bot,
                LogLevel = AppConfiguration.Content.LogLevel,
                UseInternalLogHandler = true
            });
            discordConnector.ClientErrored += Discord_ClientErrored;
            discordConnector.VoiceStateUpdated += Discord_VoiceStateUpdated;
            discordConnector.Ready += Discord_Ready;
            discordConnector.Heartbeated += Discord_HeartBeated;
            discordConnector.MessageCreated += Discord_MessageCreated;

            commands = discordConnector.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = AppConfiguration.Content.CommandPrefix
            });
            commands.RegisterCommands<Commands>();

            var vnextCfg = new VoiceNextConfiguration()
            {
                VoiceApplication = DSharpPlus.VoiceNext.Codec.VoiceApplication.Music
            };
            Voice = discordConnector.UseVoiceNext(vnextCfg);
            await discordConnector.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task Discord_ClientErrored(ClientErrorEventArgs e)
        {
            Console.WriteLine("Some connection error happenned, reconnecting...");
            await discordConnector.ReconnectAsync();
        }

        private static async Task Discord_MessageCreated(MessageCreateEventArgs e)
        {
            try
            {
                var message = e.Message;
                if (!message.Content.StartsWith(AppConfiguration.Content.CommandPrefix))
                {
                    //var ssense = await NectecService.CallSSenseService(message.Content);
                    //if (ssense != null && ssense.IsNegative())
                    var prediction = classifier.Predict(message.Content);
                    if (prediction.ClassifyLabel == SentimentClassifier.Enum.SentimentClassficationResult.Negative)
                    {
                        var ch = await discordConnector.GetChannelAsync(AppConfiguration.Content.Discord.UsersChannelID);

                        await ch.SendMessageAsync("สุภาพหน่อย!!");
                        await ch.SendMessageAsync("https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTpQsmwswjh_mYmeklbtFhPJpfBHSP32FzGHBmEopst9YWX43t1");
                    }
                }
                await Service.Context.MessageHistory.InsertMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            //var users = await e.Guild.GetAllMembersAsync();
            //Service.Context.Member.InsertDiscordMember(users);
            //throw new NotImplementedException();
        }

        private static async Task Discord_HeartBeated(HeartbeatEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static async Task Discord_Ready(ReadyEventArgs e)
        {

            var guild = e.Client.Guilds.First().Value;
            var existingUsers = Service.Context.Member.Select(x => x.ID).ToArray();
            var users = await guild.GetAllMembersAsync();
            var newUsers = users.Where(x => !existingUsers.Contains(x.Id));
            Service.Context.Member.InsertDiscordMember(newUsers);

        }

        private static async Task Discord_VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}

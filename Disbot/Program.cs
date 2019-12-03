using Disbot.Configurations;
using Disbot.Connector;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Disbot
{
    class Program
    {
        private static DiscordClient discordConnector { get; set; }
        private static CommandsNextModule commands { get; set; }
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
            //commands = discordConnector.UseCommandsNext(new CommandsNextConfiguration
            //{
            //    StringPrefix = AppConfiguration.Content.CommandPrefix
            //});
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
            var message = e.Message;
            await Service.Context.MessageHistory.InsertMessageAsync(message);
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
            //throw new NotImplementedException();
            //var ch = await discordConnector.GetChannelAsync(AppConfiguration.Content.Discord.UsersChannelID);
            //await ch.SendMessageAsync("I'll be back.");
        }

        private static async Task Discord_VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}

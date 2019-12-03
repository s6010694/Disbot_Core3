using Disbot.Models;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace Disbot.Repositories
{
    /// <summary>
    /// Data contractor for MessageHistory
    /// </summary>
    public partial class MessageHistoryRepository
    {
        public async Task InsertMessageAsync(DiscordMessage message)
        {
            var dbMessage = new MessageHistory();
            dbMessage.MemberID = message.Author.Id;
            dbMessage.Message = message.Content;
            dbMessage.CreateDate = DateTime.Now;
            await this.InsertAsync(dbMessage);
        }
    }
}

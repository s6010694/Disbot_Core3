using Disbot.Repositories.Components;
using Disbot.Models;
using System.Data.SQLite;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Disbot.Repositories
{
    /// <summary>
    /// Data contractor for Member
    /// </summary>
    public partial class MemberRepository
    {
        public void InsertDiscordMember(DiscordMember member)
        {
            this.Insert(new Member()
            {
                ID = member.Id,
                Username = member.Username,
                DisplayName = member.DisplayName,
                AvatarUrl = member.AvatarUrl
            });
        }
        public void InsertDiscordMember(IEnumerable<DiscordMember> members)
        {
            var ms = members.Select(member => new Member()
            {
                ID = member.Id,
                Username = member.Username,
                DisplayName = member.DisplayName,
                AvatarUrl = member.AvatarUrl
            });
            this.InsertMultiple(ms);
        }
    }
}

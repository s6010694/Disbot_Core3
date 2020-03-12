using Disbot.Configurations;
using Disbot.Models;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                ID = (long)member.Id,
                Username = member.Username,
                DisplayName = member.DisplayName,
                AvatarUrl = member.AvatarUrl
            });
        }
        public void InsertDiscordMember(IEnumerable<DiscordMember> members)
        {
            var ms = members.Select(member => new Member()
            {
                ID = (long)member.Id,
                Username = member.Username,
                DisplayName = member.DisplayName,
                AvatarUrl = member.AvatarUrl
            });
            this.InsertMany(ms);
        }

        internal bool CalculateIsLevelUp(long id, out uint level)
        {
            level = 0;
            var member = this.Query(x => x.ID == id).First();
            member.Exp += 1 * AppConfiguration.Content.GetUpdatedMultiplyRate();
            var nextExp = member.NextExp;
            bool levelUp = false;
            while (member.Exp >= nextExp)
            {
                var x = member.Level += 1;
                //nextExp = Math.Round(x * Math.Sin(x) + (Math.Sqrt(x) * x * 1));//round(x * sin(x) +  (sqrt(x) * x * c))
                nextExp = (float)Math.Round(x * Math.Sin(x) + Math.Pow(x, 1.486)); //ROUND(x*SIN(x) + POWER(x,c),0), lower required exp by 5% compared to above equation at constant = 1
                member.Exp = 0;
                member.NextExp = nextExp;
                level = member.Level;
                levelUp = true;
            }
            this.Update(member);
            return levelUp;
            //var currentLevel = member.Level;
            //var totalPosts = this.service.MessageHistory.Count(x => x.MemberID == id) * AppConfiguration.Content.GetUpdatedMultiplyRate();
            //var requiredPosts = Math.Pow(currentLevel, 2);
            //var shouldLevelup = totalPosts > requiredPosts;
            //level = 0;
            //if (shouldLevelup)
            //{
            //    member.Level += 1;
            //    this.Update(member);
            //    level = member.Level;
            //}
            //return shouldLevelup;
        }
    }
}

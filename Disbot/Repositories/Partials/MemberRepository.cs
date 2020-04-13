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

        internal bool CalculateIsLevelUp(long id, out int level)
        {
            level = 0;
            var member = this.Query(x => x.ID == id).First();
            member.Exp += 1 * AppConfiguration.Content.GetUpdatedMultiplyRate();
            var exp = member.Exp;
            var nextExp = member.NextExp;
            bool levelUp = false;
            while (member.Exp >= nextExp)
            {
                var x = member.Level += 1;
                var c = 1.115;
                //nextExp = Math.Round(x * Math.Sin(x) + (Math.Sqrt(x) * x * 1));//round(x * sin(x) +  (sqrt(x) * x * c))
                //nextExp = (float)Math.Round(x * Math.Sin(x) + Math.Pow(x, 1.486)); //ROUND(x*SIN(x) + POWER(x,c),0), lower required exp by 5% compared to above equation at constant = 1
                //ROUND(A2  + POWER(A2/2,1.1611) * SQRT(A2),0) / ROUND(A2 + POWER(A2/ 2, 1.115) * SQRT(A2), 0)
                nextExp = (float)Math.Round(x + Math.Pow(x / 2, c) * Math.Sqrt(x), 0); //much much linear compare to x sin x'ish and reflect how "leveling" should be, adjusting c for fancy graph curve.
                member.Exp = nextExp - member.Exp;
                member.NextExp = nextExp;
                level = member.Level;
                levelUp = true;
                exp -= nextExp;
            }
            this.Update(member);
            return levelUp;
        }
        internal void RecalcLevel()
        {
            var sql = $@"SELECT Member.Id As Id,COUNT(MessageHistory.MemberID) as Exp
                         FROM
                         	Member
                         JOIN
                         	MessageHistory
                         ON 
                         	Member.ID = MessageHistory.MemberID
                         GROUP BY Member.ID";
            var res1 = this.Connector.ExecuteReader(sql);
            foreach (var item in res1)
            {
                var id = (long)item.Id;
                var exp = (float)item.Exp;
                var user = this.Service.Member.Query(x => x.ID == id).FirstOrDefault();
                user.Level = 1;
                user.NextExp = 1;
                user.Exp = exp;
                this.Service.Member.Update(user);
                CalculateIsLevelUp(id, out var _);
            }

        }
    }
}

using Utilities.Interfaces;
using Disbot.Repositories.Components;
using Disbot.Models;

namespace Disbot.Repositories
{
    /// <summary>
    /// Data contractor for Member
    /// </summary>
    public partial class MemberRepository : Repository<Member,System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter>
    {
       public MemberRepository(IDatabaseConnectorExtension<System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter> connector) : base(connector)
       {
       }
    }
}

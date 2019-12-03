using Utilities.Interfaces;
using Disbot.Repositories.Components;
using Disbot.Models;

namespace Disbot.Repositories
{
    /// <summary>
    /// Data contractor for MessageHistory
    /// </summary>
    public partial class MessageHistoryRepository : Repository<MessageHistory,System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter>
    {
       public MessageHistoryRepository(IDatabaseConnectorExtension<System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter> connector) : base(connector)
       {
       }
    }
}

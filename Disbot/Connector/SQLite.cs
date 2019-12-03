using System.Data.SQLite;
using Utilities.SQL;

namespace Disbot.Connector
{
    public class SQLite : DatabaseConnector<SQLiteConnection, SQLiteParameter>
    {
        private static string ConnectionString = @"Data Source=disbot.db;Version=3;";
        private static string InMemoryDb = @"Data Source=:memory:;version=3;New=True;";
        public SQLite() : base(ConnectionString)
        {
            SQLFunctionConfiguration.Add(Utilities.Enum.SqlFunction.Length, "LENGTH");
        }
        public SQLite(string connectionString) : base(connectionString)
        {
            SQLFunctionConfiguration.Add(Utilities.Enum.SqlFunction.Length, "LENGTH");
        }
    }
}

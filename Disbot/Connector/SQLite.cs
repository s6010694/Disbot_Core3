using System;
using System.Data.SQLite;
using Utilities.Enum;
using Utilities.SQL;

namespace Disbot.Connector
{
    public class SQLite : DatabaseConnector
    {
        private static string ConnectionString = @"Data Source=disbot.db;Version=3;";
        private static string InMemoryDb = @"Data Source=:memory:;version=3;New=True;";
        public SQLite() : base(typeof(SQLiteConnection), ConnectionString)
        {

        }
        public SQLite(string connectionString) : base(typeof(SQLiteConnection), connectionString)
        {

        }
        protected override string CompatibleFunctionName(SqlFunction function)
        {
            switch (function)
            {
                case SqlFunction.Length:
                    return "LENGTH";
            }
            return base.CompatibleFunctionName(function);
        }
        protected override string CompatibleSQLType(Type type)
        {
            if (type == typeof(string))
            {
                return "TEXT";
            }
            else if (type == typeof(char) || type == typeof(char?))
            {
                return "TEXT";
            }
            else if (type == typeof(short) || type == typeof(short?) || type == typeof(ushort) || type == typeof(ushort?))
            {
                return "INTEGER";
            }
            else if (type == typeof(int) || type == typeof(int?) || type == typeof(uint) || type == typeof(uint?))
            {
                return "INTEGER";
            }
            else if (type == typeof(long) || type == typeof(long?) || type == typeof(ulong) || type == typeof(ulong?))
            {
                return "INTEGER";
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                return "REAL";
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return "REAL";
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                return "INTEGER";
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return "REAL";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return "TEXT";
            }
            else if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return "TEXT";
            }
            else if (type == typeof(byte) || type == typeof(byte?) || type == typeof(sbyte) || type == typeof(sbyte?))
            {
                return "INTEGER";
            }
            else if (type == typeof(byte[]))
            {
                return "BLOB";
            }
            return base.CompatibleSQLType(type);

        }
    }
}

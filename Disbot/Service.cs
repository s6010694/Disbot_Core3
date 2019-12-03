using System;
using Utilities.SQL;
using Utilities.Interfaces;
using Disbot.Repositories;
using System.Data.SQLite;
using Disbot.Configurations;

namespace Disbot
{
    public sealed class Service : IDisposable
    {
        public static Lazy<Service> _deferredService = new Lazy<Service>(() => new Service(AppConfiguration.Content.SQLiteConnectionString), true);
        public static Service Context => _deferredService.Value;
        private readonly IDatabaseConnectorExtension<SQLiteConnection, SQLiteParameter> Connector;
        Service(string connectionString)
        {
            Connector = new DatabaseConnector<SQLiteConnection, SQLiteParameter>(connectionString);
        }
        private MemberRepository _Member { get; set; }
         /// <summary>
        /// Data repository for Member table
         /// </summary>
        public MemberRepository Member
        {
            get
            {
                if(_Member == null)
                {
                    _Member = new MemberRepository(Connector);
                }
                return _Member;
            }
        }
        private MessageHistoryRepository _MessageHistory { get; set; }
         /// <summary>
        /// Data repository for MessageHistory table
         /// </summary>
        public MessageHistoryRepository MessageHistory
        {
            get
            {
                if(_MessageHistory == null)
                {
                    _MessageHistory = new MessageHistoryRepository(Connector);
                }
                return _MessageHistory;
            }
        }
            public void Dispose()
            {
                Connector?.Dispose();
            }
#region Stored Procedure
#endregion
    }
}

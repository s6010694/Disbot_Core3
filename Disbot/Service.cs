using System;
using Utilities.SQL;
using Utilities.Interfaces;
using Disbot.Repositories;
using System.Data.SQLite;
using Disbot.Configurations;
using Disbot.Repositories.Components;
using System.Collections;
using Disbot.Models;

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
            ServiceCheckUp();
        }

        private void ServiceCheckUp()
        {
            var memberTableCheck = this.Connector.ExecuteScalar($"SELECT name FROM sqlite_master WHERE type='table' AND name='Member';");
            if (memberTableCheck == null)
            {
                this.Connector.CreateTable<Member>();
            }
            var messageHistoryTableCheck = this.Connector.ExecuteScalar($"SELECT name FROM sqlite_master WHERE type='table' AND name='MessageHistory';");
            if (messageHistoryTableCheck == null)
            {
                this.Connector.CreateTable<MessageHistory>();
            }
        }

        private MemberRepository _Member { get; set; }
        /// <summary>
        /// Data repository for Member table
        /// </summary>
        public MemberRepository Member
        {
            get
            {
                if (_Member == null)
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
                if (_MessageHistory == null)
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

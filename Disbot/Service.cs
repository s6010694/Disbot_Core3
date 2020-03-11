using System;
using Utilities.SQL;
using Utilities.Interfaces;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Disbot.Repositories;
using Disbot.Configurations;
using Utilities.SQL.Extension;
using Disbot.Models;

namespace Disbot
{
    public sealed class Service : IDisposable
    {
        public static Lazy<Service> _deferredService = new Lazy<Service>(() => new Service(AppConfiguration.Content.SQLiteConnectionString), true);
        public static Service Context => _deferredService.Value;
        internal protected readonly IDatabaseConnector Connector;
        private Service(string connectionString)
        {
            Connector = new DatabaseConnector(typeof(System.Data.SQLite.SQLiteConnection), connectionString);
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
            var exceptionLogTableCheck = this.Connector.ExecuteScalar($"SELECT name FROM sqlite_master WHERE type='table' AND name='ExceptionLog'");
            if (exceptionLogTableCheck == null)
            {
                this.Connector.CreateTable<ExceptionLog>();
            }
        }
        private MemberRepository _Member { get; set; }
        public MemberRepository Member
        {
            get
            {
                if (_Member == null)
                {
                    _Member = new MemberRepository(this);
                }
                return _Member;
            }
        }
        private MessageHistoryRepository _MessageHistory { get; set; }
        public MessageHistoryRepository MessageHistory
        {
            get
            {
                if (_MessageHistory == null)
                {
                    _MessageHistory = new MessageHistoryRepository(this);
                }
                return _MessageHistory;
            }
        }
        private ExceptionLogRepository _ExceptionLog { get; set; }
        public ExceptionLogRepository ExceptionLog
        {
            get
            {
                if (_ExceptionLog == null)
                {
                    _ExceptionLog = new ExceptionLogRepository(this);
                }
                return _ExceptionLog;
            }
        }
        public void Dispose()
        {
            Connector?.Dispose();
        }
    }
}

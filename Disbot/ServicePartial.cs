using Disbot.Configurations;
using Disbot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities.SQL;
using Utilities.SQL.Extension;

namespace Disbot
{
    public partial class Service
    {
        public static Lazy<Service> _deferredService = new Lazy<Service>(() => new Service(AppConfiguration.Content.SQLiteConnectionString), true);
        public static Service Context => _deferredService.Value;
        private Service(string connectionString)
        {
            this.Connector = new DatabaseConnector(typeof(System.Data.SQLite.SQLiteConnection), connectionString);
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
    }
}

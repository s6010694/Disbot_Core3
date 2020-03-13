using System;
using Utilities.SQL;
using Utilities.Interfaces;
using Disbot.Repositories;

namespace Disbot
{
	public partial class Service : IDisposable
	{
		internal protected readonly IDatabaseConnector Connector;
		private MessageHistoryRepository _MessageHistory { get; set; }
		public MessageHistoryRepository MessageHistory
		{
			get
			{
				if(_MessageHistory == null)
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
				if(_ExceptionLog == null)
				{
					_ExceptionLog = new ExceptionLogRepository(this);
				}
				return _ExceptionLog;
			}
		}
		private MemberRepository _Member { get; set; }
		public MemberRepository Member
		{
			get
			{
				if(_Member == null)
				{
					_Member = new MemberRepository(this);
				}
				return _Member;
			}
		}
        public void Dispose()
        {
            Connector?.Dispose();
        }
	}
}

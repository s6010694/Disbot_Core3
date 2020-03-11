using Disbot.Models;
using Disbot.Repositories.Based;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities.Interfaces;

namespace Disbot.Repositories
{
    public class ExceptionLogRepository : Repository<ExceptionLog>
    {
        private readonly Service service;
        public ExceptionLogRepository(Service service) : base(service.Connector)
        {
            this.service = service;
        }
    }
}

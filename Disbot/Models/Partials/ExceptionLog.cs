using System;
namespace Disbot.Models
{
    public partial class ExceptionLog
    {
        public ExceptionLog()
        {

        }
        public ExceptionLog(string functionName, Exception ex)
        {
            this.Function = functionName;
            this.Content = ex.ToString();
            this.Date = DateTime.Now;
        }
    }
}


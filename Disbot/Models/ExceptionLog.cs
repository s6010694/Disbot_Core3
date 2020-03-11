using System;
using System.Collections.Generic;
using System.Text;

namespace Disbot.Models
{
    public class ExceptionLog
    {
        public string Function { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public ExceptionLog() { }
        public ExceptionLog(string function, string content)
        {
            this.Function = function;
            this.Content = content;
            Date = DateTime.Now;
        }
        public ExceptionLog(string function, Exception exception)
        {
            this.Function = function;
            this.Content = exception.ToString();
            this.Date = DateTime.Now;
        }
    }
}

using System;

namespace Disbot.Models
{
    //You can get Utilities package via nuget : Install-Package Deszolate.Utilities.Lite
    //[Utilities.Attributes.SQL.Table("MessageHistory")]
    public partial class MessageHistory
    {
        public ulong MemberID { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
    }
}

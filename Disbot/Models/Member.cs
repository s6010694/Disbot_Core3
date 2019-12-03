using System;

namespace Disbot.Models
{
    //You can get Utilities package via nuget : Install-Package Deszolate.Utilities.Lite
    //[Utilities.Attributes.SQL.Table("Member")]
    public partial class Member
    {
        public ulong ID { get; set; }
        //[Utilities.Attributes.SQL.PrimaryKey]
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
    }
}

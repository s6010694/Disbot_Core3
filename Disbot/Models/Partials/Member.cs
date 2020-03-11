using System;

namespace Disbot.Models
{
    //You can get Utilities package via nuget : Install-Package Deszolate.Utilities.Lite
    //[Utilities.Attributes.SQL.Table("Member")]
    public partial class Member
    {
        public void LevelUp()
        {
            this.Level += 1;
        }
    }
}

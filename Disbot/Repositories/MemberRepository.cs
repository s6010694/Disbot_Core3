// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a ModelGenerator.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
using Utilities.Interfaces;
using Disbot.Repositories.Based;
using Disbot.Models;
namespace Disbot.Repositories
{
	///<summary>
	/// Data contractor for Member
	///</summary>
	public partial class MemberRepository : Repository<Member>
	{
		private readonly Service service;
		public MemberRepository(Service service) : base(service.Connector)
		{
			this.service = service;
		}
	}
}


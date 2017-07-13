using FluentNHibernate.Mapping;
using SilkServer.NHibernate.Models;

namespace SilkServer.NHibernate.Maps
{
	public class AccountMap : ClassMap<Account>
	{
		public AccountMap()
		{
			Id(x => x.Id).Column("id");

			Map(x => x.Username).Column("username");
			Map(x => x.Email).Column("email");
			Map(x => x.Password).Column("password");
			Map(x => x.Salt).Column("salt");

			Map(x => x.Created).Column("created_at");
			Map(x => x.Updated).Column("updated_at");

			Table("accounts");
		}
	}
}

using FluentNHibernate.Mapping;
using SilkServer.SubServer.NHibernate.Models;

namespace SilkServer.SubServer.NHibernate.Maps
{
	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			Id(x => x.Id).Column("id");

			Map(x => x.Username).Column("username");
			Map(x => x.Email).Column("email");
			Map(x => x.CharacterType).Column("character_type");
			Map(x => x.Money).Column("money");
			Map(x => x.Password).Column("password");
			Map(x => x.Salt).Column("salt");

			Map(x => x.Created).Column("created_at");
			Map(x => x.Updated).Column("updated_at");

			Table("user");
		}
	}
}

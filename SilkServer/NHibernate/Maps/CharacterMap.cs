using FluentNHibernate.Mapping;
using SilkServer.NHibernate.Models;

namespace SilkServer.NHibernate.Maps
{
	public class CharacterMap : ClassMap<Character>
	{
		public CharacterMap()
		{
			Id(x => x.Id).Column("id");
			References(x => x.Account).Column("account_id");

			Map(x => x.CharacterType).Column("character_type");
			Map(x => x.Money).Column("money");
			Map(x => x.Exp).Column("exp");
			Map(x => x.Wins).Column("wins");
			Map(x => x.Defeats).Column("defeats");
			Map(x => x.Kills).Column("kills");
			Map(x => x.Deaths).Column("deaths");

			Map(x => x.Created).Column("created_at");
			Map(x => x.Updated).Column("updated_at");

			Table("characters");
		}
	}
}

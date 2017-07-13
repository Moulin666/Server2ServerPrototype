using System;

namespace SilkServer.NHibernate.Models
{
	public class Character
	{
		public virtual int Id { get; set; }
		public virtual Account Account { get; set; }

		public virtual int CharacterType { get; set; }
		public virtual int Money { get; set; }
		public virtual int Exp { get; set; }
		public virtual int Wins { get; set; }
		public virtual int Defeats { get; set; }
		public virtual int Kills { get; set; }
		public virtual int Deaths { get; set; }

		public virtual DateTime Created { get; set; }
		public virtual DateTime Updated { get; set; }
	}
}

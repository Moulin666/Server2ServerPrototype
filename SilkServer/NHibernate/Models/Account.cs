using System;

namespace SilkServer.NHibernate.Models
{
	public class Account
	{
		public virtual int Id { get; set; }

		public virtual string Username { get; set; }
		public virtual string Email { get; set; }
		public virtual string Password { get; set; }
		public virtual string Salt { get; set; }

		public virtual DateTime Created { get; set; }
		public virtual DateTime Updated { get; set; }
	}
}

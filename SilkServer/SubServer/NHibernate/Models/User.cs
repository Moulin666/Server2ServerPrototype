﻿using System;

namespace SilkServer.SubServer.NHibernate.Models
{
	public class User
	{
		public virtual int Id { get; set; }

		public virtual string Username { get; set; }
		public virtual string Password { get; set; }
		public virtual string Salt { get; set; }
		public virtual string Algorithm { get; set; }

		public virtual DateTime Created { get; set; }
		public virtual DateTime Updated { get; set; }
	}
}

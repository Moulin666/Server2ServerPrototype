using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

namespace SilkServer.SubServer.NHibernate
{
	public class NHibernateHelper
	{
		public NHibernateHelper()
		{
			InitializeSessionFactory();
		}

		private static ISessionFactory _sessionFactory;
		private static ISessionFactory SessionFactory
		{
			get
			{
				if (_sessionFactory == null)
				{
					InitializeSessionFactory();
				}

				return _sessionFactory;
			}
		}

		private static void InitializeSessionFactory()
		{
			_sessionFactory = Fluently.Configure()
				.Database(
				MySQLConfiguration.Standard.ConnectionString(cs => cs.Server("localhost")
				.Database("PirateLifeDB").Username("root")))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<MasterServer.MasterServer>())
				.BuildSessionFactory();
		}

		public static ISession OpenSession()
		{
			return SessionFactory.OpenSession();
		}
	}
}

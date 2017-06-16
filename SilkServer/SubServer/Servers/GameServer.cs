using SilkServer.Server2Server;

namespace SilkServer.SubServer.Servers
{
	public class GameServer : SubServer
	{
		#region Constructors and Destructors

		public GameServer()
		{
			ServerType = ServerType.GameServer;
		}

		#endregion

		#region Methods

		protected override void AddHandlersToServerPeer(OutgoingMasterServerPeer serverPeer)
		{
			// todo game server
		}

		#endregion
	}
}

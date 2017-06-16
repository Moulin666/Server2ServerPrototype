using SilkServer.Server2Server;

namespace SilkServer.SubServer.Servers
{
	public class LobbyServer : SubServer
	{
		#region Constructors and Destructors

		public LobbyServer()
		{
			ServerType = ServerType.Lobby;
		}

		#endregion

		#region Methods

		protected override void AddHandlersToServerPeer(OutgoingMasterServerPeer serverPeer)
		{
			// todo lobby server
		}

		#endregion
	}
}

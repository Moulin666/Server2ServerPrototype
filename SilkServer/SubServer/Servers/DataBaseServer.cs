using SilkServer.Server2Server;
using SilkServer.SubServer.Handlers.LoginServer;
using SilkServerCommon;

namespace SilkServer.SubServer.Servers
{
	public class DataBaseServer : SubServer
	{
		#region Constructors and Destructors

		public DataBaseServer()
		{
			ServerType = ServerType.DataBase;
		}

		#endregion

		#region Methods

		protected override void AddHandlersToServerPeer(OutgoingMasterServerPeer serverPeer)
		{
			serverPeer.RequestHandlers.Add((byte)UnitySubOperationCode.LoginSecurely, new LoginRequestHandler(serverPeer));
			serverPeer.RequestHandlers.Add((byte)UnitySubOperationCode.RegisterSecurely, new RegisterRequestHandler(serverPeer));
		}

		#endregion
	}
}

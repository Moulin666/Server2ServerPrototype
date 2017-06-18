using SilkServer.Server2Server;
using SilkServer.SubServer.Handlers.LoginServer;
using SilkServerCommon;

namespace SilkServer.SubServer.Servers
{
	public class LoginServer : SubServer
	{
		#region Constructors and Destructors

		public LoginServer()
		{
			ServerType = ServerType.Login;
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

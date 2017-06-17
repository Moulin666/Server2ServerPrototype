/*
 * Created by
 * roman.khusnetdinov
*/

using ExitGames.Logging;

using SilkServer.Server2Server;
using SilkServerCommon;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

using System;

namespace SilkServer.MasterServer
{
	public class UnityClient : ClientPeer
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		#endregion

		#region Properties

		private MasterServer _server;

		public Guid UserId { get; protected set; }

		#endregion

		#region Constructors and Destructors

		public UnityClient(InitRequest initRequest, MasterServer masterServer) : base(initRequest)
		{
			_server = masterServer;
			UserId = Guid.NewGuid();

			_server.ConnectedClients.Add(UserId, this);

			Log.Info("Added new UnityClient");
		}

		#endregion

		#region Methods

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("OperationRequest received: {0}", operationRequest.OperationCode);
			}

			operationRequest.Parameters.Remove((byte)ParameterCode.UserId);
			operationRequest.Parameters.Add((byte)ParameterCode.UserId, UserId.ToString());

			switch (operationRequest.OperationCode)
			{
				case ((byte)UnityClientCode.Login):
					{
						_server.SubServers.LoginServer.SendOperationRequest(operationRequest, sendParameters);
						break;
					}
				case ((byte)UnityClientCode.Lobby):
					{
						_server.SubServers.LobbyServer.SendOperationRequest(operationRequest, sendParameters);
						break;
					}
				case ((byte)UnityClientCode.Game):
					{
						_server.SubServers.GameServer.SendOperationRequest(operationRequest, sendParameters);
						break;
					}
			}
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			_server.ConnectedClients.Remove(UserId);

			Log.DebugFormat("Disconnected UnityClient - {0}-{1}", reasonCode.ToString(), reasonDetail);
		}

		#endregion
	}
}

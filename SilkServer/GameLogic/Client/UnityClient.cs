/*
 * Created by
 * roman.khusnetdinov
*/

using SilkServer.Server2Server;
using SilkServerCommon;
using SilkServer.GameLogic.WorldSystem;

using ExitGames.Logging;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

using System;

namespace SilkServer.GameLogic.Client
{
	public partial class UnityClient : ClientPeer
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public ClientConnectedStatus ClientConnectedStatus { get; private set; }

		#endregion

		#region Properties

		private MasterServer.MasterServer _server;

		public Guid UserId { get; protected set; }

		#endregion

		#region Constructor

		public UnityClient(InitRequest initRequest, MasterServer.MasterServer masterServer) : base(initRequest)
		{
			_server = masterServer;
			UserId = Guid.NewGuid();

			_server.ConnectedClients.Add(UserId, this);

			SetClientConnectedStatus(ClientConnectedStatus.NotAuth);

			Log.Info("Added new UnityClient");
		}

		#endregion

		#region ClientPeer

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
						var version = (string)operationRequest.Parameters[(byte)UnityParameterCode.GameVersion];
						if (version != "0.2")
						{
							SendOperationResponse(new OperationResponse(operationRequest.OperationCode = (byte)UnitySubOperationCode.LoginSecurely, operationRequest.Parameters)
							{
								ReturnCode = (byte)UnityErrorCode.IncorrectGameVersion,
								DebugMessage = "Incorrect game version"
							}, new SendParameters());

							Disconnect();
							return;
						}

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

			World.Instance.Leave(this);

			Log.DebugFormat("Disconnected UnityClient - {0}-{1}", reasonCode.ToString(), reasonDetail);
		}

		#endregion

		#region Methods

		public void SetClientConnectedStatus(ClientConnectedStatus newStatus)
		{
			ClientConnectedStatus = newStatus;
		}

		#endregion
	}
}

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
				case ((byte)UnityClientCode.DataBase):
					{
						if (CheckVersion((string)operationRequest.Parameters[(byte)UnityParameterCode.GameVersion]))
						{
							_server.SubServers.DataBaseServer.SendOperationRequest(operationRequest, sendParameters);
						}
						else
						{
							SendOperationResponse(new OperationResponse(operationRequest.OperationCode = operationRequest.OperationCode, operationRequest.Parameters)
							{
								ReturnCode = (byte)UnityErrorCode.IncorrectGameVersion,
								DebugMessage = "Incorrect game version"
							}, new SendParameters());

							Disconnect();
						}
						break;
					}
				case ((byte)UnityClientCode.Master):
					{
						MasterServerHandler(operationRequest, sendParameters);
					}
					break;
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

		/// <summary>
		/// Установить статус подключения игроку
		/// </summary>
		/// <param name="newStatus">Новый статус</param>
		public void SetClientConnectedStatus(ClientConnectedStatus newStatus)
		{
			ClientConnectedStatus = newStatus;
		}

		/// <summary>
		/// Проверка версии клиента
		/// </summary>
		/// <param name="version">Версия клиента</param>
		/// <returns>False - если версия не совпадает. True - если версия совпадает</returns>
		public bool CheckVersion(string version)
		{
			if (version == "0.2")
			{
				return true;
			}
			else
				return false;
		}

		#endregion
	}
}

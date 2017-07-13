using System;
using System.Collections.Generic;

using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

using ExitGames.Logging;
using PhotonHostRuntimeInterfaces;

using SilkServer.Server2Server.Operations;
using SilkServer.SubServer.Handlers;
using SilkServerCommon;

namespace SilkServer.SubServer
{
	public class OutgoingMasterServerPeer : OutboundS2SPeer
	{
		#region Constants and Fields

		public Dictionary<byte, PhotonRequestHandler> RequestHandlers = new Dictionary<byte, PhotonRequestHandler>();
		public Dictionary<byte, PhotonResponseHandler> ResponseHandlers = new Dictionary<byte, PhotonResponseHandler>();
		public Dictionary<byte, PhotonEventHandler> EventHandlers = new Dictionary<byte, PhotonEventHandler>();

		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private readonly SubServer _application;

		protected bool IsRegistered { get; set; }

		#endregion

		#region Constructors and Destructors

		public OutgoingMasterServerPeer(SubServer server) : base(server)
		{
			_application = server;
		}

		#endregion

		#region Overload of OutboundS2SPeer

		protected override void OnConnectionEstablished(object responseObject)
		{
			Log.InfoFormat("Connection to Master at {0}:{1} established (id={2})", RemoteIP, RemotePort, ConnectionId);

			RequestFiber.Enqueue(Register);
		}

		protected override void OnConnectionFailed(int errorCode, string errorMessage)
		{
			Log.ErrorFormat(
					"Master connection failed: address={0}, errorCode={1}, msg={2}",
					_application.MasterEndPoint,
					errorCode,
					errorMessage);

			IsRegistered = false;
			_application.ReconnectToMaster();
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			Log.InfoFormat("Подключение к Master Server закрыто (id={0})", ConnectionId);

			IsRegistered = false;
			_application.ReconnectToMaster();
		}

		protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
		{
			PhotonEventHandler handler;

			if (eventData.Parameters.ContainsKey((byte)UnityParameterCode.SubOperationCode) && EventHandlers.TryGetValue(Convert.ToByte(eventData.Parameters[(byte)UnityParameterCode.SubOperationCode]), out handler))
			{
				Log.DebugFormat("Found handler for OperationCode {0}", Convert.ToByte(eventData.Parameters[(byte)UnityParameterCode.SubOperationCode]));

				handler.HandleEvent(eventData);
			}
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			PhotonRequestHandler handler;

			/* Информация о приходящем запросе
            foreach(KeyValuePair<byte, object> keyValuePair in operationRequest.Parameters)
            {
                Log.DebugFormat("{0} - {1}", (UnityParameterCode)keyValuePair.Key, keyValuePair.Value);
            } */

			Log.Info(operationRequest.Parameters[(byte)UnityParameterCode.SubOperationCode]);

			if (operationRequest.Parameters.ContainsKey((byte)UnityParameterCode.SubOperationCode) && RequestHandlers.TryGetValue(Convert.ToByte(operationRequest.Parameters[(byte)UnityParameterCode.SubOperationCode]), out handler))
			{
				Log.DebugFormat("Found handler for OperationCode {0}", Convert.ToByte(operationRequest.Parameters[(byte)UnityParameterCode.SubOperationCode]));

				handler.HandleRequest(operationRequest);
			}
		}

		protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
		{
			switch ((OperationCode)operationResponse.OperationCode)
			{
				default:
					{
						PhotonResponseHandler handler;

						if (operationResponse.Parameters.ContainsKey((byte)UnityParameterCode.SubOperationCode) && ResponseHandlers.TryGetValue(Convert.ToByte(operationResponse.Parameters[(byte)UnityParameterCode.SubOperationCode]), out handler))
						{
							Log.DebugFormat("Found handler for OperationCode {0}", Convert.ToByte(operationResponse.Parameters[(byte)UnityParameterCode.SubOperationCode]));

							handler.HandleResponse(operationResponse);
						}
						else
						{
							if (Log.IsDebugEnabled)
							{
								Log.DebugFormat("Received unknown operation code {0}", operationResponse.OperationCode);
							}
						}
						break;
					}

				case OperationCode.RegisterSubServer:
					{
						if (operationResponse.ReturnCode != 0)
						{
							Log.WarnFormat("Failed to registered at Master: err={0}, msg={1}", operationResponse.ReturnCode, operationResponse.DebugMessage);
							Disconnect();
							return;
						}

						Log.Info("Соеденение с Master Server установлено!");

						IsRegistered = true;
						break;
					}
			}
		}

		#endregion

		#region Methods

		protected virtual void Register()
		{
			var contract = new RegisterSubServer
			{
				SubServerAddress = _application.PublicIPAddress.ToString(),
				TcpPort = _application.SubServerTcpPort,
				UdpPort = _application.SubServerUdpPort,
				ServerId = _application.ServerId.ToString(),
				ServerType = (int)_application.ServerType
			};

			var request = new OperationRequest((byte)OperationCode.RegisterSubServer, contract);
			SendOperationRequest(request, new SendParameters());
		}

		#endregion
	}
}

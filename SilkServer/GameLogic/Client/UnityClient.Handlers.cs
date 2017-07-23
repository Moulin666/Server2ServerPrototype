using Photon.SocketServer;
using SilkServer.GameLogic.WorldSystem;
using SilkServer.Operations.Responses;
using SilkServerCommon;
using System;
using System.Collections.Generic;

namespace SilkServer.GameLogic.Client
{
	// Handlers
	public partial class UnityClient
	{
		/// <summary>
		/// Обработчик мастер сервера
		/// </summary>
		private void MasterServerHandler(OperationRequest operationRequest, SendParameters sendParameters)
		{
			var subOperationCode = (byte)operationRequest.Parameters[(byte)UnityParameterCode.SubOperationCode];

			switch (subOperationCode)
			{
				case (byte)UnitySubOperationCode.SendChatMessage:
					ChatHandler(operationRequest, sendParameters);
					break;

				default:
					Log.WarnFormat("Unknown operation code: {0}", operationRequest.OperationCode);
					break;
			}
		}

		/// <summary>
		/// Обработчик чата
		/// </summary>
		private void ChatHandler(OperationRequest operationRequest, SendParameters sendParameters)
		{
			var chatRequest = new ChatMessageResponse(Protocol, operationRequest);

			if (!chatRequest.IsValid)
			{
				var response = new OperationResponse
				{
					OperationCode = operationRequest.OperationCode,
					ReturnCode = (byte)UnityErrorCode.InvalidParameters
				};

				SendOperationResponse(response, sendParameters);

				return;
			}

			string message = chatRequest.Message;

			if (message.StartsWith("/"))
			{
				string[] param = message.Split(new char[] { ' ' }, StringSplitOptions.None);

				switch (param[0])
				{
					case "/pm":
						ChatCommands.Instance.PrivateMessageCommand(sendParameters, this, param);
						break;

					case "/help":
						ChatCommands.Instance.HelpCommand(sendParameters, this, param);
						break;

					default:
						{
							message = "<color=#B22222>[Server]: Команда '" + param[0] + "' не доступна!"
								+ "\nИспользуйте '/help' для получения списка доступных команд сервера!</color>";

							var errorEventData = new EventData((byte)UnityEventCode.ChatMessage);
							errorEventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, message } };
							World.Instance.Send(this, errorEventData, sendParameters.Unreliable);
						}
						break;
				}

				return;
			}

			message = "<color=#D2B48C>" + Username + ": " + message + "</color>";

			var eventData = new EventData((byte)UnityEventCode.ChatMessage);
			eventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, message } };
			World.Instance.SendToAll(eventData); // IN GAME
		}
	}
}

using ExitGames.Logging;
using Photon.SocketServer;
using SilkServer.GameLogic.Client;
using SilkServerCommon;
using System.Collections.Generic;

namespace SilkServer.GameLogic.WorldSystem
{
	public class ChatCommands
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public static ChatCommands Instance = new ChatCommands();

		#endregion

		#region Commands

		/// <summary>
		/// Отправить личное сообщение
		/// </summary>
		/// <param name="sendParameters">SendParameters</param>
		/// <param name="sender">Отправитель сообщения</param>
		/// <param name="param">Параметры</param>
		public void PrivateMessageCommand(SendParameters sendParameters, UnityClient sender, string[] param)
		{
			if (param[1] == "")
				return;

			var personalEventData = new EventData((byte)UnityEventCode.ChatMessage);
			var message = "";

			string privateMessage = "";
			string targetName = param[1];

			for (int i = 2; i < param.Length; i++)
				message = message + " " + param[i];

			var targetClient = World.Instance.GetPlayerByName(targetName);

			if (targetClient != null)
			{
				if (sender.Username == targetName)
				{
					privateMessage = "<color=#B22222>[Server]: Вы не можете отправлять личные сообщения самому себе!</color>";

					personalEventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, privateMessage } };
					World.Instance.Send(sender, personalEventData, sendParameters.Unreliable);

					return;
				}

				privateMessage = "<color=#9400D3>" + sender.Username + "(ОТ КОГО):" + message + "</color>";
				personalEventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, privateMessage } };
				World.Instance.Send(targetClient, personalEventData, sendParameters.Unreliable);

				privateMessage = "<color=#9400D3>" + targetName + "(КОМУ):" + message + "</color>";
				personalEventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, privateMessage } };
				World.Instance.Send(sender, personalEventData, sendParameters.Unreliable);
			}
			else
			{
				privateMessage = "<color=#B22222>[Server]: Игрок '" + targetName + "' не в сети!</color>";

				personalEventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, privateMessage } };
				World.Instance.Send(sender, personalEventData, sendParameters.Unreliable);

				return;
			}
		}

		/// <summary>
		/// Получить список доступных команд сервера
		/// </summary>
		/// <param name="sendParameters">SendParameters</param>
		/// <param name="sender">отправитель</param>
		/// <param name="param">параметры</param>
		public void HelpCommand(SendParameters sendParameters, UnityClient sender, string[] param)
		{
			var message = "Unknown";
			var eventData = new EventData((byte)UnityEventCode.ChatMessage);

			// Chat
			message = "<color=#FF7F50>\n[Chat]: /pm [Имя] [Сообщение] - Отправить личное сообщение.\n";
			eventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, message } };
			World.Instance.Send(sender, eventData, sendParameters.Unreliable);

			// Other
			message = "[Other]: /help - Помощь по командам.\n</color>";
			eventData.Parameters = new Dictionary<byte, object> { { (byte)UnityParameterCode.ChatMessage, message } };
			World.Instance.Send(sender, eventData, sendParameters.Unreliable);
		}

		#endregion
	}
}

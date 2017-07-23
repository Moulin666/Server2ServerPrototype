using ExitGames.Logging;
using Photon.SocketServer;
using SilkServer.GameLogic.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkServer.GameLogic.WorldSystem
{
	public class World : IDisposable
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public static World Instance = new World();

		public Dictionary<Guid, UnityClient> Players { get; protected set; }

		#endregion

		#region Constructors & Destructors

		public World()
		{
			Players = new Dictionary<Guid, UnityClient>();
		}

		public void Dispose()
		{
			lock (Players)
			{
				foreach (var player in Players.Values)
				{
					player.Disconnect();
				}

				Players.Clear();
			}
		}

		#endregion

		#region Enter Join Clients

		/// <summary>
		/// Попытатся войти в игру
		/// </summary>
		/// <param name="player">Клиент</param>
		/// <returns>false - игрок уже в сети | true - игрок вошел</returns>
		public bool TryJoin(UnityClient player)
		{
			lock(Players)
			{
				if (Players.ContainsKey(player.UserId))
					return false;

				if (Players.Values.FirstOrDefault(s => s.Username == player.Username) != null)
					return false;

				Players[player.UserId] = player;
				return true;
			}
		}

		/// <summary>
		/// Покинуть игру
		/// </summary>
		/// <param name="player">Клиент</param>
		public void Leave(UnityClient player)
		{
			lock(Players)
			{
				if(Players.ContainsKey(player.UserId))
				{
					Players.Remove(player.UserId);
				}
			}
		}

		/// <summary>
		/// Войти в бой
		/// </summary>
		/// <param name="player">клиент</param>
		public void JoinGame(UnityClient player)
		{
			lock(Players)
			{
				if (player.ClientConnectedStatus == ClientConnectedStatus.Lobby)
				{
					player.SetClientConnectedStatus(ClientConnectedStatus.Game);
				}
			}
		}

		/// <summary>
		/// Выйти из боя
		/// </summary>
		/// <param name="player">клиент</param>
		public void LeaveGame(UnityClient player)
		{
			lock (Players)
			{
				if (player.ClientConnectedStatus == ClientConnectedStatus.Game)
				{
					player.SetClientConnectedStatus(ClientConnectedStatus.Lobby);
				}
			}
		}

		#endregion

		#region Get Clients
		
		/// <summary>
		/// Получить игрока по Id
		/// </summary>
		/// <param name="id">Id игрока</param>
		/// <returns>Вернет игрока или пустое значение</returns>
		public UnityClient GetPlayerById(Guid id)
		{
			lock (Players)
			{
				return Players.Values.FirstOrDefault(player => player.UserId == id);
			}
		}

		/// <summary>
		/// Получить игрока по имени
		/// </summary>
		/// <param name="name">Имя игрока</param>
		/// <returns>Вернет игрока или пустое значение</returns>
		public UnityClient GetPlayerByName(string name)
		{
			lock (Players)
			{
				return Players.Values.FirstOrDefault(player => player.Username == name);
			}
		}

		#endregion

		#region Send Event

		/// <summary>
		/// Отправить ивент всем кто находится в игре
		/// </summary>
		/// <param name="eventData">Data</param>
		/// <param name="unreliable">SendParameters unreliable</param>
		public void SendToAll(EventData eventData, bool unreliable = true)
		{
			lock (Players)
			{
				eventData.SendTo(Players.Values, new SendParameters
				{
					Encrypted = false,
					Unreliable = unreliable
				});
			}
		}

		/// <summary>
		/// Отправить ивент всем кто находится в бою
		/// </summary>
		/// <param name="eventData">Data</param>
		/// <param name="unreliable">SendParameters unreliable</param>
		public void SendToAllInGame(EventData eventData, bool unreliable = true)
		{
			lock (Players)
			{
				var playerList = new List<UnityClient>();
				foreach (var p in Players)
				{
					if (p.Value.ClientConnectedStatus == ClientConnectedStatus.Game)
					{
						playerList.Add(p.Value);
					}
				}

				eventData.SendTo(playerList, new SendParameters
				{
					Encrypted = false,
					Unreliable = unreliable
				});
			}
		}

		/// <summary>
		/// Отправить ивент конкретному игроку
		/// </summary>
		/// <param name="target">Конкретный игрок</param>
		/// <param name="eventData">Data</param>
		/// <param name="unreliable">SendParameters unreliable</param>
		public void Send(UnityClient target, EventData eventData, bool unreliable)
		{
			lock (Players)
			{
				target.SendEvent(eventData, new SendParameters
				{
					Encrypted = false,
					Unreliable = unreliable
				});
			}
		}

		#endregion
	}
}

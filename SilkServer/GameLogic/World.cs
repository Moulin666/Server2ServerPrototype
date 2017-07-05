using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkServer.GameLogic
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
					// отключить игрока
				}

				Players.Clear();
			}
		}

		#endregion

		#region Enter Join Clients

		public bool TryJoin(UnityClient player)
		{
			lock(Players)
			{
				if (Players.ContainsKey(player.UserId))
					return false;

				Players[player.UserId] = player;

				return true;
			}
		}

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

		public UnityClient GetPlayerById(Guid id)
		{
			lock (Players)
			{
				return Players.Values.FirstOrDefault(player => player.UserId == id);
			}
		}

		/*public UnityClient GetPlayerByName(string name)
		{
			lock (Players)
			{
				return Players.Values.FirstOrDefault(player => player.PlayerName == name);
			}
		}*/

		#endregion

		#region Send Event

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

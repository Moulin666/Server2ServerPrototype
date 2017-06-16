/*
 * Created by
 * roman.khusnetdinov
*/

using System;
using System.Linq;
using System.Collections.Generic;

using ExitGames.Logging;

using SilkServer.Server2Server;

namespace SilkServer.MasterServer
{
	public class SubServerCollection : Dictionary<Guid, IncomingSubServerPeer>
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		#endregion

		#region Servers

		public IncomingSubServerPeer LoginServer { get; protected set; }
		public IncomingSubServerPeer LobbyServer { get; protected set; }
		public IncomingSubServerPeer GameServer { get; protected set; }

		#endregion

		#region Methods

		public void OnConnect(IncomingSubServerPeer subServerPeer)
		{
			if (!subServerPeer.ServerId.HasValue)
			{
				throw new InvalidOperationException("Server ID cannot be null");
			}

			if (subServerPeer.ServerId.HasValue)
			{
				lock (this)
				{
					IncomingSubServerPeer peer;

					if (TryGetValue(subServerPeer.ServerId.Value, out peer))
					{
						peer.Disconnect();
						Remove(subServerPeer.ServerId.Value);

						if (subServerPeer.ServerId.Value == LoginServer.ServerId)
						{
							LoginServer = null;
						}

						if (subServerPeer.ServerId.Value == LobbyServer.ServerId)
						{
							LobbyServer = null;
						}

						if (subServerPeer.ServerId.Value == GameServer.ServerId)
						{
							GameServer = null;
						}
					}

					Add(subServerPeer.ServerId.Value, subServerPeer);

					ResetServers();
				}
			}
		}

		public void ResetServers()
		{
			if (LoginServer != null && LoginServer.ServerType != ServerType.Login)
			{
				IncomingSubServerPeer peer = Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.Login).FirstOrDefault();

				if (peer != null)
				{
					LoginServer = peer;
				}
			}

			if (LobbyServer != null && LobbyServer.ServerType != ServerType.Lobby)
			{
				IncomingSubServerPeer peer = Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.Lobby).FirstOrDefault();

				if (peer != null)
				{
					LobbyServer = peer;
				}
			}

			if (GameServer != null && GameServer.ServerType != ServerType.GameServer)
			{
				IncomingSubServerPeer peer = Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.GameServer).FirstOrDefault();

				if (peer != null)
				{
					GameServer = peer;
				}
			}

			// ========================================================================= \\

			if (LoginServer == null)
			{
				LoginServer =
					Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.Login).FirstOrDefault() ??
					Values.Where(subServerPeer => (subServerPeer.ServerType & ServerType.Login) == ServerType.Login).FirstOrDefault();
			}

			if (LobbyServer == null)
			{
				LobbyServer =
					Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.Lobby).FirstOrDefault() ??
					Values.Where(subServerPeer => (subServerPeer.ServerType & ServerType.Lobby) == ServerType.Lobby).FirstOrDefault();
			}

			if (GameServer == null)
			{
				GameServer =
				Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.GameServer).FirstOrDefault() ??
				Values.Where(subServerPeer => (subServerPeer.ServerType & ServerType.GameServer) == ServerType.GameServer).FirstOrDefault();
			}

			// ========================================================================= \\

			if (LoginServer != null)
			{
				Log.Debug("LoginServer: " + LoginServer.ConnectionId);
			}

			if (LoginServer != null)
			{
				Log.Debug("LobbyServer: " + LoginServer.ConnectionId);
			}

			if (GameServer != null)
			{
				Log.Debug("GameServer: " + GameServer.ConnectionId);
			}
		}

		public void OnDisconnect(IncomingSubServerPeer subServerPeer)
		{
			if (!subServerPeer.ServerId.HasValue)
			{
				throw new InvalidOperationException("Server ID cannot be null");
			}

			Guid id = subServerPeer.ServerId.Value;

			lock (this)
			{
				IncomingSubServerPeer peer;

				if (TryGetValue(id, out peer)) return;

				if (peer == subServerPeer)
				{
					Remove(id);

					if (LoginServer != null && id == LoginServer.ServerId)
					{
						LoginServer = null;
					}

					if (LobbyServer != null && id == LobbyServer.ServerId)
					{
						LobbyServer = null;
					}

					if (GameServer != null && id == GameServer.ServerId)
					{
						GameServer = null;
					}

					ResetServers();
				}
			}
		}

		#endregion
	}
}

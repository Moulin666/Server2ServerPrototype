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

		public IncomingSubServerPeer DataBaseServer { get; protected set; }

		#endregion

		#region Methods

		public void OnConnect(IncomingSubServerPeer subServerPeer)
		{
			if (!subServerPeer.ServerId.HasValue)
			{
				throw new InvalidOperationException("Server ID cannot be null");
			}
			else
			{
				lock (this)
				{
					IncomingSubServerPeer peer;

					if (TryGetValue(subServerPeer.ServerId.Value, out peer))
					{
						peer.Disconnect();
						Remove(subServerPeer.ServerId.Value);

						if (subServerPeer.ServerId.Value == DataBaseServer.ServerId)
						{
							DataBaseServer = null;
						}
					}

					Add(subServerPeer.ServerId.Value, subServerPeer);

					ResetServers();
				}
			}
		}

		public void ResetServers()
		{
			if (DataBaseServer != null && DataBaseServer.ServerType != ServerType.DataBase)
			{
				IncomingSubServerPeer peer = Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.DataBase).FirstOrDefault();

				if (peer != null)
				{
					DataBaseServer = peer;
				}
			}

			// ========================================================================= \\

			if (DataBaseServer == null)
			{
				DataBaseServer =
					Values.Where(subServerPeer => subServerPeer.ServerType == ServerType.DataBase).FirstOrDefault() ??
					Values.Where(subServerPeer => (subServerPeer.ServerType & ServerType.DataBase) == ServerType.DataBase).FirstOrDefault();
			}

			// ========================================================================= \\

			Log.Debug("~~~~~~~~~~~~~~~~~~~~~~~~[SERVER LIST]~~~~~~~~~~~~~~~~~~~~~~~~");

			if (DataBaseServer != null)
			{
				Log.Debug("DataBaseServer: " + DataBaseServer.ConnectionId);
			}

			Log.Debug("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
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

					if (DataBaseServer != null && id == DataBaseServer.ServerId)
					{
						DataBaseServer = null;
					}

					ResetServers();
				}
			}
		}

		#endregion
	}
}

/*
 * Created by
 * roman.khusnetdinov
*/

using System;
using System.IO;
using System.Collections.Generic;

using Photon.SocketServer;

using log4net;
using log4net.Config;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using LogManager = ExitGames.Logging.LogManager;
using SilkServer.GameLogic.Client;

namespace SilkServer.MasterServer
{
	public class MasterServer : ApplicationBase
	{
		#region Constants and Fields

		private readonly ILogger Log = LogManager.GetCurrentClassLogger();

		// ---------------- [Колекция подключенных серверов]
		public SubServerCollection SubServers { get; protected set; }

		// ---------------- [Подключенные клиенты] GUID - UserId
		public Dictionary<Guid, UnityClient> ConnectedClients { get; protected set; }

		#endregion

		#region Overload of ApplicationBase

		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			// Подключился сервер
			if (IsSubServerPeer(initRequest))
			{
				if (Log.IsDebugEnabled)
				{
					Log.DebugFormat("Connected new SubServer!");
				}
				return new IncomingSubServerPeer(initRequest, this);
			}


			// Подключился клиент
			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Connected new UnityClient!");
			}
			return new UnityClient(initRequest, this);
		}

		protected override void Setup()
		{
			// ---------------- [Создаем LOG для мастер сервера]
			LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
			GlobalContext.Properties["LogFileName"] = "MS" + ApplicationName;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));


			SubServers = new SubServerCollection();
			ConnectedClients = new Dictionary<Guid, UnityClient>();

			Log.Info("MASTER SERVER - ЗАПУЩЕН!");
		}

		protected override void TearDown()
		{
			Log.Info("MASTER SERVER - ОСТАНОВЛЕН!");
		}

		#endregion

		#region Methods

		protected virtual bool IsSubServerPeer(InitRequest initRequest)
		{
			return initRequest.LocalPort == MasterServerSettings.Default.IncomingSubServerPort;
		}

		#endregion
	}
}

using System;
using System.IO;
using System.Net;
using System.Threading;

using SilkServer.SubServer.Handlers;
using SilkServer.Server2Server;

using Photon.SocketServer;

using log4net;
using log4net.Config;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using LogManager = ExitGames.Logging.LogManager;

namespace SilkServer.SubServer
{
	public abstract class SubServer : ApplicationBase
	{
		#region Constants and Fields

		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public Guid ServerId { get; private set; }

		#endregion

		#region Properties

		// Singletons
		public static SubServer _instance;
		public static new SubServer Instance
		{
			get { return _instance; }
			protected set { Interlocked.Exchange(ref _instance, value); }
		}

		private OutgoingMasterServerPeer _masterPeer;
		public OutgoingMasterServerPeer MasterPeer
		{
			get { return _masterPeer; }
			protected set { Interlocked.Exchange(ref _masterPeer, value); }
		}

		private ServerType _serverType;
		public ServerType ServerType
		{
			get { return _serverType; }
			protected set { _serverType = value; }
		}

		// Properties
		private byte _isReconnecting;

		private Timer _retry;

		protected int ConnectRetryIntervalSeconds { get; set; }

		public IPEndPoint MasterEndPoint { get; protected set; }

		public string PublicIPAddress { get; set; }
		public int? SubServerTcpPort { get; set; }
		public int? SubServerUdpPort { get; set; }

		#endregion

		#region Constructors and Destructors

		public SubServer()
		{
			IPAddress address = IPAddress.Parse(SubServerSettings.Default.MasterIPAddress);
			int port = SubServerSettings.Default.OutgoingMasterServerPeerPort;

			MasterEndPoint = new IPEndPoint(address, port);

			ConnectRetryIntervalSeconds = SubServerSettings.Default.ConnectRetryInterval;

			PublicIPAddress = SubServerSettings.Default.PublicIPAddress;
			SubServerTcpPort = SubServerSettings.Default.SubServerTcpPort;
			SubServerUdpPort = SubServerSettings.Default.SubServerUdpPort;
		}

		#endregion

		#region Overides of ApplicationBase

		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			return null;
		}

		protected override void Setup()
		{
			Instance = this;

			InitLogging();

			Protocol.AllowRawCustomValues = true;

			ConnectToMaster();
		}

		protected override void TearDown()
		{
		}

		#endregion

		#region Methods

		protected virtual void InitLogging()
		{
			LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
			GlobalContext.Properties["LogFileName"] = "SS" + ApplicationName;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));
		}

		public void ConnectToMaster()
		{
			if (_masterPeer == null)
			{
				_masterPeer = CreateServerPeer();
			}

			if (_masterPeer.ConnectTcp(MasterEndPoint, "Master"))
			{
				if (Log.IsDebugEnabled)
				{
					Log.DebugFormat(_isReconnecting == 0 ? "Connecting to Master at {0}" : "Reconnecting to Master at {0}", MasterEndPoint);
				}
			}
			else
			{
				Log.WarnFormat("Master connection refused");
				return;
			}

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat(_isReconnecting == 0 ? "Connecting to Master at {0}" : "Reconnecting to Master at {0}", MasterEndPoint);
			}
		}

		public void ReconnectToMaster()
		{
			Thread.VolatileWrite(ref _isReconnecting, 1);

			_retry = new Timer(o => ConnectToMaster(), null, ConnectRetryIntervalSeconds * 1000, 0);
		}

		protected override void OnServerConnectionFailed(int errorCode, string errorMessage, object state)
		{
			if (_isReconnecting == 0)
			{
				Log.ErrorFormat("Master connection failed with error {0}: {1}", errorCode, errorMessage);
			}
			else if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Master connection failed with error {0}: {1}", errorCode, errorMessage);
			}

			ReconnectToMaster();
		}

		protected virtual OutgoingMasterServerPeer CreateServerPeer()
		{
			var serverPeer = new OutgoingMasterServerPeer(this);
			AddHandlersToServerPeer(serverPeer);

			return serverPeer;
		}

		protected abstract void AddHandlersToServerPeer(OutgoingMasterServerPeer serverPeer);

		#endregion
	}
}

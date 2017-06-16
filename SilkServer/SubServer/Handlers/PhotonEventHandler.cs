using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

namespace SilkServer.SubServer.Handlers
{
	public abstract class PhotonEventHandler
	{
		#region Constants and Fields

		protected readonly OutboundS2SPeer _peer;

		#endregion

		#region Constructors and Destructors

		protected PhotonEventHandler(OutboundS2SPeer peer)
		{
			_peer = peer;
		}

		#endregion

		#region Properties

		protected delegate void beforeEventReceived();
		protected beforeEventReceived BeforeEventReceived;

		protected delegate void afterEventReceived();
		protected afterEventReceived AfterEventReceived;

		#endregion

		#region Handle

		public void HandleEvent(IEventData eventData)
		{
			if (BeforeEventReceived != null)
			{
				BeforeEventReceived();
			}

			OnHandleEvent(eventData);

			if (AfterEventReceived != null)
			{
				AfterEventReceived();
			}
		}

		public abstract void OnHandleEvent(IEventData eventData);

		#endregion
	}
}

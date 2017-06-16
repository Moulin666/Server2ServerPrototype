using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

namespace SilkServer.SubServer.Handlers
{
	public abstract class PhotonRequestHandler
	{
		#region Constants and Fields

		protected readonly OutboundS2SPeer _peer;

		#endregion

		#region Constructors and Destructors

		protected PhotonRequestHandler(OutboundS2SPeer peer)
		{
			_peer = peer;
		}

		#endregion

		#region Properties

		protected delegate void beforeRequestReceived();
		protected beforeRequestReceived BeforeRequestReceived;

		protected delegate void afterRequestReceived();
		protected afterRequestReceived AfterRequestReceived;

		#endregion

		#region Handle

		public void HandleRequest(OperationRequest operationRequest)
		{
			if (BeforeRequestReceived != null)
			{
				BeforeRequestReceived();
			}

			OnHandleRequest(operationRequest);

			if (AfterRequestReceived != null)
			{
				AfterRequestReceived();
			}
		}

		public abstract void OnHandleRequest(OperationRequest operationRequest);

		#endregion
	}
}

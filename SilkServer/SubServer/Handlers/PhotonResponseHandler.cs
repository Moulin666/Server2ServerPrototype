using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

namespace SilkServer.SubServer.Handlers
{
	public abstract class PhotonResponseHandler
	{
		#region Constants and Fields

		protected readonly OutboundS2SPeer _peer;

		#endregion

		#region Constructors and Destructors

		protected PhotonResponseHandler(OutboundS2SPeer peer)
		{
			_peer = peer;
		}

		#endregion

		#region Properties

		protected delegate void beforeResponseReceived();
		protected beforeResponseReceived BeforeResponseReceived;

		protected delegate void afterResponseReceived();
		protected afterResponseReceived AfterResponseReceived;

		#endregion

		#region Handle

		public void HandleResponse(OperationResponse operationResponse)
		{
			if (BeforeResponseReceived != null)
			{
				BeforeResponseReceived();
			}

			OnHandleResponse(operationResponse);

			if (AfterResponseReceived != null)
			{
				AfterResponseReceived();
			}
		}

		public abstract void OnHandleResponse(OperationResponse operationResponse);

		#endregion
	}
}

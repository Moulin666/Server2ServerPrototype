using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using SilkServerCommon;

namespace SilkServer.Operations.Responses
{
	public class ChatMessageResponse : Operation
	{
		public ChatMessageResponse(IRpcProtocol protocol, OperationRequest request) : base(protocol, request) {  }

		[DataMember(Code = (byte)UnityParameterCode.ChatMessage)]
		public string Message { get; set; }
	}
}

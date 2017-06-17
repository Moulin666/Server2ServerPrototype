using Photon.SocketServer;
using Photon.SocketServer.Rpc;

namespace SilkServer.Server2Server.Operations
{
	public class RegisterSubServer : Operation
	{
		#region Constructors and Destructors

		public RegisterSubServer(IRpcProtocol rpcProtocol, OperationRequest operationRequest)
			: base(rpcProtocol, operationRequest)
		{
		}

		public RegisterSubServer() { }

		#endregion

		#region Properties

		[DataMember(Code = 4, IsOptional = false)]
		public string SubServerAddress { get; set; }

		[DataMember(Code = 3, IsOptional = false)]
		public string ServerId { get; set; }

		[DataMember(Code = 2, IsOptional = true)]
		public int? TcpPort { get; set; }

		[DataMember(Code = 1, IsOptional = true)]
		public int? UdpPort { get; set; }

		[DataMember(Code = 5, IsOptional = true)]
		public int ServerType { get; set; }

		#endregion
	}
}

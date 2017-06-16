namespace SilkServer.Server2Server
{
	public enum ErrorCode : short
	{
		UnknownError = -4,
		OperationDenied = -3,
		OperationInvalid = -2,
		InternalServerError = -1,

		Ok = 0,

		UsernameInUse = 1,
		UsernameOrPasswordInvalid = 2,
	}
}

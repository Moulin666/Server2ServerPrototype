namespace SilkServer.GameLogic.Client
{
	public enum ClientConnectedStatus : byte
	{
		NotAuth = 0x1,

		Lobby = 0x2,

		Game = 0x8,
	}
}

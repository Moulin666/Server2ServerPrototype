using System;

namespace SilkServer.Server2Server
{
	[Flags]
	public enum ServerType
	{
		Login = 1,

		Lobby = 2,

		GameServer = 3
	}
}

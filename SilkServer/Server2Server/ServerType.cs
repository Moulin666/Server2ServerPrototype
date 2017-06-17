using System;

namespace SilkServer.Server2Server
{
	[Flags]
	public enum ServerType
	{
		Login = 1,

		GameServer = 2,

		Lobby = 3
	}
}

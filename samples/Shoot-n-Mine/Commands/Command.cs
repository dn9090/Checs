using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public enum CommandType
	{
		RocketLaunch,
		SateliteLaunch
	}

	public struct Command
	{
		public CommandType cmdType;

		public int playerId;

		public Command(CommandType type, int playerId)
		{
			this.cmdType = type;
			this.playerId = playerId;
		}
	}
}

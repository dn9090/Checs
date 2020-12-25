using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Checs;

namespace Shoot_n_Mine
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RocketLaunchCommand
	{
		public Command command;

		public RocketType rocketType;

		public Vector2 target;

		public RocketLaunchCommand(int playerId, RocketType rocketType, Vector2 target)
		{
			this.command = new Command(CommandType.RocketLaunch, playerId);
			this.rocketType = rocketType;
			this.target = target;
		}
	}
}

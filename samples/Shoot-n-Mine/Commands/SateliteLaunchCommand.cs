using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Checs;

namespace Shoot_n_Mine
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SateliteLaunchCommand
	{
		public Command command;

		public Vector2 target;

		public SateliteLaunchCommand(int playerId, Vector2 target)
		{
			this.command = new Command(CommandType.SateliteLaunch, playerId);
			this.target = target;
		}
	}
}

using System;
using System.Collections.Generic;
using SFML.System;
using Checs;

namespace Shoot_n_Mine.Engine
{
	public static class Time
	{
		public static float time => s_Clock.ElapsedTime.AsSeconds();

		public static float deltaTime;

		public static readonly float fixedDeltaTime = 0.01f;

		private static Clock s_Clock;

		private static float s_LastTimeCheck;

		public static void Initialize()
		{
			s_Clock = new Clock();
		}

		public static void UpdateDeltaTime()
		{
			float currentTimeCheck = time;
			deltaTime = currentTimeCheck - s_LastTimeCheck;
			s_LastTimeCheck = currentTimeCheck;
		}
	}
}

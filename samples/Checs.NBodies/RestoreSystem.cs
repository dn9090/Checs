using System;
using System.IO;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class RestoreSystem : System
	{
		internal MemoryStream memory;

		internal bool isHot;

		public RestoreSystem()
		{
			memory = new MemoryStream(1024);
		}

		~RestoreSystem()
		{
			memory.Dispose();
		}

		public override void Setup()
		{
		}

		public override void Run(float deltaTime)
		{
			if(isHot)
			{
				if(!Keyboard.IsKeyPressed(Keyboard.Key.S) && !Keyboard.IsKeyPressed(Keyboard.Key.R))
					isHot = false;
			} else {
				if(Keyboard.IsKeyPressed(Keyboard.Key.S))
				{
					Console.WriteLine("Saving...");
					memory.Seek(0, SeekOrigin.Begin);
					manager.TakeSnapshot(EntityQuery.universal, memory);
					isHot = true;
				}

				if(Keyboard.IsKeyPressed(Keyboard.Key.R))
				{
					Console.WriteLine("Restoring...");
					memory.Seek(0, SeekOrigin.Begin);
					manager.RestoreSnapshot(memory);
					isHot = true;
				}
			}
		}
	}
}
using System;
using System.Diagnostics;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Checs.NBodies
{
	class Program
	{
		static void Main(string[] args)
		{
			var window = new RenderWindow(new VideoMode(1280, 720), "Checs N-Body Demo");
			var view = window.GetView();
			window.Closed += (_, _) => window.Close();

			using var manager = new EntityManager();

			var systems = new System[] {
				new InputSystem(window, view),
				new GeneratorSystem(),
				new PhysicsSystem(),
				new ColorSystem(),
				new RenderingSystem(window)
			};

			for(int i = 0; i < systems.Length; ++i)
			{
				systems[i].manager = manager;
				systems[i].Setup();
			}

			var deltaTime = 0f;
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			//window.GetView().Center = new Vector2f(0f, 0f);

			while(window.IsOpen)
			{
				window.Clear();
				window.DispatchEvents();

				deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
				stopwatch.Restart();

				for(int i = 0; i < systems.Length; ++i)
					systems[i].Run(deltaTime);

				window.Display();
			}
		}
	}
}
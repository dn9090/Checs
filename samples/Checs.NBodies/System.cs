using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public abstract class System
	{
		public EntityManager manager;

		public virtual void Setup() {}
		
		public abstract void Run(float deltaTime);
	}
}
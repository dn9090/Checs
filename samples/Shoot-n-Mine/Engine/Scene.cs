using System;
using System.Collections.Generic;
using Checs;

namespace Shoot_n_Mine.Engine
{
	public abstract class Scene : IDisposable
	{
		public World world;

		public SceneManager manager;

		public List<ComponentSystem> systems;

		public void Initialize()
		{
			this.world = new World();
			this.systems = new List<ComponentSystem>();
		}

		public void Enable()
		{
			foreach(ComponentSystem system in this.systems)
				system.OnEnabled(this);
		}

		public void Update()
		{
			foreach(ComponentSystem system in this.systems)
				system.OnUpdate(this);
		}

		public void Disable()
		{
			foreach(ComponentSystem system in this.systems)
				system.OnDisabled(this);
		}		

		public void Dispose()
		{
			this.world.Dispose();
			this.world = null;
			this.systems.Clear();
			this.systems = null;
		}

		public virtual void OnSceneLoad() {}

		public virtual void OnSceneUnload() {}
	}
}

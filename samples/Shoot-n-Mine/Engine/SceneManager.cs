using System;
using System.Collections.Generic;
using Checs;

namespace Shoot_n_Mine.Engine
{
	public sealed class SceneManager
	{
		public Scene active;

		private Dictionary<int, Scene> m_Scenes;

		private int m_LoadId;

		private bool m_LoadAfterUpdate;

		private bool m_IsUpdating;

		public SceneManager()
		{
			this.active = null;
			this.m_Scenes = new Dictionary<int, Scene>();
			this.m_IsUpdating = false;
			this.m_LoadAfterUpdate = false;
		}

		public void Add(int id, Scene scene)
		{
			this.m_Scenes.TryAdd(id, scene);
			scene.manager = this;
		}

		public void Load(int id)
		{
			if(this.m_IsUpdating)
			{
				this.m_LoadId = id;
				this.m_LoadAfterUpdate = true;

				return;
			}

			if(this.m_Scenes.TryGetValue(id, out Scene scene))
			{
				if(this.active != null)
				{
					this.active.Disable();
					this.active.OnSceneUnload();
					this.active.Dispose();
				}
					
				this.active = scene;
				this.active.Initialize();
				this.active.OnSceneLoad();
				this.active.Enable();
			}
		}

		public void Update()
		{
			this.m_IsUpdating = true;

			this.active.Update();

			this.m_IsUpdating = false;

			if(this.m_LoadAfterUpdate)
				Load(this.m_LoadId);

			this.m_LoadAfterUpdate = false;
		}
	}
}

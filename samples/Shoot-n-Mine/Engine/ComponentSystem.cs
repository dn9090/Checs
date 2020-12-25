using System;
using Checs;

namespace Shoot_n_Mine.Engine
{
	public abstract class ComponentSystem
	{
		public virtual void OnEnabled(Scene Scene)
		{
		}

		public virtual void OnDisabled(Scene Scene)
		{
		}

		public abstract void OnUpdate(Scene Scene);
	}
}

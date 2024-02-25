using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class PhysicsSystem : System
	{
		public const float G = 1.3f; 

		public const float TimeScale = 5f;

		public EntityQuery query;

		public override void Setup()
		{
			this.query = manager.CreateQuery(new[] {
				ComponentType.Of<Position>(),
				ComponentType.Of<Velocity>(),
				ComponentType.Of<Acceleration>(),
				ComponentType.Of<Mass>()
			});
		}

		public override void Run(float deltaTime)
		{
			var itLhs = manager.GetIterator(this.query);

			while(itLhs.TryNext(out var lhs))
			{
				var itRhs = manager.GetIterator(this.query);

				while(itRhs.TryNext(out var rhs))
					Compute(lhs, rhs, G);
			}

			var time = TimeScale * deltaTime;

			var it = manager.GetIterator(this.query);

			while(it.TryNext(out var table))
			{
				var positions     = table.GetComponentData<Position>();
				var velocities    = table.GetComponentData<Velocity>();
				var accelerations = table.GetComponentDataReadOnly<Acceleration>();

				var atime = (time * time) * 0.5f;
				
				for(int i = 0; i < table.length; ++i)
				{
					positions[i].value  += velocities[i].value * time + atime * accelerations[i].value;
					velocities[i].value += accelerations[i].value * time;
				}
			}
		}

		public static void Compute(EntityTable lhs, EntityTable rhs, float g)
		{
			var entitiesA      = lhs.GetEntities();
			var accelerationsA = lhs.GetComponentData<Acceleration>();
			var positionsA     = lhs.GetComponentDataReadOnly<Position>();
			var velocitiesA    = lhs.GetComponentDataReadOnly<Velocity>();

			var entitiesB  = rhs.GetEntities();
			var positionsB = rhs.GetComponentDataReadOnly<Position>();
			var massesB    = rhs.GetComponentDataReadOnly<Mass>();

			for(int a = 0; a < lhs.length; ++a)
			{
				var acceleration = Vector2.Zero;

				for(int b = 0; b < rhs.length; ++b)
				{
					if(entitiesA[a] == entitiesB[b])
						continue;

					var delta = positionsB[b].value - positionsA[a].value;
					var mag   = delta.Length();
					var m     = massesB[b].value / (mag * mag * mag);

					acceleration += delta * m;
				}

				accelerationsA[a].value = g * acceleration;
			}
		}
	}
}
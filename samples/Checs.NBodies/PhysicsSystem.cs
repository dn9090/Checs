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
			var itOuter = manager.GetIterator(this.query);

			while(itOuter.TryNext(out var a))
			{
				var itInner = manager.GetIterator(this.query);

				while(itInner.TryNext(out var b))
					Compute(a, b, G);

				itInner.Dispose();
			}

			itOuter.Dispose();

			var time = TimeScale * deltaTime;

			manager.ForEach(this.query, static (span, time) => {
				var atime         = (time * time) * 0.5f;
				var positions     = span.GetComponentData<Position>();
				var velocities    = span.GetComponentData<Velocity>();
				var accelerations = span.GetComponentDataReadOnly<Acceleration>();

				for(int i = 0; i < span.length; ++i)
				{
					positions[i].value += velocities[i].value * time + atime * accelerations[i].value;
					velocities[i].value += accelerations[i].value * time;
				}
			}, time);
		}

		public static void Compute(EntityTable a, EntityTable b, float g)
		{
			var aEntities      = a.GetEntities();
			var aAccelerations = a.GetComponentData<Acceleration>();
			var aPositions     = a.GetComponentDataReadOnly<Position>();
			var aVelocities    = a.GetComponentDataReadOnly<Velocity>();

			var bEntities      = b.GetEntities();
			var bPositions     = b.GetComponentDataReadOnly<Position>();
			var bMasses        = b.GetComponentDataReadOnly<Mass>();

			for(int i = 0; i < a.length; ++i)
			{
				var entity = aEntities[i];
				var acceleration = Vector2.Zero;
				var position = aPositions[i].value;

				for(int j = 0; j < b.length; ++j)
				{
					if(entity == bEntities[j])
						continue;

					var delta = bPositions[j].value - position;
					var mag = delta.Length();
					var m = bMasses[j].value / (mag * mag * mag);

					acceleration += delta * m;
				}

				aAccelerations[i].value = g * acceleration;
			}
		}
	}
}
using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class GeneratorSystem : System
	{
		public static int nBodies = 100;

		public static float lifetime = 60;

		public EntityArchetype archetype;

		public EntityQuery query;

		public float time;

		public override void Setup()
		{
			this.archetype = manager.CreateArchetype(new[] {
				ComponentType.Of<Position>(),
				ComponentType.Of<Velocity>(),
				ComponentType.Of<Mass>(),
				ComponentType.Of<Acceleration>(),
				ComponentType.Of<RenderShape>()
			});
			this.query = manager.CreateQuery(includeTypes: new[] {
				ComponentType.Of<Position>(),
				ComponentType.Of<Velocity>(),
				ComponentType.Of<Mass>(),
				ComponentType.Of<Acceleration>(),
				ComponentType.Of<RenderShape>()
			}, excludeTypes: new[] {
				ComponentType.Of<GravCenter>()
			});

			GenerateGravCenter();
		}

		public override void Run(float deltaTime)
		{
			time -= deltaTime;

			if(time < 0)
			{
				manager.DestroyEntity(this.query);
				Generate(nBodies);

				time = lifetime;
			}
		}
		
		public void Generate(int count)
		{
			const float BaseMass = 0.1f;
			const float VarMass = 0.8f; 
			const float Speed = 2f;
			const float Initial = 12000.0f;

			var rand = new Random();

			manager.CreateEntity(this.archetype, count, static (span, rand) => {
				var positions  = span.GetComponentData<Position>();
				var velocities = span.GetComponentData<Velocity>();
				var masses     = span.GetComponentData<Mass>();
				var shapes     = span.GetComponentData<RenderShape>();
				
				for(int i = 0; i < span.length; ++i)
				{
					positions[i].value = new Vector2(rand.Next() % 2000 - 700, rand.Next() % 300 - 300);
					masses[i].value    = BaseMass + ((float)rand.Next() / (float)int.MaxValue) * VarMass;

					var radius = positions[i].value.Length();
					var norm   = Vector2.Normalize(positions[i].value);
					var rot    = Perpendicular(norm);
					var v      = MathF.Sqrt(Initial / radius / masses[i].value / Speed);

					velocities[i].value = rot * v;
					shapes[i].baseColor = new Color((uint)rand.Next());
					shapes[i].radius    = MathF.Sqrt(100f * masses[i].value);
				}
			}, rand);
		}

		public void GenerateGravCenter()
		{
			var entity = manager.CreateEntity(this.archetype);

			manager.AddComponentData<GravCenter>(entity);
			manager.SetComponentData<Position>(entity, new Position { value = Vector2.Zero });
			manager.SetComponentData<Velocity>(entity, new Velocity { value = Vector2.Zero });
			manager.SetComponentData<Mass>(entity, new Mass { value = 12000.0f });
			manager.SetComponentData<RenderShape>(entity, new RenderShape { radius = 100f, baseColor = new Color(255, 240, 225) });
		}

		public static Vector2 Perpendicular(Vector2 value)
		{
			return new Vector2(-value.Y, value.X);
		}
	}
}
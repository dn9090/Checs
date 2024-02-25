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

			manager.CreateEntity(this.archetype, count, static (table, rand) => {
				var positions  = table.GetComponentData<Position>();
				var velocities = table.GetComponentData<Velocity>();
				var masses     = table.GetComponentData<Mass>();
				var shapes     = table.GetComponentData<RenderShape>();
				
				for(int i = 0; i < table.length; ++i)
				{
					positions[i].value = new Vector2(rand.Next() % 2000 - 700, rand.Next() % 300 - 300);
					masses[i].value    = BaseMass + ((float)rand.Next() / (float)int.MaxValue) * VarMass;

					var radius = positions[i].value.Length();
					var normal = Vector2.Normalize(positions[i].value);
					var rot    = NMath.Perpendicular(normal);
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
			manager.SetComponentData(entity, new Position(Vector2.Zero));
			manager.SetComponentData(entity, new Velocity(Vector2.Zero));
			manager.SetComponentData(entity, new Mass(12000.0f));
			manager.SetComponentData(entity, new RenderShape { radius = 100f, baseColor = new Color(255, 240, 225) });
		}
	}
}
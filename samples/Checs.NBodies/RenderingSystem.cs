using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class RenderingSystem : System
	{
		public RenderTarget renderTarget;

		public CircleShape circle;

		public EntityQuery query;

		public RenderingSystem(RenderTarget renderTarget)
		{
			this.renderTarget = renderTarget;
		}

		public override void Setup()
		{
			this.query = manager.CreateQuery(new[] {
				ComponentType.Of<Position>(),
				ComponentType.Of<Mass>(),
				ComponentType.Of<RenderShape>()
			});

			this.circle = new CircleShape();
		}

		public override void Run(float deltaTime)
		{
			var it = manager.GetIterator(this.query);
			
			while(it.TryNext(out var table))
			{
				var positions = table.GetComponentDataReadOnly<Position>();
				var masses    = table.GetComponentDataReadOnly<Mass>();
				var shapes    = table.GetComponentDataReadOnly<RenderShape>();

				for(int i = 0; i < table.length; ++i)
				{
					circle.Radius    = shapes[i].radius;
					circle.Origin    = new Vector2f(shapes[i].radius, shapes[i].radius);
					circle.Position  = new Vector2f(positions[i].value.X, positions[i].value.Y);
					circle.FillColor = shapes[i].color;

					renderTarget.Draw(circle);
				}
			}
		}
	}
}
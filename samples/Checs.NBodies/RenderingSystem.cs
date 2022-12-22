using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class RenderingSystem : System
	{
		public struct RenderContext
		{
			public RenderTarget renderTarget;

			public CircleShape circle;
		}

		public RenderTarget renderTarget;

		public EntityQuery query;

		public RenderContext context;

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
			
			this.context = new RenderContext {
				renderTarget = renderTarget,
				circle = new CircleShape()
			};
		}

		public override void Run(float deltaTime)
		{
			manager.ForEach(this.query, static (span, context) => {
				var positions = span.GetComponentDataReadOnly<Position>();
				var masses    = span.GetComponentDataReadOnly<Mass>();
				var shapes    = span.GetComponentDataReadOnly<RenderShape>();

				for(int i = 0; i < span.length; ++i)
				{
					context.circle.Radius = shapes[i].radius;
					context.circle.Origin = new Vector2f(shapes[i].radius, shapes[i].radius);
					context.circle.Position = new Vector2f(positions[i].value.X, positions[i].value.Y);
					context.circle.FillColor = shapes[i].color;
					context.renderTarget.Draw(context.circle);
				}
			}, this.context);
		}
	}
}
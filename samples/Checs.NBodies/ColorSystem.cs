using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class ColorSystem : System
	{
		public EntityQuery query;

		public float time;

		public override void Setup()
		{
			this.query = manager.CreateQuery(new[] { ComponentType.Of<RenderShape>() });
		}

		public override void Run(float deltaTime)
		{
			time += deltaTime;
			var sin = ((float)Math.Sin(time) + 1f) / 2f;

			manager.ForEach(this.query, static (span, sin) => {
				var renderShapes = span.GetComponentData<RenderShape>();

				for(int i = 0; i < renderShapes.Length; ++i)
				{
					var vec = new Vector3(
						(float)(renderShapes[i].baseColor.R / 255f),
						(float)(renderShapes[i].baseColor.G / 255f),
						(float)(renderShapes[i].baseColor.B / 255f));
					var col = Lerp(vec, Vector3.One, MathF.Min(sin, 0.8f)) * 255f;

					renderShapes[i].color = new Color((byte)col.X, (byte)col.Y, (byte)col.Z, 255);
				}
			}, sin);
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return a + (b - a) * t;
		}
	}
}
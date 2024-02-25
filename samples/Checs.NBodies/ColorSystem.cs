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
			var sin = (MathF.Sin(time) + 1f) / 2f;

			var it = manager.GetIterator(this.query);

			while(it.TryNext(out var table))
			{
				var renderShapes = table.GetComponentData<RenderShape>();

				for(int i = 0; i < renderShapes.Length; ++i)
				{
					var vec = new Vector3(
						(float)(renderShapes[i].baseColor.R / 255f),
						(float)(renderShapes[i].baseColor.G / 255f),
						(float)(renderShapes[i].baseColor.B / 255f));
					var col = NMath.Lerp(vec, Vector3.One, MathF.Min(sin, 0.7f)) * 255f;

					renderShapes[i].color = new Color((byte)col.X, (byte)col.Y, (byte)col.Z, 255);
				}
			}
		}
	}
}
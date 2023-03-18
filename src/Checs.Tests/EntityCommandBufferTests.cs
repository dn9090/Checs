using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityCommandBufferTests
	{
		[Fact]
		public void ThrowsOnAccessAfterDispose()
		{
			using var manager = new EntityManager();

			var buffer = manager.CreateCommandBuffer();
			buffer.Dispose();

			Assert.Throws<ObjectDisposedException>(() => buffer.DestroyEntity(Span<Entity>.Empty));
		}

		[Fact]
		public void AllowsRepeatedPlayback()
		{
			using var manager = new EntityManager();

			using var buffer = manager.CreateCommandBuffer();

			buffer.CreateEntity(10);
			
			manager.Playback(buffer, clear: false);
			manager.Playback(buffer, clear: false);
			manager.Playback(buffer, clear: true);

			Assert.Equal(30, manager.entityCount);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityCommandBufferTests
	{
		[Fact]
		public void NoAccessAfterDispose()
		{
			using var manager = new EntityManager();

			var buffer = manager.CreateCommandBuffer();
			buffer.Dispose();

			Assert.Throws<ObjectDisposedException>(() => buffer.DestroyEntity(Span<Entity>.Empty));
		}
	}
}

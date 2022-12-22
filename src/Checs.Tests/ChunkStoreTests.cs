using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public unsafe partial class ChunkStoreTests
	{
		[Fact]
		public void AquireWithoutReleaseAllocatesChunks()
		{
			using ChunkStore store = new ChunkStore();

			{
				var count = store.GetCount();
				var chunks = new Chunk*[5];

				for(int i = 0; i < chunks.Length; ++i)
					chunks[i] = store.Aquire();

				Assert.Equal(IntPtr.Zero, store.free);
				Assert.Equal((IntPtr)chunks[chunks.Length - 1], store.head);
				Assert.Equal(count + chunks.Length, store.GetCount());
			}
		}

		[Fact]
		public void AquireAndReleaseReusesChunks()
		{
			using ChunkStore store = new ChunkStore();

			{
				var chunks = new IntPtr[5];
				var reusedChunks = new IntPtr[5];

				for(int i = 0; i < chunks.Length; ++i)
					chunks[i] = (IntPtr)store.Aquire();

				for(int i = 0; i < chunks.Length; ++i)
					store.Release((Chunk*)chunks[i]);

				for(int i = 0; i < chunks.Length; ++i)
					reusedChunks[i] = (IntPtr)store.Aquire();

				var set = new HashSet<IntPtr>(chunks);
				set.UnionWith(reusedChunks);

				Assert.Equal(chunks.Length, set.Count);
			}
		}
	}
}

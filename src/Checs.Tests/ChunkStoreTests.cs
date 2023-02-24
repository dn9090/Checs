using System;
using System.Collections.Generic;
using System.Threading;
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

		[Fact]
		public void ThreadsReuseChunks()
		{
			const int ChunkCount  = 12;
			const int ThreadCount = 4;

			static unsafe void DoWork(ref ChunkStore store, CountdownEvent countdownEvent, ManualResetEventSlim resetEvent)
			{
				var chunks = new Chunk*[ChunkCount];

				resetEvent.Wait();
				resetEvent.Reset();

				for(int i = 0; i < ChunkCount; ++i) // Allocating phase.
					chunks[i] = store.Aquire();

				countdownEvent.Signal();
				resetEvent.Wait();
				resetEvent.Reset();

				for(int i = 0; i < ChunkCount; ++i) // Release phase.
					store.Release(chunks[i]);

				countdownEvent.Signal();
				resetEvent.Wait();
				resetEvent.Reset();

				for(int i = 0; i < ChunkCount; ++i) // Reuse phase.
					chunks[i] = store.Aquire();
			}

			var store = new ChunkStore();

			var threads        = new Thread[ThreadCount];
			var resetEvents    = new ManualResetEventSlim[ThreadCount];
			var countdownEvent = new CountdownEvent(ThreadCount);
		
			for(int i = 0; i < threads.Length; ++i)
			{
				var resetEvent = new ManualResetEventSlim();
				var thread     = new Thread(() => DoWork(ref store, countdownEvent, resetEvent));

				thread.Start();

				resetEvents[i] = resetEvent;
				threads[i]     = thread;
			}

			for(int i = 0; i < threads.Length; ++i)
				resetEvents[i].Set(); // Allocating phase.
			
			countdownEvent.Wait();
			countdownEvent.Reset();

			Assert.Equal(ChunkCount * ThreadCount, store.GetCount());

			for(int i = 0; i < threads.Length; ++i)
				resetEvents[i].Set(); // Release phase.

			countdownEvent.Wait();
			countdownEvent.Reset();

			Assert.Equal(ChunkCount * ThreadCount, store.GetFreeCount());

			for(int i = 0; i < threads.Length; ++i)
				resetEvents[i].Set(); // Reuse phase.

			for(int i = 0; i < threads.Length; ++i)
				threads[i].Join();

			Assert.Equal(ChunkCount * ThreadCount, store.GetCount());

			store.Dispose();
		}
	}
}

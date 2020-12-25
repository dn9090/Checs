using System;
using System.Collections.Generic;
using System.Threading;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine.Net
{
	public static class NetworkBuffer
	{
		public const int Capacity = 1014;

		public static volatile int count;

		private static byte[] s_Buffer;

		private static SemaphoreSlim s_Semaphore;

		static NetworkBuffer()
		{
			count = 0;
			s_Buffer = new byte[Capacity];
			s_Semaphore = new SemaphoreSlim(1);
		}

		public static void Add(Span<byte> msg)
		{
			s_Semaphore.Wait();
			var dest = new Span<byte>(s_Buffer).Slice(count, s_Buffer.Length);
			msg.CopyTo(dest);
			count += msg.Length;
			s_Semaphore.Release();
		}

		public static unsafe void Add<T>(T msg) where T : unmanaged
		{
			byte* ptr = (byte*)&msg;
			Add(new Span<byte>(ptr, sizeof(T)));
		}

		public static int CopyAndClear(Span<byte> destination)
		{
			s_Semaphore.Wait();
			var bytes = Math.Min(destination.Length, count);
			var source = new Span<byte>(s_Buffer).Slice(0, bytes);
			source.CopyTo(destination);
			count = 0;
			s_Semaphore.Release();

			return bytes;
		}
	}
}

	



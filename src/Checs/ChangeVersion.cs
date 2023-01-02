using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal unsafe struct ChangeVersion : IDisposable
	{
		public uint value => *this.handle;

		public uint* handle;

		public ChangeVersion(uint initialValue)
		{
			this.handle = (uint*)Allocator.Alloc(sizeof(uint));
			*this.handle = initialValue;
		}

		public uint Increment()
		{
			// Sloppy increment on multi-threaded systems.
			// We do not care about the exact value. The only thing that
			// needs to be respected is that all versions of local changes
			// after a read are higher than the version that was read.
			return ++(*this.handle);
		}

		public uint Read()
		{
			return Interlocked.Add(ref Unsafe.AsRef<uint>(this.value), 0);
		}

		public void Dispose()
		{
			Allocator.Free(this.handle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool DidChange(uint actualVersion, uint changeVersion)
		{
			return (int)(actualVersion - changeVersion) > 0;
		}
	}
}
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Checs
{
	internal static class ThrowHelper
	{
		[DoesNotReturn]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ThrowArchetypeTooLargeExeception()
		{
			throw new ArgumentOutOfRangeException("Archetype sizes may not be larger than " + Chunk.BufferSize + " bytes.");
		}

		[DoesNotReturn]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static unsafe void ThrowWriteOnReadExeception(Archetype* archetype)
		{
			throw new InvalidOperationException(new EntityArchetype(archetype->index) + " was written while it was being read.");
		}

		[DoesNotReturn]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static unsafe void ThrowEntityMovedException()
		{
			throw new InvalidOperationException("Entity possibly moved or destroyed.");
		}
	}
}
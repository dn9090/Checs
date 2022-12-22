using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace Checs
{
	internal unsafe struct ChangeVersion
	{
		public uint entityVersion;

		public uint structuralVersion;

		public int componentVersion;

		public void Reset()
		{
			this.entityVersion = 1;
			this.structuralVersion = 1;
			this.componentVersion = 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasChanged(int version)
		{
			return (componentVersion - version) > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetCurrentChangeVersion()
		{
			return componentVersion - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MarkStructuralChange()
		{
			++this.structuralVersion;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CheckStructuralChange(uint version)
		{
			if(this.structuralVersion != version)
				throw new InvalidOperationException("Structural change detected. " + 
					"Use the EntityCommandBuffer to defer structural changes while iterating.");
		}
	}
}

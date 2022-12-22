using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Checs
{
	internal static unsafe class UnsafeUtility
	{
		public static void CloneMemory5(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var absoluteByteCount = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == absoluteByteCount)
					return;

				offset += byteCount;

				var src = source;
				var remaining = byteCount;

				while((remaining & -4) != 0)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while((offset & -Vector<byte>.Count) == 0); // & -Vector<byte>.Count was 2x slower.
			
			var stopLoopAtOffset = absoluteByteCount - Vector<byte>.Count;

			if((byteCount & -Vector<byte>.Count) != 0)
				goto CopyWindowed;

			var pattern = Unsafe.ReadUnaligned<Vector<byte>>(destination);
			var patternByteCount = offset - byteCount;
			
			while(offset <= stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, pattern);
				offset += patternByteCount;
				dst    += patternByteCount;
			}

			goto CopyRemaining;

		CopyWindowed:
			var window = destination;

			while(offset <= stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(window));

				window    += Vector<byte>.Count;
				dst       += Vector<byte>.Count;
				offset    += Vector<byte>.Count;
			}

		CopyRemaining:
			var rep = (offset / byteCount) * byteCount; // Closest pattern end.

			if(offset != absoluteByteCount)
			{
				// next line is the replacement for (buffer is the offset from destination before the loop above):
				// destination + buffer - Vector<byte>.Count
				var repEnd = destination + rep - Vector<byte>.Count;
				var dstEnd = destination + stopLoopAtOffset;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(repEnd));
			}
		}

		public static void CloneMemory4(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var absoluteByteCount = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == absoluteByteCount)
					return;

				offset += byteCount;

				var src = source;
				var remaining = byteCount;

				while((remaining & -4) != 0)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while((offset & (2 * -Vector<byte>.Count)) == 0); // & -Vector<byte>.Count was 2x slower.
			
			var stopLoopAtOffset = absoluteByteCount - Vector<byte>.Count;
			var from = destination;
			// var buffer = offset;

			while(offset <= stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(from));

				offset += Vector<byte>.Count;
				from   += Vector<byte>.Count;
				dst    += Vector<byte>.Count;
			}

			var rep = (offset / byteCount) * byteCount; // Closest pattern end.

			if(offset != absoluteByteCount)
			{
				// next line is the replacement for (buffer is the offset from destination before the loop above):
				// destination + buffer - Vector<byte>.Count
				var repEnd = destination + rep - Vector<byte>.Count;
				var dstEnd = destination + stopLoopAtOffset;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(repEnd));
			}
		}
		
		/*
		public static void CloneMemory4_____(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var absoluteByteCount = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == absoluteByteCount)
					return;

				offset += byteCount;

				var src = source;
				var remaining = byteCount;

				while((remaining & -4) != 0)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while((offset & -Vector<byte>.Count) == 0);
			
			var dstEnd = destination + absoluteByteCount - Vector<byte>.Count;
			var from = destination;

			// var buffer = offset;

			while(dst <= dstEnd)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(from));

				dst += Vector<byte>.Count;
				from += Vector<byte>.Count;
			}

			var pos = (int)(dst - destination); // Written bytes in destination.
			var rep = (pos / byteCount) * byteCount; // Closest pattern end.

			if(pos != 0)
			{
				// next line is the replacement for (buffer is the offset from destination before the loop above):
				// destination + buffer - Vector<byte>.Count
				var repEnd = destination + rep - Vector<byte>.Count;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(repEnd));
			}
		}*/

		public static void CloneMemory3(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var absoluteByteCount = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == absoluteByteCount)
					return;

				offset += byteCount;

				var src = source;
				var remaining = byteCount;

				while((remaining & -4) != 0)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while((offset & -Vector<byte>.Count) == 0);
			
			var stopLoopAtOffset = absoluteByteCount - Vector<byte>.Count;
			var buffer = offset;

			while(offset <= stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(destination + (offset % buffer)));

				offset += Vector<byte>.Count;
				dst += Vector<byte>.Count;
			}

			if(offset != absoluteByteCount)
			{
				var repEnd = destination + buffer - Vector<byte>.Count;
				var dstEnd = destination + stopLoopAtOffset;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(repEnd));
			}
		}

		/*
		public static void CloneMemory3____(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var bytes = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == bytes)
					return;

				var src = source;
				var remaining = byteCount;

				offset += byteCount;

				while(remaining >= 4)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while(offset < Vector<byte>.Count);
			
			var stopLoopAtOffset = bytes - Vector<byte>.Count;
			var from = destination;

			while(offset < stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(from));

				from += Vector<byte>.Count;
				dst += Vector<byte>.Count;
				offset += Vector<byte>.Count;
			}

			if(offset < bytes)
			{
				var tail = destination + ((offset / byteCount) * byteCount) - Vector<byte>.Count;
				var dstEnd = destination + stopLoopAtOffset;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(tail));
			}
		}*/

		public static void CloneMemory2(byte* destination, byte* source, int byteCount, int count)
		{
			if(count == 0)
				return;

			if(byteCount == 0)
				return;

			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var numElements = Vector<byte>.Count / byteCount;
			var numElementsByteCount = numElements * byteCount;

			var i = 0;
			var dst = destination;

			do
			{
				var remaining = byteCount;
				var src = source;

				while(remaining >= 4)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;

				++i;
				--count;
			} while(count != 0 && i < numElements);
			
			if(numElements > 0) // Skip byteCounts larger than Vector<byte>.Count.
			{
				var src = Unsafe.ReadUnaligned<Vector<byte>>(destination);
				
				while(count > numElements)
				{
					Unsafe.WriteUnaligned(dst, src);
					count -= numElements;
					dst += numElementsByteCount;
				}
			}

			while(count > 0)
			{
				Unsafe.CopyBlockUnaligned(dst, destination, (uint)byteCount);
				dst += byteCount;
				--count;
			}
		}

		public static void CloneMemory(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 0)
				return;
			
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var numElements = Vector<byte>.Count / byteCount;

			if(numElements == 0 || count <= numElements)
				goto CannotVectorize;
		
			Unsafe.SkipInit(out Vector<byte> vector); // can be destination
			var ptr = (byte*)&vector;
			var bytesInVector = numElements * byteCount;
							
			for(int i = 0; i < numElements; ++i)
			{
				Unsafe.CopyBlockUnaligned(ptr, source, (uint)byteCount);
				ptr += byteCount;
			}

			do
			{
				Unsafe.WriteUnaligned(destination, vector);
				count -= numElements;
				destination += bytesInVector;
			} while(count > numElements);
			
		CannotVectorize:
			RepeatCopyBlock(destination, source, byteCount, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RepeatCopyBlock(byte* destination, byte* source, int byteCount, int count)
		{
			while(count > 0)
			{
				Unsafe.CopyBlockUnaligned(destination, source, (uint)byteCount);
				destination += byteCount;
				--count;
			}
		}
	}
}

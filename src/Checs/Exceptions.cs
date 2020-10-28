using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[Serializable]
	public class ArchetypeTooLargeException : Exception
	{
		public ArchetypeTooLargeException(string message) 
			: base(message)
		{}

		public ArchetypeTooLargeException(string message, Exception innerException)
			: base (message, innerException)
		{}

		public ArchetypeTooLargeException(int blockSize)
			: this(ToMessage(blockSize))
		{}

		private static unsafe string ToMessage(int blockSize)
		{
			int size = (blockSize + sizeof(Entity));
			return 
				"The archetype is too large. The required size is "+ size +
				" bytes but the buffer has only a capacity of " + Chunk.BufferSize +
				" bytes.";
		}
	}
}

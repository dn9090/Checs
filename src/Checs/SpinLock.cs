using System;
using System.Threading;

namespace Checs
{
	internal unsafe struct SpinLock
	{
		public int _lock_;

		public void Aquire()
		{
			while(true)
			{
				if(Interlocked.CompareExchange(ref this._lock_, 1, 0) == 0)
					return;

				while(Volatile.Read(ref this._lock_) == 1)
					continue;

				// TODO: With .NET 7 use the X86Base.Pause intrinsic.
			}
		}

		public void Release()
		{
			Volatile.Write(ref this._lock_, 0);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public struct Managed<T>
	{
		public T value
		{
			get
			{
				if(this.m_NotNull)
					return ManagedStore.Get<T>(this.m_HashCode);
				
				return default;
			}
			set
			{
				if(value == null)
				{
					if(this.m_NotNull)
						ManagedStore.Destroy(this.m_HashCode);
					this.m_NotNull = false;
				} else {
					this.m_HashCode = ManagedStore.Store(value);
					this.m_NotNull = true;
				}
			}
		}

		private int m_HashCode;

		private bool m_NotNull;
	}
}
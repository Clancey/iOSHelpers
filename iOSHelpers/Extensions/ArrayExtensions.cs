using System;

namespace System
{
	public static class ArrayExtensions
	{
		public static int IndexOf(this Array array, object item)
		{
			return Array.IndexOf (array, item);
		}
	}
}


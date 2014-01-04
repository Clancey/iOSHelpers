using System;

namespace System
{
	public static class ArrayExtensions
	{
		public static int IndexOf(this Array array, object item)
		{
			return Array.IndexOf (array, item);
		}
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			Array.ForEach (array, action);
		}
	}
}


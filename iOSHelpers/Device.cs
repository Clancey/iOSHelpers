using System;
using Foundation;

namespace iOSHelpers
{
	public static class Device
	{
		static NSObject Invoker;
		public static void EnsureInvokedOnMainThread (Action action)
		{
			if (NSThread.Current.IsMainThread) {
				action ();
				return;
			}
			if (Invoker == null)
				Invoker = new NSObject ();
			Invoker.BeginInvokeOnMainThread (() => 
				action ()
			);
		}
	}
}


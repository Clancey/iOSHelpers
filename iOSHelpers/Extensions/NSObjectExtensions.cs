using System;
using Foundation;
using System.Collections.Generic;

namespace iOSHelpers
{
	public static class NSObjectExtensions
	{
		static ObserverHelper helper = new ObserverHelper ();
		public static void AddObserver(this NSObject obj, string key, Action action)
		{
			obj.AddObserver (helper, (NSString)key, NSKeyValueObservingOptions.New, IntPtr.Zero);
			helper.Add (obj,key, action);
		}

		public static void RemoveObserver(this NSObject obj,string key)
		{
			obj.RemoveObserver (helper, (NSString)key);
		}

		class ObserverHelper: NSObject
		{

			public Dictionary<NSObject,Dictionary<string,List<Action>>> Actions = new Dictionary<NSObject,Dictionary<string,List<Action>>>();
			public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
			{
				Console.WriteLine (keyPath);
				Dictionary<string,List<Action>> actions;
				if (!Actions.TryGetValue (ofObject, out actions))
					return;
				List<Action> foundActions;
				if (actions.TryGetValue (keyPath, out foundActions))
					foundActions.ForEach (x => x ());

			}
			public void Add(NSObject obj,string key, Action action)
			{
				Dictionary<string,List<Action>> actions;
				if (!Actions.TryGetValue (obj, out actions))
					Actions.Add (obj, actions = new Dictionary<string, List<Action>> ());
				List<Action> foundActions;
				if (actions.TryGetValue (key, out foundActions))
					foundActions.Add (action);
				else
					actions [key] = new List<Action>{ action };
			}
			public void Remove(NSObject obj,string key)
			{
				Dictionary<string,List<Action>> actions;
				if (!Actions.TryGetValue (obj, out actions))
					return;
				if (actions.ContainsKey (key))
					actions.Remove (key);
			}
		}

	}
}


using System;
using UIKit;

namespace UIKit
{
	public static class TableViewExtensions
	{
		public static T DequeueReusableCell<T> (this UITableView tv, string key) where T : UITableViewCell, new()
		{
			var cell = tv.DequeueReusableCell (key);
			if (cell is T)
				return (T)cell;
			return new T ();
		}
		public static T DequeueReusableCell<T>(this UITableView tv) where T : UITableViewCell, new() => tv.DequeueReusableCell<T>(typeof(T).Name);
		
	}
}


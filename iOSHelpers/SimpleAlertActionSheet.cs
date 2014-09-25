using System;
using System.Collections.Generic;
using System.Collections;
using UIKit;

namespace iOSHelpers
{
	public class SimpleAlertActionSheet: UIAlertController , IEnumerable
	{
		public SimpleAlertActionSheet(IntPtr handle) : base(handle)
		{

		}
		public SimpleAlertActionSheet () : base(UIAlertController.Create("","", UIAlertControllerStyle.ActionSheet).Handle )
		{

		}

		public void Add(string title, Action action)
		{
			var a = UIAlertAction.Create (title, UIAlertActionStyle.Default, (ac) => {
				if(action != null)
					action ();
			});
			AddAction (a);
		}

		public void Add(string title, UIAlertActionStyle style, Action action)
		{
			var a = UIAlertAction.Create (title, style, (ac) => {
				if(action != null)
					action ();
			});
			AddAction (a);
		}
	}
}


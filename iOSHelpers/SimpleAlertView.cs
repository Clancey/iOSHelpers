using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace iOSHelpers
{
	public class SimpleAlertView : UIAlertView
	{
		Dictionary<int,Action> dict = new Dictionary<int, Action>();
		Dictionary<int,UIColor> colors = new Dictionary<int, UIColor> ();
		public SimpleAlertView (string title, string message) 
		{
			Title = title;
			Message = message;

			Clicked += (object sender, UIButtonEventArgs e) => {
				Action a;
				if (dict.TryGetValue (e.ButtonIndex, out a) && a != null)
					a ();
			};
//			this.Presented += (object sender, EventArgs e) => {
//				foreach(UIButton b in Subviews.Where(x=> x is UIButton))
//				{
//					UIColor color;
//					if(!colors.TryGetValue(b.Tag -1,out color))
//						continue;
//					b.SetTitleColor(color, UIControlState.Normal);
//				}
//			};
//			WillPresent += (object sender, EventArgs e) => {
//
//				foreach(UIButton b in Subviews.Where(x=> x is UIButton))
//				{
//					UIColor color;
//					if(!colors.TryGetValue(b.Tag -1,out color))
//						continue;
//					b.SetTitleColor(color, UIControlState.Normal);
//				}
//			};
		}

		public int Add(string title, Action action)
		{
			var index = AddButton (title);
			dict.Add (index, action);
			return index;
		}

		public int Add(string title, UIColor color, Action action)
		{
			var index = AddButton (title);
			dict.Add (index, action);
			colors.Add(index,color);
			return index;
		}
	}
}


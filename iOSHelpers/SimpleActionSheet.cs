using System;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

namespace iOSHelpers
{
	public class SimpleActionSheet : UIActionSheet , IEnumerable
	{
		Dictionary<int,Action> dict = new Dictionary<int, Action>();
		Dictionary<int,UIColor> colors = new Dictionary<int, UIColor> ();
		public SimpleActionSheet ()
		{
			Clicked += async (object sender, UIButtonEventArgs e) => {
				//iOS8 doesnt let you present a new screen until the old one is gone, This fixes that issue.
				await Task.Delay(10);
				Action a;
				if (dict.TryGetValue (e.ButtonIndex, out a) && a != null)
					a ();
			};
			WillPresent += (object sender, EventArgs e) => {
				foreach(UIButton b in Subviews.Where(x=> x is UIButton))
				{
					UIColor color;
					if(!colors.TryGetValue(b.Tag -1,out color))
						continue;
					b.SetTitleColor(color, UIControlState.Normal);
				}
			};
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


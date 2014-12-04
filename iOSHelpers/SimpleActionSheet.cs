using System;
using System.Collections.Generic;
using UIKit;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Foundation;

namespace iOSHelpers
{
	public class SimpleActionSheet : UIActionSheet , IEnumerable
	{
		Dictionary<int,Action> dict = new Dictionary<int, Action>();
		Dictionary<int,UIColor> colors = new Dictionary<int, UIColor> ();
		public UIColor TextColor { get; set; }
		public SimpleActionSheet ()
		{
			TextColor = TintColor;
			Clicked += async (object sender, UIButtonEventArgs e) => {
				//iOS8 doesnt let you present a new screen until the old one is gone, This fixes that issue.
				await Task.Delay(10);
				Action a;
				if (dict.TryGetValue ((int)e.ButtonIndex, out a) && a != null)
					a ();
			};
			WillPresent += (object sender, EventArgs e) => {
				foreach(UIButton b in Subviews.Where(x=> x is UIButton))
				{
					UIColor color;
					if(!colors.TryGetValue((int)b.Tag -1,out color))
						color = TextColor;
					b.SetTitleColor(color, UIControlState.Normal);
				}
			};

			if (this.RespondsToSelector(new ObjCRuntime.Selector("_alertController")))
			{
				var alertController =  this.ValueForKey((NSString)"_alertController") as UIAlertController;
				if (alertController != null) {
					alertController.View.TintColor = TextColor;
				}

			}
			else
			{
				// use other methods for iOS 7 or older.
			}
		}

		public int Add(string title, Action action)
		{
			var index = (int)AddButton (title);
			dict.Add (index, action);
			return index;
		}

		public int Add(string title, UIColor color, Action action)
		{
			var index = (int)AddButton (title);
			dict.Add (index, action);
			colors.Add(index,color);
			return index;
		}
	}
}


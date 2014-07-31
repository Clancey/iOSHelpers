using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
				if(tcs != null)
					tcs.TrySetResult(e.ButtonIndex);
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
		public override void Show ()
		{
			if (this.ButtonCount == 0)
				this.Add ("Ok", null);
			base.Show ();
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
		TaskCompletionSource<int> tcs;
		public async Task<int> ClickedAsync()
		{
			tcs = new TaskCompletionSource<int> ();

			return await tcs.Task;
		}

		public string TextEntryValue
		{
			get{
				var tb = GetTextField (0);
				return tb != null ? tb.Text : "";
			}
		}
	}
}


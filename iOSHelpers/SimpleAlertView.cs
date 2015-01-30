using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iOSHelpers
{
	public class SimpleAlertView : UIAlertView
	{
		Dictionary<nint,Action> dict = new Dictionary<nint, Action>();
		Dictionary<nint,UIColor> colors = new Dictionary<nint, UIColor> ();
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

		public nint Add(string title, Action action)
		{
			var index = AddButton (title);
			dict.Add (index, action);
			return index;
		}

		public nint Add(string title, UIColor color, Action action)
		{
			var index = AddButton (title);
			dict.Add (index, action);
			colors.Add(index,color);
			return index;
		}
		TaskCompletionSource<nint> tcs;
		public async Task<nint> ClickedAsync()
		{
			tcs = new TaskCompletionSource<nint> ();

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


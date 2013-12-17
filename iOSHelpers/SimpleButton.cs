using System;
using MonoTouch.UIKit;

namespace iOSHelpers
{
	public class SimpleButton : UIButton
	{
		public SimpleButton ()
		{
			this.TouchUpInside += (object sender, EventArgs e) => {
				if (Tapped != null)
					Tapped (this);
			};
		}

		public string Title {
			get{ return this.CurrentTitle; }
			set {
				this.SetTitle (value, UIControlState.Normal);
				this.SizeToFit ();
			}
		}

		public Action<SimpleButton> Tapped { get; set; }
	}
}


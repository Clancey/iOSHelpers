using System;
using UIKit;

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

		public new string Title {
			get{ return this.CurrentTitle; }
			set {
				this.SetTitle (value, UIControlState.Normal);
				this.SizeToFit ();
			}
		}

		public new UIColor TitleColor
		{
			get{ return this.TitleColor (UIControlState.Normal); }
			set{ this.SetTitleColor (value, UIControlState.Normal); }
		}
		public UIColor TitleSelectedColor
		{
			get{ return this.TitleColor (UIControlState.Highlighted); }
			set { this.SetTitleColor (value, UIControlState.Highlighted); }
		}

		public UIImage Image
		{
			get{ return this.ImageForState (UIControlState.Normal);}
			set{ this.SetImage (value, UIControlState.Normal); }
		}

		public Action<SimpleButton> Tapped { get; set; }
	}
}

